//using MilkChillar.Application.DTOs.AccountLedger;
//using MilkChillar.Application.Interfaces;
//using MilkChillar.Application.Parameters;
//using MilkChillar.Application.Responses;
//using MilkChillar.Application;
//using Microsoft.EntityFrameworkCore;


//namespace MilkChillar.Infrastructure.Services
//{
//    public class AccountLedgerService : IAccountLedgerService
//    {
//        private readonly ApplicationDbContext _context;

//        public AccountLedgerService(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<PaginatedResult<AccountLedgerDto>> GetAccountLedgerAsync(AccountLedgerQueryParameters query)
//        {
//            var ledgerQuery = from jel in _context.JournalEntryLines
//                              join je in _context.JournalEntries on jel.JournalEntryId equals je.JournalEntryId
//                              join account in _context.Accounts on jel.AccountId equals account.AccountId
//                              where account.TenantId == query.TenantId &&
//                                    jel.AccountId == query.AccountId
//                              select new
//                              {
//                                  jel.JournalLineId,
//                                  jel.JournalEntryId,
//                                  jel.AccountId,
//                                  account.AccountCode,
//                                  account.Name,
//                                  je.EntryDate,
//                                  je.ReferenceNo,
//                                  je.Description,
//                                  jel.Narration,
//                                  jel.Debit,
//                                  jel.Credit,
//                                  je.SourceTable,
//                                  je.SourceId
//                              };

//            // Apply date filters
//            if (query.FromDate.HasValue)
//            {
//                ledgerQuery = ledgerQuery.Where(x => x.EntryDate >= query.FromDate.Value);
//            }

//            if (query.ToDate.HasValue)
//            {
//                ledgerQuery = ledgerQuery.Where(x => x.EntryDate <= query.ToDate.Value);
//            }

//            // Apply search filter
//            if (!string.IsNullOrWhiteSpace(query.Search))
//            {
//                ledgerQuery = ledgerQuery.Where(x =>
//                    (x.Description != null && x.Description.Contains(query.Search)) ||
//                    (x.Narration != null && x.Narration.Contains(query.Search)) ||
//                    (x.ReferenceNo != null && x.ReferenceNo.Contains(query.Search))
//                );
//            }

//            // Apply source table filter
//            if (!string.IsNullOrWhiteSpace(query.SourceTable))
//            {
//                ledgerQuery = ledgerQuery.Where(x => x.SourceTable == query.SourceTable);
//            }

//            // Filter zero transactions if needed
//            if (!query.IncludeZeroTransactions)
//            {
//                ledgerQuery = ledgerQuery.Where(x => x.Debit != 0 || x.Credit != 0);
//            }

//            var totalCount = await ledgerQuery.CountAsync();

//            // Order by date and get paginated results
//            var orderedQuery = ledgerQuery.OrderBy(x => x.EntryDate).ThenBy(x => x.JournalEntryId);

//            var skip = (query.PageNumber - 1) * query.PageSize;
//            var items = await orderedQuery
//                .Skip(skip)
//                .Take(query.PageSize)
//                .ToListAsync();

//            // Calculate running balance
//            var runningBalance = 0m;

//            // Get opening balance (balance before FromDate if specified)
//            if (query.FromDate.HasValue)
//            {
//                runningBalance = await GetAccountBalanceAsync(query.AccountId, query.TenantId, query.FromDate.Value.AddDays(-1));
//            }

//            var resultDtos = items.Select(x => new AccountLedgerDto
//            {
//                JournalLineId = x.JournalLineId,
//                JournalEntryId = x.JournalEntryId,
//                AccountId = x.AccountId,
//                AccountCode = x.AccountCode,
//                AccountName = x.Name,
//                EntryDate = x.EntryDate,
//                ReferenceNo = x.ReferenceNo,
//                Description = x.Description,
//                Narration = x.Narration,
//                Debit = x.Debit,
//                Credit = x.Credit,
//                RunningBalance = runningBalance += (x.Debit - x.Credit),
//                SourceTable = x.SourceTable,
//                SourceId = x.SourceId
//            }).ToList();

//            return new PaginatedResult<AccountLedgerDto>
//            {
//                Items = resultDtos,
//                TotalCount = totalCount,
//                PageNumber = query.PageNumber,
//                PageSize = query.PageSize
//            };
//        }

