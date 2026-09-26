using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;
using MilkChillar.Application.DTOs.FinancialYear;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;

namespace MilkChillar.Infrastructure.Services
{
    public class FinancialYearService : IFinancialYearService
    {
        private readonly ApplicationDbContext _context;

        public FinancialYearService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static DateTime NormalizeToUtc(DateTime dt)
        {
            return DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);
        }

        public async Task<IEnumerable<FinancialYearDto>> GetAllAsync(int tenantId)
        {
            await EnsureCurrentFinancialYearExistsAsync(tenantId);

            var years = await _context.FinancialYears
                .Include(f => f.ClosedByUser)
                .Include(f => f.RetainedEarningsAccount)
                .Where(f => f.TenantId == tenantId)
                .OrderByDescending(f => f.StartDate)
                .ToListAsync();

            return years.Select(MapToDto);
        }

        public async Task<FinancialYearDto?> GetByIdAsync(int id, int tenantId)
        {
            var fy = await _context.FinancialYears
                .Include(f => f.ClosedByUser)
                .Include(f => f.RetainedEarningsAccount)
                .FirstOrDefaultAsync(f => f.FinancialYearId == id && f.TenantId == tenantId);

            return fy == null ? null : MapToDto(fy);
        }

        public async Task<FinancialYearDto?> GetActiveAsync(int tenantId)
        {
            await EnsureCurrentFinancialYearExistsAsync(tenantId);

            var fy = await _context.FinancialYears
                .Include(f => f.ClosedByUser)
                .Include(f => f.RetainedEarningsAccount)
                .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.IsActive);

