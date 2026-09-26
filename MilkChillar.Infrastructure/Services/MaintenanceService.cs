using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;
using MilkChillar.Application.DTOs.Maintenance;

namespace MilkChillar.Infrastructure.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> MassUpdateSupplierRatesByDodhi(int dodhiId, decimal newRate, int tenantId)
        {
            var suppliers = await _context.Suppliers
                .Where(s => s.DodhiId == dodhiId && s.TenantId == tenantId)
                .ToListAsync();
            foreach (var supplier in suppliers)
                supplier.Rate = newRate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RateSummaryDto> GetSupplierRateSummaryForPeriod(int accountId, DateOnly startDate, DateOnly endDate, int tenantId)
        {
            var purchases = await _context.Purchases
                .Where(p => p.AccountId == accountId && p.TenantId == tenantId && p.Date >= startDate && p.Date <= endDate)
                .ToListAsync();

            //var previousRate = purchases.FirstOrDefault()?.Rate ?? 0;
            var totalLiters = purchases.Sum(p => p.GrossLiters);
            var totalAmount = purchases.Sum(p => p.TotalAmount);

            double previousRate;

            if (totalLiters > 0)
            {
                previousRate = (double)(totalAmount / totalLiters);   // Safe division
            }
            else
            {
                previousRate = (double)(purchases.FirstOrDefault()?.Rate ?? 0);
            }

            return new RateSummaryDto
            {
                PreviousRate = (decimal)previousRate,
                TotalLiters = totalLiters,
                TotalAmount = totalAmount
            };
        }

        public async Task<RateSummaryDto> GetBuyerRateSummaryForPeriod(int accountId, DateOnly startDate, DateOnly endDate, int tenantId)
        {
            var sales = await _context.Sales
                .Where(s => s.AccountId == accountId && s.TenantId == tenantId && s.Date >= startDate && s.Date <= endDate)
                .ToListAsync();

            //var previousRate = sales.FirstOrDefault()?.Rate ?? 0;
            var totalLiters = sales.Sum(s => s.NetLiters);
            var totalAmount = sales.Sum(s => s.TotalAmount);

            double previousRate;

            if (totalLiters > 0)
            {
                previousRate = (double)(totalAmount / totalLiters);   // Safe division
            }
            else
            {
                previousRate = (double)(sales.FirstOrDefault()?.Rate ?? 0);
            }

            return new RateSummaryDto
            {
                PreviousRate = (decimal)previousRate,
                TotalLiters = totalLiters,
                TotalAmount = totalAmount
            };
        }

        public async Task<bool> UpdateSupplierRateForPeriod(int accountId, decimal newRate, DateOnly startDate, DateOnly endDate, int tenantId)
        {
            var purchases = await _context.Purchases
                .Where(p => p.AccountId == accountId && p.TenantId == tenantId && p.Date >= startDate && p.Date <= endDate)
                .ToListAsync();
            foreach (var purchase in purchases)
                purchase.Rate = newRate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBuyerRateForPeriod(int accountId, decimal newRate, DateOnly startDate, DateOnly endDate, int tenantId)
        {
            var sales = await _context.Sales
                .Where(s => s.AccountId == accountId && s.TenantId == tenantId && s.Date >= startDate && s.Date <= endDate)
                .ToListAsync();
            foreach (var sale in sales)
                sale.Rate = newRate;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> MassUpdateSupplierRates(decimal newRate, int tenantId)
        {
            var suppliers = await _context.Suppliers.Where(s => s.TenantId == tenantId).ToListAsync();
            foreach (var supplier in suppliers)
                supplier.Rate = newRate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MassUpdateBuyerRates(decimal newRate, int tenantId)
        {
            var buyers = await _context.Buyers.Where(b => b.TenantId == tenantId).ToListAsync();
            foreach (var buyer in buyers)
                buyer.Rate = newRate;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> MassUpdateSupplierDodhi(int dodhiId, int[] supplierIds, int tenantId)
        {
            var suppliers = await _context.Suppliers
                .Where(s => supplierIds.Contains(s.SupplierId) && s.TenantId == tenantId)
                .ToListAsync();
            foreach (var supplier in suppliers)
                supplier.DodhiId = dodhiId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DataCleanupPreviewDto> GetCleanupPreviewAsync(int tenantId)
        {
            var purchasesCount = await _context.Purchases.CountAsync(p => p.TenantId == tenantId);
            var salesCount = await _context.Sales.CountAsync(s => s.TenantId == tenantId);
            var chillarReceivesCount = await _context.ChillarReceives.CountAsync(cr => cr.TenantId == tenantId);
            var stockEntriesCount = await _context.StockEntries.CountAsync(se => se.TenantId == tenantId);
            var cashPaymentsCount = await _context.CashPayments.CountAsync(cp => cp.TenantId == tenantId);
            var cashReceiptsCount = await _context.CashReceipts.CountAsync(cr => cr.TenantId == tenantId);
            var bankPaymentsCount = await _context.BankPayments.CountAsync(bp => bp.TenantId == tenantId);
            var bankReceiptsCount = await _context.BankReceipts.CountAsync(br => br.TenantId == tenantId);
            var journalEntriesCount = await _context.JournalEntries.CountAsync(je => je.TenantId == tenantId);

            var suppliersCount = await _context.Suppliers.CountAsync(s => s.TenantId == tenantId);
            var buyersCount = await _context.Buyers.CountAsync(b => b.TenantId == tenantId);
            var employeesCount = await _context.Employees.CountAsync(e => e.TenantId == tenantId);
            var chillarsCount = await _context.Chillars.CountAsync(c => c.TenantId == tenantId);
            var accountsCount = await _context.Accounts.CountAsync(a => a.TenantId == tenantId);

            var totalTransactional = purchasesCount + salesCount + chillarReceivesCount + stockEntriesCount +
                                     cashPaymentsCount + cashReceiptsCount + bankPaymentsCount + bankReceiptsCount +
                                     journalEntriesCount;

            var totalMaster = suppliersCount + buyersCount + employeesCount + chillarsCount + accountsCount;

            return new DataCleanupPreviewDto
            {
                PurchasesCount = purchasesCount,
                SalesCount = salesCount,
                ChillarReceivesCount = chillarReceivesCount,
                StockEntriesCount = stockEntriesCount,
                CashPaymentsCount = cashPaymentsCount,
                CashReceiptsCount = cashReceiptsCount,
                BankPaymentsCount = bankPaymentsCount,
                BankReceiptsCount = bankReceiptsCount,
                JournalEntriesCount = journalEntriesCount,
                SuppliersCount = suppliersCount,
                BuyersCount = buyersCount,
                EmployeesCount = employeesCount,
                ChillarsCount = chillarsCount,
                AccountsCount = accountsCount,
                TotalTransactionalRecords = totalTransactional,
                TotalMasterRecords = totalMaster
            };
        }

        public async Task<DataCleanupResultDto> ExecuteCleanupAsync(DataCleanupRequestDto request, int tenantId, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.TenantId == tenantId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Current user could not be validated.");
            }

            if (!string.IsNullOrWhiteSpace(request.Password) && user.PasswordHash != request.Password)
            {
                throw new UnauthorizedAccessException("Incorrect administrator password.");
            }

            var confirmed = !string.IsNullOrWhiteSpace(request.ConfirmationText) &&
                (string.Equals(request.ConfirmationText.Trim(), "RESET", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(request.ConfirmationText.Trim(), "CLEAR DATA", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(request.ConfirmationText.Trim(), "CONFIRM", StringComparison.OrdinalIgnoreCase));

            if (!confirmed)
            {
                throw new InvalidOperationException("Please type 'RESET' or 'CONFIRM' to authorize data cleanup.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = new DataCleanupResultDto { Success = true };

                if (request.ClearPurchases)
                {
                    result.DeletedPurchases = await _context.Purchases
                        .Where(p => p.TenantId == tenantId)
                        .ExecuteDeleteAsync();
                }

                if (request.ClearSales)
                {
                    result.DeletedSales = await _context.Sales
                        .Where(s => s.TenantId == tenantId)
                        .ExecuteDeleteAsync();
                }

                if (request.ClearChillarReceives)
                {
                    result.DeletedChillarReceives = await _context.ChillarReceives
                        .Where(cr => cr.TenantId == tenantId)
                        .ExecuteDeleteAsync();
                }

                if (request.ClearStockEntries)
                {
                    result.DeletedStockEntries = await _context.StockEntries
                        .Where(se => se.TenantId == tenantId)
                        .ExecuteDeleteAsync();
                }

                if (request.ClearPaymentsAndReceipts)
                {
                    var cashPaymentIds = await _context.CashPayments
                        .Where(cp => cp.TenantId == tenantId)
                        .Select(cp => cp.CashPaymentId)
                        .ToListAsync();
                    await _context.CashPaymentLines
                        .Where(cpl => cashPaymentIds.Contains(cpl.CashPaymentId))
                        .ExecuteDeleteAsync();
                    var cashPayments = await _context.CashPayments
                        .Where(cp => cp.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    var cashReceiptIds = await _context.CashReceipts
                        .Where(cr => cr.TenantId == tenantId)
                        .Select(cr => cr.CashReceiptId)
                        .ToListAsync();
                    await _context.CashReceiptLines
                        .Where(crl => cashReceiptIds.Contains(crl.CashReceiptId))
                        .ExecuteDeleteAsync();
                    var cashReceipts = await _context.CashReceipts
                        .Where(cr => cr.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    var bankPaymentIds = await _context.BankPayments
                        .Where(bp => bp.TenantId == tenantId)
                        .Select(bp => bp.BankPaymentId)
                        .ToListAsync();
                    await _context.BankPaymentLines
                        .Where(bpl => bankPaymentIds.Contains(bpl.BankPaymentId))
                        .ExecuteDeleteAsync();
                    var bankPayments = await _context.BankPayments
                        .Where(bp => bp.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    var bankReceiptIds = await _context.BankReceipts
                        .Where(br => br.TenantId == tenantId)
                        .Select(br => br.BankReceiptId)
                        .ToListAsync();
                    await _context.BankReceiptLines
                        .Where(brl => bankReceiptIds.Contains(brl.BankReceiptId))
                        .ExecuteDeleteAsync();
                    var bankReceipts = await _context.BankReceipts
                        .Where(br => br.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    result.DeletedPaymentsAndReceipts = cashPayments + cashReceipts + bankPayments + bankReceipts;
                }

                if (request.ClearJournalEntries)
                {
                    // Unlink foreign keys pointing to journal_entries to avoid constraint violations
                    await _context.FinancialYears
                        .Where(fy => fy.TenantId == tenantId)
                        .ExecuteUpdateAsync(s => s.SetProperty(fy => fy.ClosingJournalEntryId, (int?)null));

                    await _context.AccountOpeningBalances
                        .Where(ob => ob.TenantId == tenantId)
                        .ExecuteUpdateAsync(s => s.SetProperty(ob => ob.JournalEntryId, (int?)null));

                    await _context.CashPayments
                        .Where(cp => cp.TenantId == tenantId)
                        .ExecuteUpdateAsync(s => s.SetProperty(cp => cp.JournalEntryId, (int?)null));

                    await _context.CashReceipts
                        .Where(cr => cr.TenantId == tenantId)
                        .ExecuteUpdateAsync(s => s.SetProperty(cr => cr.JournalEntryId, (int?)null));

                    await _context.BankPayments
                        .Where(bp => bp.TenantId == tenantId)
                        .ExecuteUpdateAsync(s => s.SetProperty(bp => bp.JournalEntryId, (int?)null));

                    await _context.BankReceipts
                        .Where(br => br.TenantId == tenantId)
                        .ExecuteUpdateAsync(s => s.SetProperty(br => br.JournalEntryId, (int?)null));

                    var journalEntryIds = await _context.JournalEntries
                        .Where(je => je.TenantId == tenantId)
                        .Select(je => je.JournalEntryId)
                        .ToListAsync();
                    await _context.JournalEntryLines
                        .Where(jel => journalEntryIds.Contains(jel.JournalEntryId))
                        .ExecuteDeleteAsync();

                    result.DeletedJournalEntries = await _context.JournalEntries
                        .Where(je => je.TenantId == tenantId)
                        .ExecuteDeleteAsync();
                }

                if (request.PreserveAccounts)
                {
                    await _context.AccountBalances
                        .Where(ab => ab.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    await _context.AccountOpeningBalances
                        .Where(ob => ob.TenantId == tenantId)
                        .ExecuteDeleteAsync();
                }
                else
                {
                    await _context.Users
                        .Where(u => u.TenantId == tenantId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(u => u.EmployeeId, (int?)null)
                            .SetProperty(u => u.SupplierId, (int?)null)
                            .SetProperty(u => u.BuyerId, (int?)null));

                    var suppliers = await _context.Suppliers
                        .Where(s => s.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    var buyers = await _context.Buyers
                        .Where(b => b.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    var employees = await _context.Employees
                        .Where(e => e.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    var chillars = await _context.Chillars
                        .Where(c => c.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    result.DeletedMasterEntities = suppliers + buyers + employees + chillars;

                    await _context.AccountBalances
                        .Where(ab => ab.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    await _context.AccountOpeningBalances
                        .Where(ob => ob.TenantId == tenantId)
                        .ExecuteDeleteAsync();

                    // Unlink retained earnings before deleting accounts
                    await _context.FinancialYears
                        .Where(fy => fy.TenantId == tenantId)
                        .ExecuteUpdateAsync(s => s.SetProperty(fy => fy.RetainedEarningsAccountId, (int?)null));

                    result.DeletedAccounts = await _context.Accounts
                        .Where(a => a.TenantId == tenantId)
                        .ExecuteDeleteAsync();
                }

                await transaction.CommitAsync();

                result.TotalDeleted = result.DeletedPurchases + result.DeletedSales +
                                      result.DeletedChillarReceives + result.DeletedStockEntries +
                                      result.DeletedPaymentsAndReceipts + result.DeletedJournalEntries +
                                      result.DeletedMasterEntities + result.DeletedAccounts;

                result.Message = request.PreserveAccounts
                    ? $"Data cleanup successful. Cleared {result.TotalDeleted} operational records. All Accounts and master entities were preserved with zeroed balances."
                    : $"Full system cleanup successful. Cleared {result.TotalDeleted} records including all operational and master entity data.";

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

}
