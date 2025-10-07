using MilkChillar.Application.DTOs.AccountOpeningBalances;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Application;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Infrastructure.Services
{
    public class AccountOpeningBalanceService : IAccountOpeningBalanceService
    {
        private readonly ApplicationDbContext _context;

        public AccountOpeningBalanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AccountOpeningBalanceDto>> GetAllAsync(int tenantId)
        {
            return await _context.AccountOpeningBalances
                .Include(aob => aob.Account)
                .Include(aob => aob.User)
                .Include(aob => aob.JournalEntry)
                .Where(aob => aob.TenantId == tenantId)
                .Select(aob => new AccountOpeningBalanceDto
                {
                    OpeningBalanceId = aob.OpeningBalanceId,
                    TenantId = aob.TenantId,
                    AccountId = aob.AccountId,
                    OpeningDate = aob.OpeningDate,
                    DebitOpening = aob.DebitOpening,
                    CreditOpening = aob.CreditOpening,
                    Description = aob.Description,
                    AddedBy = aob.AddedBy,
                    JournalEntryId = aob.JournalEntryId,
                    CreatedAt = aob.CreatedAt,
                    AccountCode = aob.Account != null ? aob.Account.AccountCode : null,
                    AccountName = aob.Account != null ? aob.Account.Name : null,
                    AccountFullCode = aob.Account != null ? aob.Account.FullCode : null,
                    AddedByUsername = aob.User != null ? aob.User.Username : null,
                    JournalDescription = aob.JournalEntry != null ? aob.JournalEntry.Description : null
                })
                .OrderBy(aob => aob.AccountCode)
                .ThenBy(aob => aob.OpeningDate)
                .ToListAsync();
        }

        public async Task<PaginatedResult<AccountOpeningBalanceDto>> GetOpeningBalancesAsync(AccountOpeningBalanceQueryParameters query)
        {
            var openingBalancesQuery = _context.AccountOpeningBalances
                .Include(aob => aob.Account)
                .Include(aob => aob.User)
                .Include(aob => aob.JournalEntry)
                .Where(aob => aob.TenantId == query.TenantId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                openingBalancesQuery = openingBalancesQuery.Where(aob =>
                    (aob.Account != null && aob.Account.Name.ToLower().Contains(query.Search.ToLower())) ||
                    (aob.Account != null && aob.Account.AccountCode.Contains(query.Search)) ||
                    (aob.Description != null && aob.Description.Contains(query.Search))
                );
            }

            if (query.AccountId.HasValue)
                openingBalancesQuery = openingBalancesQuery.Where(aob => aob.AccountId == query.AccountId);

            if (query.FromDate.HasValue)
                openingBalancesQuery = openingBalancesQuery.Where(aob => aob.OpeningDate >= query.FromDate);

            if (query.ToDate.HasValue)
                openingBalancesQuery = openingBalancesQuery.Where(aob => aob.OpeningDate <= query.ToDate);

            if (query.HasDebitBalance.HasValue)
                openingBalancesQuery = openingBalancesQuery.Where(aob =>
                    query.HasDebitBalance.Value ? aob.DebitOpening > 0 : aob.DebitOpening == 0);

            if (query.HasCreditBalance.HasValue)
                openingBalancesQuery = openingBalancesQuery.Where(aob =>
                    query.HasCreditBalance.Value ? aob.CreditOpening > 0 : aob.CreditOpening == 0);

            if (query.MinAmount.HasValue)
                openingBalancesQuery = openingBalancesQuery.Where(aob =>
                    (aob.DebitOpening + aob.CreditOpening) >= query.MinAmount);

            if (query.MaxAmount.HasValue)
                openingBalancesQuery = openingBalancesQuery.Where(aob =>
                    (aob.DebitOpening + aob.CreditOpening) <= query.MaxAmount);

            var totalCount = await openingBalancesQuery.CountAsync();
            var skip = (query.PageNumber - 1) * query.PageSize;

            var openingBalances = await openingBalancesQuery
                .OrderBy(aob => aob.Account.AccountCode)
                .ThenBy(aob => aob.OpeningDate)
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            var openingBalanceDtos = openingBalances.Select(aob => new AccountOpeningBalanceDto
            {
                OpeningBalanceId = aob.OpeningBalanceId,
                TenantId = aob.TenantId,
                AccountId = aob.AccountId,
                OpeningDate = aob.OpeningDate,
                DebitOpening = aob.DebitOpening,
                CreditOpening = aob.CreditOpening,
                Description = aob.Description,
                AddedBy = aob.AddedBy,
                JournalEntryId = aob.JournalEntryId,
                CreatedAt = aob.CreatedAt,
                AccountCode = aob.Account?.AccountCode,
                AccountName = aob.Account?.Name,
                AccountFullCode = aob.Account?.FullCode,
                AddedByUsername = aob.User?.Username,
                JournalDescription = aob.JournalEntry?.Description
            }).ToList();

            return new PaginatedResult<AccountOpeningBalanceDto>
            {
                Items = openingBalanceDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<AccountOpeningBalanceDto?> GetByIdAsync(int id, int tenantId)
        {
            var aob = await _context.AccountOpeningBalances
                .Include(x => x.Account)
                .Include(x => x.User)
                .Include(x => x.JournalEntry)
                .FirstOrDefaultAsync(x => x.OpeningBalanceId == id && x.TenantId == tenantId);

            if (aob == null) return null;

            return new AccountOpeningBalanceDto
            {
                OpeningBalanceId = aob.OpeningBalanceId,
                TenantId = aob.TenantId,
                AccountId = aob.AccountId,
                OpeningDate = aob.OpeningDate,
                DebitOpening = aob.DebitOpening,
                CreditOpening = aob.CreditOpening,
                Description = aob.Description,
                AddedBy = aob.AddedBy,
                JournalEntryId = aob.JournalEntryId,
                CreatedAt = aob.CreatedAt,
                AccountCode = aob.Account?.AccountCode,
                AccountName = aob.Account?.Name,
                AccountFullCode = aob.Account?.FullCode,
                AddedByUsername = aob.User?.Username,
                JournalDescription = aob.JournalEntry?.Description
            };
        }

        public async Task<AccountOpeningBalanceDto> CreateAsync(CreateAccountOpeningBalanceDto dto, int tenantId, int userId)
        {
            // Validate that both debit and credit are not zero
            if (dto.DebitOpening == 0 && dto.CreditOpening == 0)
                throw new ArgumentException("Either debit opening or credit opening must be greater than zero.");

            // Check if opening balance already exists for this account and date
            var existing = await _context.AccountOpeningBalances
                .FirstOrDefaultAsync(aob => aob.TenantId == tenantId &&
                                          aob.AccountId == dto.AccountId &&
                                          aob.OpeningDate.Date == dto.OpeningDate.Date);

            if (existing != null)
                throw new InvalidOperationException($"Opening balance already exists for this account on {dto.OpeningDate:yyyy-MM-dd}.");

            var openingBalance = new AccountOpeningBalance
            {
                TenantId = tenantId,
                AccountId = dto.AccountId,
                OpeningDate = dto.OpeningDate,
                DebitOpening = dto.DebitOpening,
                CreditOpening = dto.CreditOpening,
                Description = dto.Description,
                AddedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AccountOpeningBalances.Add(openingBalance);
            await _context.SaveChangesAsync();

            // Load related data
            var account = await _context.Accounts.FindAsync(openingBalance.AccountId);
            var user = await _context.Users.FindAsync(openingBalance.AddedBy);
            var journalEntry = openingBalance.JournalEntryId.HasValue
                ? await _context.JournalEntries.FindAsync(openingBalance.JournalEntryId)
                : null;

            return new AccountOpeningBalanceDto
            {
                OpeningBalanceId = openingBalance.OpeningBalanceId,
                TenantId = openingBalance.TenantId,
                AccountId = openingBalance.AccountId,
                OpeningDate = openingBalance.OpeningDate,
                DebitOpening = openingBalance.DebitOpening,
                CreditOpening = openingBalance.CreditOpening,
                Description = openingBalance.Description,
                AddedBy = openingBalance.AddedBy,
                JournalEntryId = openingBalance.JournalEntryId,
                CreatedAt = openingBalance.CreatedAt,
                AccountCode = account?.AccountCode,
                AccountName = account?.Name,
                AccountFullCode = account?.FullCode,
                AddedByUsername = user?.Username,
                JournalDescription = journalEntry?.Description
            };
        }

        public async Task<AccountOpeningBalanceDto?> UpdateAsync(int id, UpdateAccountOpeningBalanceDto dto, int tenantId)
        {
            var openingBalance = await _context.AccountOpeningBalances
                .FirstOrDefaultAsync(aob => aob.OpeningBalanceId == id && aob.TenantId == tenantId);

            if (openingBalance == null) return null;

            // Validate that both debit and credit are not zero
            if (dto.DebitOpening == 0 && dto.CreditOpening == 0)
                throw new ArgumentException("Either debit opening or credit opening must be greater than zero.");

            // Check for duplicate if account or date changed
            if (openingBalance.AccountId != dto.AccountId || openingBalance.OpeningDate.Date != dto.OpeningDate.Date)
            {
                var existing = await _context.AccountOpeningBalances
                    .FirstOrDefaultAsync(aob => aob.TenantId == tenantId &&
                                              aob.AccountId == dto.AccountId &&
                                              aob.OpeningDate.Date == dto.OpeningDate.Date &&
                                              aob.OpeningBalanceId != id);

                if (existing != null)
                    throw new InvalidOperationException($"Opening balance already exists for this account on {dto.OpeningDate:yyyy-MM-dd}.");
            }

            openingBalance.AccountId = dto.AccountId;
            openingBalance.OpeningDate = dto.OpeningDate;
            openingBalance.DebitOpening = dto.DebitOpening;
            openingBalance.CreditOpening = dto.CreditOpening;
            openingBalance.Description = dto.Description;

            await _context.SaveChangesAsync();

            // Load related data
            var account = await _context.Accounts.FindAsync(openingBalance.AccountId);
            var user = await _context.Users.FindAsync(openingBalance.AddedBy);
            var journalEntry = openingBalance.JournalEntryId.HasValue
                ? await _context.JournalEntries.FindAsync(openingBalance.JournalEntryId)
                : null;

            return new AccountOpeningBalanceDto
            {
                OpeningBalanceId = openingBalance.OpeningBalanceId,
                TenantId = openingBalance.TenantId,
                AccountId = openingBalance.AccountId,
                OpeningDate = openingBalance.OpeningDate,
                DebitOpening = openingBalance.DebitOpening,
                CreditOpening = openingBalance.CreditOpening,
                Description = openingBalance.Description,
                AddedBy = openingBalance.AddedBy,
                JournalEntryId = openingBalance.JournalEntryId,
                CreatedAt = openingBalance.CreatedAt,
                AccountCode = account?.AccountCode,
                AccountName = account?.Name,
                AccountFullCode = account?.FullCode,
                AddedByUsername = user?.Username,
                JournalDescription = journalEntry?.Description
            };
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var openingBalance = await _context.AccountOpeningBalances
                .FirstOrDefaultAsync(aob => aob.OpeningBalanceId == id && aob.TenantId == tenantId);

            if (openingBalance == null) return false;

            _context.AccountOpeningBalances.Remove(openingBalance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AccountOpeningBalanceDto>> GetByAccountIdAsync(int accountId, int tenantId)
        {
            return await _context.AccountOpeningBalances
                .Include(aob => aob.Account)
                .Include(aob => aob.User)
                .Include(aob => aob.JournalEntry)
                .Where(aob => aob.AccountId == accountId && aob.TenantId == tenantId)
                .Select(aob => new AccountOpeningBalanceDto
                {
                    OpeningBalanceId = aob.OpeningBalanceId,
                    TenantId = aob.TenantId,
                    AccountId = aob.AccountId,
                    OpeningDate = aob.OpeningDate,
                    DebitOpening = aob.DebitOpening,
                    CreditOpening = aob.CreditOpening,
                    Description = aob.Description,
                    AddedBy = aob.AddedBy,
                    JournalEntryId = aob.JournalEntryId,
                    CreatedAt = aob.CreatedAt,
                    AccountCode = aob.Account != null ? aob.Account.AccountCode : null,
                    AccountName = aob.Account != null ? aob.Account.Name : null,
                    AccountFullCode = aob.Account != null ? aob.Account.FullCode : null,
                    AddedByUsername = aob.User != null ? aob.User.Username : null,
                    JournalDescription = aob.JournalEntry != null ? aob.JournalEntry.Description : null
                })
                .OrderBy(aob => aob.OpeningDate)
                .ToListAsync();
        }

        public async Task<AccountOpeningBalanceDto?> GetByAccountAndDateAsync(int accountId, DateTime openingDate, int tenantId)
        {
            var aob = await _context.AccountOpeningBalances
                .Include(x => x.Account)
                .Include(x => x.User)
                .Include(x => x.JournalEntry)
                .FirstOrDefaultAsync(x => x.AccountId == accountId &&
                                        x.OpeningDate.Date == openingDate.Date &&
                                        x.TenantId == tenantId);

            if (aob == null) return null;

            return new AccountOpeningBalanceDto
            {
                OpeningBalanceId = aob.OpeningBalanceId,
                TenantId = aob.TenantId,
                AccountId = aob.AccountId,
                OpeningDate = aob.OpeningDate,
                DebitOpening = aob.DebitOpening,
                CreditOpening = aob.CreditOpening,
                Description = aob.Description,
                AddedBy = aob.AddedBy,
                JournalEntryId = aob.JournalEntryId,
                CreatedAt = aob.CreatedAt,
                AccountCode = aob.Account?.AccountCode,
                AccountName = aob.Account?.Name,
                AccountFullCode = aob.Account?.FullCode,
                AddedByUsername = aob.User?.Username,
                JournalDescription = aob.JournalEntry?.Description
            };
        }
    }
}