            return fy == null ? null : MapToDto(fy);
        }

        public async Task<FinancialYearDto> CreateAsync(CreateFinancialYearDto dto, int tenantId)
        {
            var startDateUtc = NormalizeToUtc(dto.StartDate);
            var endDateUtc = NormalizeToUtc(dto.EndDate);

            if (startDateUtc >= endDateUtc)
            {
                throw new ArgumentException("Start date must be earlier than End date.");
            }

            // Check duplicate name
            var exists = await _context.FinancialYears
                .AnyAsync(f => f.TenantId == tenantId && f.Name.ToLower() == dto.Name.Trim().ToLower());

            if (exists)
            {
                throw new InvalidOperationException($"A Financial Year with the name '{dto.Name}' already exists.");
            }

            // If setting as active, deactivate existing active
            if (dto.SetAsActive)
            {
                var activeYears = await _context.FinancialYears
                    .Where(f => f.TenantId == tenantId && f.IsActive)
                    .ToListAsync();

                foreach (var y in activeYears)
                {
                    y.IsActive = false;
                    y.UpdatedAt = DateTime.UtcNow;
                }
            }

            var entity = new FinancialYear
            {
                TenantId = tenantId,
                Name = dto.Name.Trim(),
                Code = string.IsNullOrWhiteSpace(dto.Code) ? dto.Name.Trim() : dto.Code.Trim(),
                StartDate = startDateUtc,
                EndDate = endDateUtc,
                IsActive = dto.SetAsActive,
                IsClosed = false,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FinancialYears.Add(entity);
            await _context.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<FinancialYearDto?> UpdateAsync(int id, UpdateFinancialYearDto dto, int tenantId)
        {
            var fy = await _context.FinancialYears
                .FirstOrDefaultAsync(f => f.FinancialYearId == id && f.TenantId == tenantId);

            if (fy == null) return null;

            if (fy.IsClosed)
            {
                throw new InvalidOperationException("Cannot modify a closed Financial Year.");
            }

            var startDateUtc = NormalizeToUtc(dto.StartDate);
            var endDateUtc = NormalizeToUtc(dto.EndDate);

            if (startDateUtc >= endDateUtc)
            {
                throw new ArgumentException("Start date must be earlier than End date.");
            }

            fy.Name = dto.Name.Trim();
            fy.Code = string.IsNullOrWhiteSpace(dto.Code) ? dto.Name.Trim() : dto.Code.Trim();
            fy.StartDate = startDateUtc;
            fy.EndDate = endDateUtc;
            fy.Notes = dto.Notes;
            fy.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(fy);
        }

        public async Task<bool> SetActiveAsync(int id, int tenantId)
        {
            var target = await _context.FinancialYears
                .FirstOrDefaultAsync(f => f.FinancialYearId == id && f.TenantId == tenantId);

            if (target == null) return false;

            var activeYears = await _context.FinancialYears
                .Where(f => f.TenantId == tenantId && f.IsActive)
                .ToListAsync();

            foreach (var y in activeYears)
            {
                y.IsActive = false;
                y.UpdatedAt = DateTime.UtcNow;
            }

            target.IsActive = true;
            target.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CloseFinancialYearPreviewDto> GetClosingPreviewAsync(int id, int tenantId)
        {
            var fy = await _context.FinancialYears
                .FirstOrDefaultAsync(f => f.FinancialYearId == id && f.TenantId == tenantId);

            if (fy == null)
            {
                throw new KeyNotFoundException("Financial Year not found.");
            }

            // Journal lines within this financial year period
            var startUtc = NormalizeToUtc(fy.StartDate);
            var endUtc = NormalizeToUtc(fy.EndDate).AddDays(1).AddTicks(-1);

            var journalLines = await _context.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Include(jel => jel.Account)
                    .ThenInclude(a => a.SubAccount)
                        .ThenInclude(sa => sa.MainAccount)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                              jel.JournalEntry.EntryDate >= startUtc &&
                              jel.JournalEntry.EntryDate <= endUtc)
                .ToListAsync();

            // P&L Accounts (Revenues & Expenses)
            // Revenue: MainAccountCode starts with 4 or FinancialStatementComponent == "Revenue"
            var revenueLines = journalLines.Where(l => IsRevenueAccount(l.Account));
            decimal totalRevenue = revenueLines.Sum(l => l.Credit - l.Debit);

            // Expense: MainAccountCode starts with 5, 6, 7 or CostOfSales / OperatingExpenses
            var expenseLines = journalLines.Where(l => IsExpenseAccount(l.Account));
            decimal totalExpense = expenseLines.Sum(l => l.Debit - l.Credit);

            decimal netProfitLoss = totalRevenue - totalExpense;

            // Balance Sheet accounts to roll forward: Assets (100s), Liabilities (200s), Equity (300s)
            var balanceSheetAccounts = await _context.Accounts
                .Include(a => a.SubAccount)
                    .ThenInclude(sa => sa.MainAccount)
                .Where(a => a.TenantId == tenantId)
                .ToListAsync();

            var accountsToRoll = new List<RollForwardAccountItemDto>();

            // Calculate closing balance as of EndDate
            foreach (var acc in balanceSheetAccounts.Where(a => IsBalanceSheetAccount(a)))
            {
                var allLinesForAcc = await _context.JournalEntryLines
                    .Include(jel => jel.JournalEntry)
                    .Where(jel => jel.AccountId == acc.AccountId &&
                                  jel.JournalEntry.TenantId == tenantId &&
                                  jel.JournalEntry.EntryDate <= endUtc)
                    .ToListAsync();

                decimal totalDebits = allLinesForAcc.Sum(l => l.Debit);
                decimal totalCredits = allLinesForAcc.Sum(l => l.Credit);
                decimal netBalance = totalDebits - totalCredits;

                if (Math.Abs(netBalance) > 0.001m)
                {
                    decimal newDebit = netBalance > 0 ? netBalance : 0m;
                    decimal newCredit = netBalance < 0 ? -netBalance : 0m;

                    accountsToRoll.Add(new RollForwardAccountItemDto
                    {
                        AccountId = acc.AccountId,
                        AccountCode = acc.AccountCode,
                        AccountName = acc.Name,
                        AccountType = GetAccountCategory(acc),
                        DebitTotal = totalDebits,
                        CreditTotal = totalCredits,
                        ClosingBalance = netBalance,
                        NewDebitOpening = Math.Round(newDebit, 2),
                        NewCreditOpening = Math.Round(newCredit, 2)
                    });
                }
            }

            return new CloseFinancialYearPreviewDto
            {
                FinancialYearId = fy.FinancialYearId,
                FinancialYearName = fy.Name,
                StartDate = fy.StartDate,
                EndDate = fy.EndDate,
                TotalRevenue = Math.Round(totalRevenue, 2),
                TotalExpense = Math.Round(totalExpense, 2),
                NetProfitLoss = Math.Round(netProfitLoss, 2),
                AccountsToRollForwardCount = accountsToRoll.Count,
                RollForwardAccounts = accountsToRoll.OrderBy(a => a.AccountCode).ToList()
            };
        }

        public async Task<FinancialYearCloseResultDto> CloseFinancialYearAsync(int id, CloseFinancialYearDto dto, int tenantId, int userId)
        {
            var fy = await _context.FinancialYears
                .FirstOrDefaultAsync(f => f.FinancialYearId == id && f.TenantId == tenantId);

            if (fy == null)
            {
                throw new KeyNotFoundException("Financial Year not found.");
            }

            if (fy.IsClosed)
            {
                throw new InvalidOperationException("This Financial Year is already closed.");
            }

            // Verify Retained Earnings account
            var retainedEarningsAcc = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == dto.RetainedEarningsAccountId && a.TenantId == tenantId);

            if (retainedEarningsAcc == null)
            {
                throw new ArgumentException("Retained earnings equity account is required for year-end close.");
            }

            // Resolve or create next financial year
            FinancialYear nextFy;
            if (dto.NextFinancialYearId.HasValue)
            {
                nextFy = await _context.FinancialYears
                    .FirstOrDefaultAsync(f => f.FinancialYearId == dto.NextFinancialYearId.Value && f.TenantId == tenantId)
                    ?? throw new ArgumentException("Specified next financial year not found.");
            }
            else
            {
                // Auto compute next year dates (day after current EndDate for 1 year)
                var nextStart = NormalizeToUtc(fy.EndDate.AddDays(1));
                var nextEnd = NormalizeToUtc(nextStart.AddYears(1).AddDays(-1));
                var nextName = !string.IsNullOrWhiteSpace(dto.NextYearName) 
                    ? dto.NextYearName.Trim() 
                    : $"FY {nextStart:yyyy}-{nextEnd:yyyy}";
                var nextCode = !string.IsNullOrWhiteSpace(dto.NextYearCode)
                    ? dto.NextYearCode.Trim()
                    : $"FY{nextStart:yy}-{nextEnd:yy}";

                nextFy = await _context.FinancialYears
                    .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Name == nextName);

                if (nextFy == null)
                {
                    nextFy = new FinancialYear
                    {
                        TenantId = tenantId,
                        Name = nextName,
                        Code = nextCode,
                        StartDate = dto.NextYearStartDate.HasValue ? NormalizeToUtc(dto.NextYearStartDate.Value) : nextStart,
                        EndDate = dto.NextYearEndDate.HasValue ? NormalizeToUtc(dto.NextYearEndDate.Value) : nextEnd,
                        IsActive = false,
                        IsClosed = false,
                        Notes = "Created automatically during year-end close.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.FinancialYears.Add(nextFy);
                    await _context.SaveChangesAsync();
                }
            }

            var startUtc = NormalizeToUtc(fy.StartDate);
            var endUtc = NormalizeToUtc(fy.EndDate).AddDays(1).AddTicks(-1);

            // Step 1: Calculate Net Profit / Loss & Generate Closing Journal Entry
            var journalLines = await _context.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Include(jel => jel.Account)
                    .ThenInclude(a => a.SubAccount)
                        .ThenInclude(sa => sa.MainAccount)
                .Where(jel => jel.JournalEntry.TenantId == tenantId &&
                              jel.JournalEntry.EntryDate >= startUtc &&
                              jel.JournalEntry.EntryDate <= endUtc)
                .ToListAsync();

            var revenueLines = journalLines.Where(l => IsRevenueAccount(l.Account)).ToList();
            var expenseLines = journalLines.Where(l => IsExpenseAccount(l.Account)).ToList();

            decimal totalRevenue = revenueLines.Sum(l => l.Credit - l.Debit);
            decimal totalExpense = expenseLines.Sum(l => l.Debit - l.Credit);
            decimal netProfitLoss = totalRevenue - totalExpense;

            JournalEntry? closingJe = null;

            // Only create closing JE if there is revenue, expense, or net profit/loss
            if (revenueLines.Count > 0 || expenseLines.Count > 0 || Math.Abs(netProfitLoss) > 0.001m)
            {
                closingJe = new JournalEntry
                {
                    TenantId = tenantId,
                    EntryDate = NormalizeToUtc(fy.EndDate),
                    SourceTable = "financial_years",
                    SourceId = fy.FinancialYearId,
                    ReferenceNo = $"CLOSE-{fy.Code}",
                    Description = $"Year-End Closing Entry for {fy.Name}",
                    AddedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                // Close each revenue account to zero (Debit revenue accounts)
                var revGroups = revenueLines.GroupBy(l => l.AccountId);
                foreach (var grp in revGroups)
                {
                    decimal netRev = grp.Sum(l => l.Credit - l.Debit);
                    if (Math.Abs(netRev) > 0.001m)
                    {
                        closingJe.JournalEntryLines.Add(new JournalEntryLine
                        {
                            AccountId = grp.Key,
                            Debit = netRev > 0 ? netRev : 0m,
                            Credit = netRev < 0 ? -netRev : 0m,
                            Narration = $"Closing Revenue account to P&L ({fy.Name})"
                        });
                    }
                }

                // Close each expense account to zero (Credit expense accounts)
                var expGroups = expenseLines.GroupBy(l => l.AccountId);
                foreach (var grp in expGroups)
                {
                    decimal netExp = grp.Sum(l => l.Debit - l.Credit);
                    if (Math.Abs(netExp) > 0.001m)
                    {
                        closingJe.JournalEntryLines.Add(new JournalEntryLine
                        {
                            AccountId = grp.Key,
                            Debit = netExp < 0 ? -netExp : 0m,
                            Credit = netExp > 0 ? netExp : 0m,
                            Narration = $"Closing Expense account to P&L ({fy.Name})"
                        });
                    }
                }

                // Transfer Net Profit / Loss to Retained Earnings
                if (netProfitLoss > 0)
                {
                    // Profit: Credit Retained Earnings
                    closingJe.JournalEntryLines.Add(new JournalEntryLine
                    {
                        AccountId = retainedEarningsAcc.AccountId,
                        Debit = 0m,
                        Credit = netProfitLoss,
                        Narration = $"Transfer Net Profit for {fy.Name} to Retained Earnings"
                    });
                }
                else if (netProfitLoss < 0)
                {
                    // Loss: Debit Retained Earnings
                    closingJe.JournalEntryLines.Add(new JournalEntryLine
                    {
                        AccountId = retainedEarningsAcc.AccountId,
                        Debit = -netProfitLoss,
                        Credit = 0m,
                        Narration = $"Transfer Net Loss for {fy.Name} to Retained Earnings"
                    });
                }

                _context.JournalEntries.Add(closingJe);
                await _context.SaveChangesAsync();
            }

            // Step 2: Roll forward Balance Sheet accounts into Next FY as Opening Balances
            var preview = await GetClosingPreviewAsync(id, tenantId);
            int rolledCount = 0;

            var nextStartUtc = NormalizeToUtc(nextFy.StartDate);

            // Remove any existing opening balance in nextFy that might conflict
            var existingNextOpening = await _context.AccountOpeningBalances
                .Where(a => a.TenantId == tenantId && a.OpeningDate.Date == nextStartUtc.Date)
                .ToListAsync();

            if (existingNextOpening.Count > 0)
            {
                _context.AccountOpeningBalances.RemoveRange(existingNextOpening);
                await _context.SaveChangesAsync();
            }

            foreach (var acc in preview.RollForwardAccounts)
            {
                if (acc.NewDebitOpening > 0 || acc.NewCreditOpening > 0)
                {
                    var aob = new AccountOpeningBalance
                    {
                        TenantId = tenantId,
                        AccountId = acc.AccountId,
                        OpeningDate = nextStartUtc,
                        DebitOpening = acc.NewDebitOpening,
                        CreditOpening = acc.NewCreditOpening,
                        Description = $"B/F from {fy.Name} Year-End Close",
                        AddedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AccountOpeningBalances.Add(aob);
                    rolledCount++;
                }
            }

            await _context.SaveChangesAsync();

            // Step 3: Lock Old Financial Year and Activate Next Financial Year
            fy.IsClosed = true;
            fy.IsActive = false;
            fy.ClosedAt = DateTime.UtcNow;
            fy.ClosedBy = userId;
            fy.ClosingJournalEntryId = closingJe?.JournalEntryId;
            fy.RetainedEarningsAccountId = retainedEarningsAcc.AccountId;
            fy.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? fy.Notes : dto.Notes;
            fy.UpdatedAt = DateTime.UtcNow;

            nextFy.IsActive = true;
            nextFy.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new FinancialYearCloseResultDto
            {
                Success = true,
                Message = $"Successfully closed {fy.Name} and activated {nextFy.Name}.",
                ClosedFinancialYearId = fy.FinancialYearId,
                ActiveFinancialYearId = nextFy.FinancialYearId,
                ClosingJournalEntryId = closingJe?.JournalEntryId,
                RolledForwardBalancesCount = rolledCount,
                NetProfitLoss = Math.Round(netProfitLoss, 2)
            };
        }

        public async Task<bool> ReopenFinancialYearAsync(int id, int tenantId, int userId)
        {
            var fy = await _context.FinancialYears
                .FirstOrDefaultAsync(f => f.FinancialYearId == id && f.TenantId == tenantId);

            if (fy == null || !fy.IsClosed) return false;

            // Remove closing journal entry if exists
            if (fy.ClosingJournalEntryId.HasValue)
            {
                var jeId = fy.ClosingJournalEntryId.Value;
                fy.ClosingJournalEntryId = null;
                await _context.SaveChangesAsync();

                var jeLines = await _context.JournalEntryLines
                    .Where(l => l.JournalEntryId == jeId)
                    .ToListAsync();
                _context.JournalEntryLines.RemoveRange(jeLines);

                var je = await _context.JournalEntries.FindAsync(jeId);
                if (je != null)
                {
                    _context.JournalEntries.Remove(je);
                }
            }

            fy.IsClosed = false;
            fy.ClosedAt = null;
            fy.ClosedBy = null;
            fy.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsDateInClosedYearAsync(DateTime date, int tenantId)
        {
            var dateUtc = NormalizeToUtc(date);
            return await _context.FinancialYears
                .AnyAsync(f => f.TenantId == tenantId &&
                               f.IsClosed &&
                               f.StartDate <= dateUtc &&
                               f.EndDate >= dateUtc);
        }

        private async Task EnsureTableExistsAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS public.financial_years (
                        financial_year_id SERIAL PRIMARY KEY,
                        tenant_id INT NOT NULL REFERENCES public.tenants(tenant_id) ON DELETE CASCADE,
                        name VARCHAR(50) NOT NULL,
                        code VARCHAR(20) NOT NULL,
                        start_date TIMESTAMP WITH TIME ZONE NOT NULL,
                        end_date TIMESTAMP WITH TIME ZONE NOT NULL,
                        is_active BOOLEAN NOT NULL DEFAULT FALSE,
                        is_closed BOOLEAN NOT NULL DEFAULT FALSE,
                        closed_at TIMESTAMP WITH TIME ZONE NULL,
                        closed_by INT NULL REFERENCES public.users(user_id) ON DELETE SET NULL,
                        closing_journal_entry_id INT NULL REFERENCES public.journal_entries(journal_entry_id) ON DELETE SET NULL,
                        retained_earnings_account_id INT NULL REFERENCES public.accounts(account_id) ON DELETE SET NULL,
                        notes VARCHAR(255) NULL,
                        created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                        updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
                    );
                    CREATE INDEX IF NOT EXISTS idx_financial_years_tenant_active ON public.financial_years(tenant_id, is_active);
                    CREATE INDEX IF NOT EXISTS idx_financial_years_dates ON public.financial_years(tenant_id, start_date, end_date);
                ");
            }
            catch
            {
                // Table already exists or permission restricted
            }
        }

        public async Task<FinancialYearDto> EnsureCurrentFinancialYearExistsAsync(int tenantId)
        {
            await EnsureTableExistsAsync();

            var existing = await _context.FinancialYears
                .Where(f => f.TenantId == tenantId)
                .OrderByDescending(f => f.StartDate)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                // Ensure at least one year is marked active if none is
                if (!await _context.FinancialYears.AnyAsync(f => f.TenantId == tenantId && f.IsActive))
                {
                    existing.IsActive = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return MapToDto(existing);
            }

            // Derive sensible defaults for current fiscal year (July 1 - June 30 standard)
            var now = DateTime.UtcNow;
            DateTime fyStart;
            DateTime fyEnd;

            if (now.Month >= 7)
            {
                fyStart = new DateTime(now.Year, 7, 1, 0, 0, 0, DateTimeKind.Utc);
                fyEnd = new DateTime(now.Year + 1, 6, 30, 23, 59, 59, DateTimeKind.Utc);
            }
            else
            {
                fyStart = new DateTime(now.Year - 1, 7, 1, 0, 0, 0, DateTimeKind.Utc);
                fyEnd = new DateTime(now.Year, 6, 30, 23, 59, 59, DateTimeKind.Utc);
            }

            var defaultName = $"FY {fyStart:yyyy}-{fyEnd:yyyy}";
            var defaultCode = $"FY{fyStart:yy}-{fyEnd:yy}";

            var defaultFy = new FinancialYear
            {
                TenantId = tenantId,
                Name = defaultName,
                Code = defaultCode,
                StartDate = fyStart,
                EndDate = fyEnd,
                IsActive = true,
                IsClosed = false,
                Notes = "Primary operational financial year.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FinancialYears.Add(defaultFy);
            await _context.SaveChangesAsync();

            return MapToDto(defaultFy);
        }

        private static bool IsRevenueAccount(Account? account)
        {
            if (account == null) return false;
            var mainCode = account.SubAccount?.MainAccount?.MainAccountCode ?? "";
            var component = account.SubAccount?.MainAccount?.FinancialStatementComponent ?? "";
            return mainCode.StartsWith("4") || component.Equals("Revenue", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsExpenseAccount(Account? account)
        {
            if (account == null) return false;
            var mainCode = account.SubAccount?.MainAccount?.MainAccountCode ?? "";
            var component = account.SubAccount?.MainAccount?.FinancialStatementComponent ?? "";
            return mainCode.StartsWith("5") || mainCode.StartsWith("6") || mainCode.StartsWith("7") ||
                   component.Equals("CostOfSales", StringComparison.OrdinalIgnoreCase) ||
                   component.Equals("OperatingExpenses", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBalanceSheetAccount(Account? account)
        {
            if (account == null) return false;
            return !IsRevenueAccount(account) && !IsExpenseAccount(account);
        }

        private static string GetAccountCategory(Account? account)
        {
            if (account == null) return "Other";
            var mainCode = account.SubAccount?.MainAccount?.MainAccountCode ?? "";
            var component = account.SubAccount?.MainAccount?.FinancialStatementComponent ?? "";

            if (mainCode.StartsWith("1") || component.Contains("Asset", StringComparison.OrdinalIgnoreCase))
                return "Asset";
            if (mainCode.StartsWith("2") || component.Contains("Liabilit", StringComparison.OrdinalIgnoreCase))
                return "Liability";
            if (mainCode.StartsWith("3") || component.Contains("Equity", StringComparison.OrdinalIgnoreCase))
                return "Equity";

            return "Balance Sheet";
        }

        private static FinancialYearDto MapToDto(FinancialYear f)
        {
            return new FinancialYearDto
            {
                FinancialYearId = f.FinancialYearId,
                TenantId = f.TenantId,
                Name = f.Name,
                Code = f.Code,
                StartDate = f.StartDate,
                EndDate = f.EndDate,
                IsActive = f.IsActive,
                IsClosed = f.IsClosed,
                ClosedAt = f.ClosedAt,
                ClosedBy = f.ClosedBy,
                ClosedByUsername = f.ClosedByUser?.Username,
                ClosingJournalEntryId = f.ClosingJournalEntryId,
                RetainedEarningsAccountId = f.RetainedEarningsAccountId,
                RetainedEarningsAccountName = f.RetainedEarningsAccount?.Name,
                Notes = f.Notes,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            };
        }
    }
}