//        public async Task<AccountLedgerSummaryDto?> GetAccountLedgerSummaryAsync(int accountId, int tenantId, DateTime? fromDate = null, DateTime? toDate = null)
//        {
//            var account = await _context.Accounts
//                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.TenantId == tenantId);

//            if (account == null) return null;

//            var ledgerQuery = from jel in _context.JournalEntryLines
//                              join je in _context.JournalEntries on jel.JournalEntryId equals je.JournalEntryId
//                              where jel.AccountId == accountId && je.TenantId == tenantId
//                              select new { jel, je };

//            if (fromDate.HasValue)
//            {
//                ledgerQuery = ledgerQuery.Where(x => x.je.EntryDate >= fromDate.Value);
//            }

//            if (toDate.HasValue)
//            {
//                ledgerQuery = ledgerQuery.Where(x => x.je.EntryDate <= toDate.Value);
//            }

//            var ledgerData = await ledgerQuery.ToListAsync();

//            var totalDebits = ledgerData.Sum(x => x.jel.Debit);
//            var totalCredits = ledgerData.Sum(x => x.jel.Credit);
//            var openingBalance = fromDate.HasValue
//                ? await GetAccountBalanceAsync(accountId, tenantId, fromDate.Value.AddDays(-1))
//                : 0m;

//            return new AccountLedgerSummaryDto
//            {
//                AccountId = accountId,
//                AccountCode = account.AccountCode,
//                AccountName = account.Name,
//                OpeningBalance = openingBalance,
//                TotalDebits = totalDebits,
//                TotalCredits = totalCredits,
//                ClosingBalance = openingBalance + totalDebits - totalCredits,
//                TransactionCount = ledgerData.Count,
//                FirstTransactionDate = ledgerData.OrderBy(x => x.je.EntryDate).FirstOrDefault()?.je.EntryDate,
//                LastTransactionDate = ledgerData.OrderByDescending(x => x.je.EntryDate).FirstOrDefault()?.je.EntryDate
//            };
//        }

//        public async Task<PaginatedResult<AccountLedgerDto>> GetMultipleAccountLedgerAsync(MultipleAccountLedgerQueryParameters query)
//        {
//            var ledgerQuery = from jel in _context.JournalEntryLines
//                              join je in _context.JournalEntries on jel.JournalEntryId equals je.JournalEntryId
//                              join account in _context.Accounts on jel.AccountId equals account.AccountId
//                              where account.TenantId == query.TenantId
//                              select new
//                              {
//                                  jel.JournalLineId,
//                                  jel.JournalEntryId,
//                                  jel.AccountId,
//                                  account.AccountCode,
//                                  account.Name,
//                                  je.EntryDate,
//                                  je.ReferenceNo,
//                                  je.Description,
//                                  jel.Narration,
//                                  jel.Debit,
//                                  jel.Credit,
//                                  je.SourceTable,
//                                  je.SourceId
//                              };

//            // Apply account filters
//            if (query.AccountIds != null && query.AccountIds.Any())
//            {
//                ledgerQuery = ledgerQuery.Where(x => query.AccountIds.Contains(x.AccountId));
//            }

//            if (!string.IsNullOrWhiteSpace(query.AccountCodePrefix))
//            {
//                ledgerQuery = ledgerQuery.Where(x => x.AccountCode.StartsWith(query.AccountCodePrefix));
//            }

//            // Apply date filters
//            if (query.FromDate.HasValue)
//            {
//                ledgerQuery = ledgerQuery.Where(x => x.EntryDate >= query.FromDate.Value);
//            }

//            if (query.ToDate.HasValue)
//            {
//                ledgerQuery = ledgerQuery.Where(x => x.EntryDate <= query.ToDate.Value);
//            }

//            // Apply search filter
//            if (!string.IsNullOrWhiteSpace(query.Search))
//            {
//                ledgerQuery = ledgerQuery.Where(x =>
//                    (x.Description != null && x.Description.Contains(query.Search)) ||
//                    (x.Narration != null && x.Narration.Contains(query.Search)) ||
//                    (x.ReferenceNo != null && x.ReferenceNo.Contains(query.Search)) ||
//                    x.Name.Contains(query.Search) ||
//                    x.AccountCode.Contains(query.Search)
//                );
//            }

