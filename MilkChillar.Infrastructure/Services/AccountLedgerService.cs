using MilkChillar.Application.DTOs.AccountLedger;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Application;
using Microsoft.EntityFrameworkCore;


namespace MilkChillar.Infrastructure.Services
{
    public class AccountLedgerService : IAccountLedgerService
    {
        private readonly ApplicationDbContext _context;

        public AccountLedgerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<AccountLedgerDto>> GetAccountLedgerAsync(AccountLedgerQueryParameters query)
        {
            var ledgerQuery = from jel in _context.JournalEntryLines
                              join je in _context.JournalEntries on jel.JournalEntryId equals je.JournalEntryId
                              join account in _context.Accounts on jel.AccountId equals account.AccountId
                              where account.TenantId == query.TenantId &&
                                    jel.AccountId == query.AccountId
                              select new
                              {
                                  jel.JournalLineId,
                                  jel.JournalEntryId,
                                  jel.AccountId,
                                  account.AccountCode,
                                  account.Name,
                                  je.EntryDate,
                                  je.ReferenceNo,
                                  je.Description,
                                  jel.Narration,
                                  jel.Debit,
                                  jel.Credit,
                                  je.SourceTable,
                                  je.SourceId
                              };

            // Apply date filters
            if (query.FromDate.HasValue)
            {
                ledgerQuery = ledgerQuery.Where(x => x.EntryDate >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                ledgerQuery = ledgerQuery.Where(x => x.EntryDate <= query.ToDate.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                ledgerQuery = ledgerQuery.Where(x =>
                    (x.Description != null && x.Description.Contains(query.Search)) ||
                    (x.Narration != null && x.Narration.Contains(query.Search)) ||
                    (x.ReferenceNo != null && x.ReferenceNo.Contains(query.Search))
                );
            }

            // Apply source table filter
            if (!string.IsNullOrWhiteSpace(query.SourceTable))
            {
                ledgerQuery = ledgerQuery.Where(x => x.SourceTable == query.SourceTable);
            }

            // Filter zero transactions if needed
            if (!query.IncludeZeroTransactions)
            {
                ledgerQuery = ledgerQuery.Where(x => x.Debit != 0 || x.Credit != 0);
            }

            var totalCount = await ledgerQuery.CountAsync();

            // Order by date and get paginated results
            var orderedQuery = ledgerQuery.OrderBy(x => x.EntryDate).ThenBy(x => x.JournalEntryId);

            var skip = (query.PageNumber - 1) * query.PageSize;
            var items = await orderedQuery
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            // Calculate running balance
            var runningBalance = 0m;

            // Get opening balance (balance before FromDate if specified)
            if (query.FromDate.HasValue)
            {
                runningBalance = await GetAccountBalanceAsync(query.AccountId, query.TenantId, query.FromDate.Value.AddDays(-1));
            }

            var resultDtos = items.Select(x => new AccountLedgerDto
            {
                JournalLineId = x.JournalLineId,
                JournalEntryId = x.JournalEntryId,
                AccountId = x.AccountId,
                AccountCode = x.AccountCode,
                AccountName = x.Name,
                EntryDate = x.EntryDate,
                ReferenceNo = x.ReferenceNo,
                Description = x.Description,
                Narration = x.Narration,
                Debit = x.Debit,
                Credit = x.Credit,
                RunningBalance = runningBalance += (x.Debit - x.Credit),
                SourceTable = x.SourceTable,
                SourceId = x.SourceId
            }).ToList();

            return new PaginatedResult<AccountLedgerDto>
            {
                Items = resultDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<AccountLedgerSummaryDto?> GetAccountLedgerSummaryAsync(int accountId, int tenantId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.TenantId == tenantId);

            if (account == null) return null;

            var ledgerQuery = from jel in _context.JournalEntryLines
                              join je in _context.JournalEntries on jel.JournalEntryId equals je.JournalEntryId
                              where jel.AccountId == accountId && je.TenantId == tenantId
                              select new { jel, je };

            if (fromDate.HasValue)
            {
                ledgerQuery = ledgerQuery.Where(x => x.je.EntryDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                ledgerQuery = ledgerQuery.Where(x => x.je.EntryDate <= toDate.Value);
            }

            var ledgerData = await ledgerQuery.ToListAsync();

            var totalDebits = ledgerData.Sum(x => x.jel.Debit);
            var totalCredits = ledgerData.Sum(x => x.jel.Credit);
            var openingBalance = fromDate.HasValue
                ? await GetAccountBalanceAsync(accountId, tenantId, fromDate.Value.AddDays(-1))
                : 0m;

            return new AccountLedgerSummaryDto
            {
                AccountId = accountId,
                AccountCode = account.AccountCode,
                AccountName = account.Name,
                OpeningBalance = openingBalance,
                TotalDebits = totalDebits,
                TotalCredits = totalCredits,
                ClosingBalance = openingBalance + totalDebits - totalCredits,
                TransactionCount = ledgerData.Count,
                FirstTransactionDate = ledgerData.OrderBy(x => x.je.EntryDate).FirstOrDefault()?.je.EntryDate,
                LastTransactionDate = ledgerData.OrderByDescending(x => x.je.EntryDate).FirstOrDefault()?.je.EntryDate
            };
        }

        public async Task<PaginatedResult<AccountLedgerDto>> GetMultipleAccountLedgerAsync(MultipleAccountLedgerQueryParameters query)
        {
            var ledgerQuery = from jel in _context.JournalEntryLines
                              join je in _context.JournalEntries on jel.JournalEntryId equals je.JournalEntryId
                              join account in _context.Accounts on jel.AccountId equals account.AccountId
                              where account.TenantId == query.TenantId
                              select new
                              {
                                  jel.JournalLineId,
                                  jel.JournalEntryId,
                                  jel.AccountId,
                                  account.AccountCode,
                                  account.Name,
                                  je.EntryDate,
                                  je.ReferenceNo,
                                  je.Description,
                                  jel.Narration,
                                  jel.Debit,
                                  jel.Credit,
                                  je.SourceTable,
                                  je.SourceId
                              };

            // Apply account filters
            if (query.AccountIds != null && query.AccountIds.Any())
            {
                ledgerQuery = ledgerQuery.Where(x => query.AccountIds.Contains(x.AccountId));
            }

            if (!string.IsNullOrWhiteSpace(query.AccountCodePrefix))
            {
                ledgerQuery = ledgerQuery.Where(x => x.AccountCode.StartsWith(query.AccountCodePrefix));
            }

            // Apply date filters
            if (query.FromDate.HasValue)
            {
                ledgerQuery = ledgerQuery.Where(x => x.EntryDate >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                ledgerQuery = ledgerQuery.Where(x => x.EntryDate <= query.ToDate.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                ledgerQuery = ledgerQuery.Where(x =>
                    (x.Description != null && x.Description.Contains(query.Search)) ||
                    (x.Narration != null && x.Narration.Contains(query.Search)) ||
                    (x.ReferenceNo != null && x.ReferenceNo.Contains(query.Search)) ||
                    x.Name.Contains(query.Search) ||
                    x.AccountCode.Contains(query.Search)
                );
            }

            var totalCount = await ledgerQuery.CountAsync();
            var skip = (query.PageNumber - 1) * query.PageSize;

            var items = await ledgerQuery
                .OrderBy(x => x.AccountCode)
                .ThenBy(x => x.EntryDate)
                .ThenBy(x => x.JournalEntryId)
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            var resultDtos = items.Select(x => new AccountLedgerDto
            {
                JournalLineId = x.JournalLineId,
                JournalEntryId = x.JournalEntryId,
                AccountId = x.AccountId,
                AccountCode = x.AccountCode,
                AccountName = x.Name,
                EntryDate = x.EntryDate,
                ReferenceNo = x.ReferenceNo,
                Description = x.Description,
                Narration = x.Narration,
                Debit = x.Debit,
                Credit = x.Credit,
                RunningBalance = 0, // Running balance calculation would be complex for multiple accounts
                SourceTable = x.SourceTable,
                SourceId = x.SourceId
            }).ToList();

            return new PaginatedResult<AccountLedgerDto>
            {
                Items = resultDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<decimal> GetAccountBalanceAsync(int accountId, int tenantId, DateTime? asOfDate = null)
        {
            var query = from jel in _context.JournalEntryLines
                        join je in _context.JournalEntries on jel.JournalEntryId equals je.JournalEntryId
                        where jel.AccountId == accountId && je.TenantId == tenantId
                        select new { jel.Debit, jel.Credit, je.EntryDate };

            if (asOfDate.HasValue)
            {
                query = query.Where(x => x.EntryDate <= asOfDate.Value);
            }

            var transactions = await query.ToListAsync();
            return transactions.Sum(x => x.Debit - x.Credit);
        }

        public async Task<IEnumerable<AccountLedgerSummaryDto>> GetAllAccountBalancesAsync(int tenantId, string? accountCodePrefix = null)
        {
            var accountsQuery = _context.Accounts.Where(a => a.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(accountCodePrefix))
            {
                accountsQuery = accountsQuery.Where(a => a.AccountCode.StartsWith(accountCodePrefix));
            }

            var accounts = await accountsQuery.ToListAsync();
            var results = new List<AccountLedgerSummaryDto>();

            foreach (var account in accounts)
            {
                var summary = await GetAccountLedgerSummaryAsync(account.AccountId, tenantId);
                if (summary != null)
                {
                    results.Add(summary);
                }
            }

            return results.OrderBy(x => x.AccountCode);
        }
    }
}