//            var totalCount = await ledgerQuery.CountAsync();
//            var skip = (query.PageNumber - 1) * query.PageSize;

//            var items = await ledgerQuery
//                .OrderBy(x => x.AccountCode)
//                .ThenBy(x => x.EntryDate)
//                .ThenBy(x => x.JournalEntryId)
//                .Skip(skip)
//                .Take(query.PageSize)
//                .ToListAsync();

//            var resultDtos = items.Select(x => new AccountLedgerDto
//            {
//                JournalLineId = x.JournalLineId,
//                JournalEntryId = x.JournalEntryId,
//                AccountId = x.AccountId,
//                AccountCode = x.AccountCode,
//                AccountName = x.Name,
//                EntryDate = x.EntryDate,
//                ReferenceNo = x.ReferenceNo,
//                Description = x.Description,
//                Narration = x.Narration,
//                Debit = x.Debit,
//                Credit = x.Credit,
//                RunningBalance = 0, // Running balance calculation would be complex for multiple accounts
//                SourceTable = x.SourceTable,
//                SourceId = x.SourceId
//            }).ToList();

//            return new PaginatedResult<AccountLedgerDto>
//            {
//                Items = resultDtos,
//                TotalCount = totalCount,
//                PageNumber = query.PageNumber,
//                PageSize = query.PageSize
//            };
//        }

//        public async Task<decimal> GetAccountBalanceAsync(int accountId, int tenantId, DateTime? asOfDate = null)
//        {
//            var query = from jel in _context.JournalEntryLines
//                        join je in _context.JournalEntries on jel.JournalEntryId equals je.JournalEntryId
//                        where jel.AccountId == accountId && je.TenantId == tenantId
//                        select new { jel.Debit, jel.Credit, je.EntryDate };

//            if (asOfDate.HasValue)
//            {
//                query = query.Where(x => x.EntryDate <= asOfDate.Value);
//            }

//            var transactions = await query.ToListAsync();
//            return transactions.Sum(x => x.Debit - x.Credit);
//        }

//        public async Task<IEnumerable<AccountLedgerSummaryDto>> GetAllAccountBalancesAsync(int tenantId, string? accountCodePrefix = null)
//        {
//            var accountsQuery = _context.Accounts.Where(a => a.TenantId == tenantId);

//            if (!string.IsNullOrWhiteSpace(accountCodePrefix))
//            {
//                accountsQuery = accountsQuery.Where(a => a.AccountCode.StartsWith(accountCodePrefix));
//            }

//            var accounts = await accountsQuery.ToListAsync();
//            var results = new List<AccountLedgerSummaryDto>();

//            foreach (var account in accounts)
//            {
//                var summary = await GetAccountLedgerSummaryAsync(account.AccountId, tenantId);
//                if (summary != null)
//                {
//                    results.Add(summary);
//                }
//            }

//            return results.OrderBy(x => x.AccountCode);
//        }
//    }
//}


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
            // Build optimized query with only necessary includes
            var ledgerQuery = _context.JournalEntryLines
                .Where(jel => jel.Account.TenantId == query.TenantId && jel.AccountId == query.AccountId)
                .Select(jel => new
                {
                    jel.JournalLineId,
                    jel.JournalEntryId,
                    jel.AccountId,
                    AccountCode = jel.Account.AccountCode,
                    AccountName = jel.Account.Name,
                    jel.JournalEntry.EntryDate,
                    jel.JournalEntry.ReferenceNo,
                    jel.JournalEntry.Description,
                    jel.Narration,
                    jel.Debit,
                    jel.Credit,
                    jel.JournalEntry.SourceTable,
                    jel.JournalEntry.SourceId
                });

            // Apply date filters (pushed to database)
            if (query.FromDate.HasValue)
            {
                ledgerQuery = ledgerQuery.Where(x => x.EntryDate >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                ledgerQuery = ledgerQuery.Where(x => x.EntryDate <= query.ToDate.Value);
            }

            // Apply search filter (pushed to database)
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchLower = query.Search.ToLower();
                ledgerQuery = ledgerQuery.Where(x =>
                    (x.Description != null && x.Description.ToLower().Contains(searchLower)) ||
                    (x.Narration != null && x.Narration.ToLower().Contains(searchLower)) ||
                    (x.ReferenceNo != null && x.ReferenceNo.ToLower().Contains(searchLower))
                );
            }

            // Apply source table filter (pushed to database)
            if (!string.IsNullOrWhiteSpace(query.SourceTable))
            {
                ledgerQuery = ledgerQuery.Where(x => x.SourceTable == query.SourceTable);
            }

            // Filter zero transactions (pushed to database)
            if (!query.IncludeZeroTransactions)
            {
                ledgerQuery = ledgerQuery.Where(x => x.Debit != 0 || x.Credit != 0);
            }

            // Execute query once and get data
            var allData = await ledgerQuery
                .OrderBy(x => x.EntryDate)
                .ThenBy(x => x.JournalEntryId)
                .AsNoTracking() // Optimize for read-only
                .ToListAsync();

            // Process data based on grouping preference
            List<AccountLedgerDto> processedData;

            if (query.GroupPurchasesByPeriod)
            {
                // Separate purchases/sales from other transactions
                var purchaseSales = allData.Where(x =>
                    x.SourceTable != null &&
                    (x.SourceTable.Equals("purchase", StringComparison.OrdinalIgnoreCase) ||
                     x.SourceTable.Equals("sales", StringComparison.OrdinalIgnoreCase))
                ).ToList();

                var otherTransactions = allData.Where(x =>
                    x.SourceTable == null ||
                    (!x.SourceTable.Equals("purchase", StringComparison.OrdinalIgnoreCase) &&
                     !x.SourceTable.Equals("sales", StringComparison.OrdinalIgnoreCase))
                ).ToList();

                // Group purchases/sales
                var groupedPurchaseSales = GroupTransactionsByPeriod(purchaseSales);

                // Convert other transactions to DTOs
                var otherDtos = otherTransactions.Select(x => new AccountLedgerDto
                {
                    JournalLineId = x.JournalLineId,
                    JournalEntryId = x.JournalEntryId,
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    EntryDate = x.EntryDate,
                    ReferenceNo = x.ReferenceNo,
                    Description = x.Description,
                    Narration = x.Narration,
                    Debit = x.Debit,
                    Credit = x.Credit,
                    RunningBalance = 0,
                    SourceTable = x.SourceTable,
                    SourceId = x.SourceId,
                    IsGrouped = false
                }).ToList();

                // Combine and sort by date
                processedData = groupedPurchaseSales
                    .Concat(otherDtos)
                    .OrderBy(x => x.EntryDate)
                    .ThenBy(x => x.JournalEntryId)
                    .ToList();
            }
            else
            {
                // No grouping - convert all to DTOs
                processedData = allData.Select(x => new AccountLedgerDto
                {
                    JournalLineId = x.JournalLineId,
                    JournalEntryId = x.JournalEntryId,
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    EntryDate = x.EntryDate,
                    ReferenceNo = x.ReferenceNo,
                    Description = x.Description,
                    Narration = x.Narration,
                    Debit = x.Debit,
                    Credit = x.Credit,
                    RunningBalance = 0,
                    SourceTable = x.SourceTable,
                    SourceId = x.SourceId,
                    IsGrouped = false
                }).ToList();
            }

            // Calculate running balance for all entries
            var runningBalance = 0m;
            if (query.FromDate.HasValue)
            {
                runningBalance = await GetAccountBalanceAsync(query.AccountId, query.TenantId, query.FromDate.Value.AddDays(-1));
            }

            foreach (var item in processedData)
            {
                runningBalance += (item.Debit - item.Credit);
                item.RunningBalance = runningBalance;
            }

            // Apply pagination
            var totalCount = processedData.Count;
            var skip = (query.PageNumber - 1) * query.PageSize;
            var paginatedData = processedData.Skip(skip).Take(query.PageSize).ToList();

            return new PaginatedResult<AccountLedgerDto>
            {
                Items = paginatedData,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private List<AccountLedgerDto> GroupTransactionsByPeriod<T>(List<T> transactions) where T : class
        {
            if (!transactions.Any()) return new List<AccountLedgerDto>();

            // Extract properties using reflection once
            var grouped = transactions
                .Select(x => new
                {
                    EntryDate = (DateTime)x.GetType().GetProperty("EntryDate")!.GetValue(x)!,
                    JournalLineId = (int)x.GetType().GetProperty("JournalLineId")!.GetValue(x)!,
                    JournalEntryId = (int)x.GetType().GetProperty("JournalEntryId")!.GetValue(x)!,
                    AccountId = (int)x.GetType().GetProperty("AccountId")!.GetValue(x)!,
                    AccountCode = (string)x.GetType().GetProperty("AccountCode")!.GetValue(x)!,
                    AccountName = (string)x.GetType().GetProperty("AccountName")!.GetValue(x)!,
                    Debit = (decimal)x.GetType().GetProperty("Debit")!.GetValue(x)!,
                    Credit = (decimal)x.GetType().GetProperty("Credit")!.GetValue(x)!,
                    SourceTable = (string?)x.GetType().GetProperty("SourceTable")!.GetValue(x)
                })
                .GroupBy(x => new
                {
                    Year = x.EntryDate.Year,
                    Month = x.EntryDate.Month,
                    Period = x.EntryDate.Day <= 15 ? 1 : 2, // 1st half (1-15) or 2nd half (16-end)
                    SourceTable = x.SourceTable
                })
                .Where(g => g.Count() > 0) // Only include periods with transactions
                .Select(group =>
                {
                    var firstDate = group.Min(x => x.EntryDate);
                    var lastDate = group.Max(x => x.EntryDate);
                    var totalDebit = group.Sum(x => x.Debit);
                    var totalCredit = group.Sum(x => x.Credit);
                    var firstItem = group.First();
                    var sourceTable = group.Key.SourceTable ?? "Unknown";

                    var periodStart = new DateTime(group.Key.Year, group.Key.Month, group.Key.Period == 1 ? 1 : 16);
                    var periodEnd = group.Key.Period == 1
                        ? new DateTime(group.Key.Year, group.Key.Month, 15)
                        : new DateTime(group.Key.Year, group.Key.Month, DateTime.DaysInMonth(group.Key.Year, group.Key.Month));

                    return new AccountLedgerDto
                    {
                        JournalLineId = 0, // Grouped entry
                        JournalEntryId = 0, // Grouped entry
                        AccountId = firstItem.AccountId,
                        AccountCode = firstItem.AccountCode,
                        AccountName = firstItem.AccountName,
                        EntryDate = firstDate, // Use first transaction date in the period
                        ReferenceNo = $"{periodStart:dd MMM} - {periodEnd:dd MMM yyyy}",
                        Description = $"{sourceTable} Summary ({group.Count()} entries)",
                        Narration = $"Period: {periodStart:dd/MM/yyyy} to {periodEnd:dd/MM/yyyy}",
                        Debit = totalDebit,
                        Credit = totalCredit,
                        RunningBalance = 0, // Will be calculated later
                        SourceTable = $"{sourceTable}_Grouped",
                        SourceId = null,
                        IsGrouped = true,
                        GroupedTransactionCount = group.Count(),
                        PeriodStart = periodStart,
                        PeriodEnd = periodEnd
                    };
                })
                .OrderBy(x => x.EntryDate)
                .ToList();

            return grouped;
        }

        public async Task<MilkCardDto?> GetMilkCardAsync(MilkCardQueryParameters query)
        {
            // Get account details
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == query.AccountId && a.TenantId == query.TenantId);

            if (account == null) return null;

            // Determine period start and end
            var periodStart = new DateOnly(query.Date.Year, query.Date.Month, query.Date.Day <= 15 ? 1 : 16);
            var periodEnd = query.Date.Day <= 15
                ? new DateOnly(query.Date.Year, query.Date.Month, 15)
                : new DateOnly(query.Date.Year, query.Date.Month, DateTime.DaysInMonth(query.Date.Year, query.Date.Month));

            List<MilkCardLineDto> milkCardLines;

            if (query.TransactionType == "Purchase")
            {
                // Get purchases grouped by date and time of day
                var purchases = await _context.Purchases
                    .Where(p => p.TenantId == query.TenantId &&
                                p.AccountId == query.AccountId &&
                                p.Date >= periodStart &&
                                p.Date <= periodEnd)
                    .OrderBy(p => p.Date)
                    .ThenBy(p => p.TimeOfDay)
                    .ToListAsync();

                if (!purchases.Any()) return null;

                // Group by date to combine morning and evening
                milkCardLines = purchases
                    .GroupBy(p => p.Date)
                    .Select(g =>
                    {
                        var morningEntry = g.FirstOrDefault(p => p.TimeOfDay.ToLower() == "morning");
                        var eveningEntry = g.FirstOrDefault(p => p.TimeOfDay.ToLower() == "evening");

                        var morningQty = morningEntry?.GrossLiters ?? 0;
                        var morningRate = morningEntry?.Rate ?? 0;
                        var morningAmount = morningEntry?.TotalAmount ?? 0;
                        var morningId = morningEntry?.PurchaseId ?? 0;

                        var eveningQty = eveningEntry?.GrossLiters ?? 0;
                        var eveningRate = eveningEntry?.Rate ?? 0;
                        var eveningAmount = eveningEntry?.TotalAmount ?? 0;
                        var eveningId = eveningEntry?.PurchaseId ?? 0;

                        return new MilkCardLineDto
                        {
                            Date = g.Key.ToDateTime(TimeOnly.MinValue),
                            MorningQuantity = morningQty,
                            MorningRate = morningRate,
                            MorningAmount = morningAmount,
                            MorningTransactionId = morningId,
                            EveningQuantity = eveningQty,
                            EveningRate = eveningRate,
                            EveningAmount = eveningAmount,
                            EveningTransactionId = eveningId,
                            TotalQuantity = morningQty + eveningQty,
                            TotalAmount = morningAmount + eveningAmount,
                            Remarks = null
                        };
                    })
                    .ToList();
            }
            else // Sales
            {
                var sales = await _context.Sales
                    .Where(s => s.TenantId == query.TenantId &&
                                s.AccountId == query.AccountId &&
                                s.Date >= periodStart &&
                                s.Date <= periodEnd)
                    .OrderBy(s => s.Date)
                    .ToListAsync();

                if (!sales.Any()) return null;

                milkCardLines = sales.Select(s => new MilkCardLineDto
                {
                    Date = s.Date.ToDateTime(TimeOnly.MinValue),
                    MorningQuantity = 0,
                    MorningRate = 0,
                    MorningAmount = 0,
                    MorningTransactionId = 0,
                    EveningQuantity = s.NetLiters,
                    EveningRate = s.Rate,
                    EveningAmount = s.TotalAmount,
                    EveningTransactionId = s.SaleId,
                    TotalQuantity = s.NetLiters,
                    TotalAmount = s.TotalAmount,
                    Remarks = null
                }).ToList();
            }

            // Calculate totals
            var totalMorningQuantity = milkCardLines.Sum(l => l.MorningQuantity);
            var totalMorningAmount = milkCardLines.Sum(l => l.MorningAmount);
            var totalEveningQuantity = milkCardLines.Sum(l => l.EveningQuantity);
            var totalEveningAmount = milkCardLines.Sum(l => l.EveningAmount);
            var grandTotalQuantity = milkCardLines.Sum(l => l.TotalQuantity);
            var grandTotalAmount = milkCardLines.Sum(l => l.TotalAmount);

            // Calculate average rates
            var avgMorningRate = totalMorningQuantity > 0 ? totalMorningAmount / totalMorningQuantity : 0;
            var avgEveningRate = totalEveningQuantity > 0 ? totalEveningAmount / totalEveningQuantity : 0;
            var avgTotalRate = grandTotalQuantity > 0 ? grandTotalAmount / grandTotalQuantity : 0;

            return new MilkCardDto
            {
                AccountId = query.AccountId,
                AccountCode = account.AccountCode,
                AccountName = account.Name,
                TransactionType = query.TransactionType,
                PeriodStart = periodStart.ToDateTime(TimeOnly.MinValue),
                PeriodEnd = periodEnd.ToDateTime(TimeOnly.MinValue),
                PeriodLabel = $"{periodStart:dd MMM} - {periodEnd:dd MMM yyyy}",
                Lines = milkCardLines,
                TotalMorningQuantity = totalMorningQuantity,
                TotalMorningAmount = totalMorningAmount,
                AverageMorningRate = Math.Round(avgMorningRate, 2),
                TotalEveningQuantity = totalEveningQuantity,
                TotalEveningAmount = totalEveningAmount,
                AverageEveningRate = Math.Round(avgEveningRate, 2),
                GrandTotalQuantity = grandTotalQuantity,
                GrandTotalAmount = grandTotalAmount,
                AverageTotalRate = Math.Round(avgTotalRate, 2),
                TransactionCount = milkCardLines.Count
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
                RunningBalance = 0,
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