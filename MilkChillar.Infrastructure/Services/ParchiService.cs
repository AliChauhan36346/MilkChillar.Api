using MilkChillar.Application.DTOs.Parchi;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;
using MilkChillar.Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Security.Principal;
using System.Xml.Linq;

namespace MilkChillar.Infrastructure.Services
{
    public class ParchiService : IParchiService
    {
        private readonly ApplicationDbContext _context;

        public ParchiService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ParchiResultDto> GetSupplierParchiAsync(ParchiQueryParameters query)
        {
            // Get all suppliers matching filters
            var suppliersQuery = _context.Suppliers
                .Include(s => s.Account)
                .Include(s => s.Dodhi)
                .Where(s => s.TenantId == query.TenantId);

            // Apply filters
            if (query.IsActive.HasValue)
            {
                suppliersQuery = suppliersQuery.Where(s => s.IsActive == query.IsActive.Value);
            }

            if (query.DodhiId.HasValue)
            {
                suppliersQuery = suppliersQuery.Where(s => s.DodhiId == query.DodhiId.Value);
            }

            if (query.SupplierId.HasValue)
            {
                suppliersQuery = suppliersQuery.Where(s => s.SupplierId == query.SupplierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                suppliersQuery = suppliersQuery.Where(s =>
                    s.Account.Name.Contains(query.Search) ||
                    s.KhataNumber.Contains(query.Search));
            }

            var suppliers = await suppliersQuery
                .OrderBy(s => s.KhataNumber)
                .ToListAsync();

            var parchiItems = new List<ParchiDto>();

            foreach (var supplier in suppliers)
            {
                var parchi = await CalculateSupplierParchi(supplier, query.StartDate, query.EndDate, query.TenantId);
                parchiItems.Add(parchi);
            }

            var summary = new ParchiSummaryDto
            {
                TotalLiters = parchiItems.Sum(p => p.TotalLiters),
                TotalPurchaseAmount = parchiItems.Sum(p => p.PurchaseAmount),
                TotalPayments = parchiItems.Sum(p => p.PaymentsInPeriod),
                TotalParchiAmount = parchiItems.Sum(p => p.ParchiAmount)
            };

            return new ParchiResultDto
            {
                Items = parchiItems,
                Summary = summary,
                TotalCount = parchiItems.Count
            };
        }

        public async Task<ParchiDto?> GetSingleSupplierParchiAsync(int supplierId, DateTime startDate, DateTime endDate, int tenantId)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Account)
                .Include(s => s.Dodhi)
                .FirstOrDefaultAsync(s => s.SupplierId == supplierId && s.TenantId == tenantId);

            if (supplier == null) return null;

            return await CalculateSupplierParchi(supplier, startDate, endDate, tenantId);
        }

        private async Task<ParchiDto> CalculateSupplierParchi(Supplier supplier, DateTime startDate, DateTime endDate, int tenantId)
        {
            var accountId = supplier.AccountId;

            // Convert to UTC
            var startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var startDateOnly = DateOnly.FromDateTime(startDate);
            var endDateOnly = DateOnly.FromDateTime(endDate);

            // 1. Calculate Previous Balance (everything before start date)
            var previousBalance = await _context.JournalEntryLines
                .Where(jel => jel.AccountId == accountId &&
                       jel.JournalEntry.TenantId == tenantId &&
                       jel.JournalEntry.EntryDate < startDateUtc)
                .SumAsync(jel => (decimal?)(jel.Credit - jel.Debit)) ?? 0;

            // 2. Get Period Transactions from Journal Entries
            var periodJournalLines = await _context.JournalEntryLines
                .Where(jel => jel.AccountId == accountId &&
                       jel.JournalEntry.TenantId == tenantId &&
                       jel.JournalEntry.EntryDate >= startDateUtc &&
                       jel.JournalEntry.EntryDate <= endDateUtc)
                .Select(jel => new
                {
                    jel.Debit,
                    jel.Credit,
                    jel.JournalEntry.SourceTable
                })
                .ToListAsync();

            // 3. Get liters AND amount from Purchase table
            var periodPurchases = await _context.Purchases
                .Where(p => p.AccountId == accountId &&
                       p.TenantId == tenantId &&
                       p.Date >= startDateOnly &&
                       p.Date <= endDateOnly)
                .Select(p => new
                {
                    Liters = p.GrossLiters,
                    Amount = p.GrossLiters * p.Rate
                })
                .ToListAsync();

            var totalLiters = periodPurchases.Sum(p => p.Liters);
            var purchaseAmount = periodPurchases.Sum(p => p.Amount);

            var paymentsInPeriod = periodJournalLines
                .Where(jel => (jel.SourceTable == "cash_payments" || jel.SourceTable == "bank_payments"))
                .Sum(jel => jel.Debit);

            var receiptsInPeriod = periodJournalLines
                .Where(jel => (jel.SourceTable == "cash_receipts" || jel.SourceTable == "bank_receipts"))
                .Sum(jel => jel.Credit);

            // 4. Calculate Closing Balance from journal entries
            var periodBalance = periodJournalLines.Sum(jel => jel.Credit - jel.Debit);
            var closingBalance = previousBalance + periodBalance;

            // 5. Apply Credit Limit Logic
            // Positive balance = We owe supplier (Credit balance)
            // Negative balance = Supplier owes us (Debit balance)
            // Credit limit of 20000 means: maintain their balance at 20000 DEBIT (negative)

            decimal parchiAmount;
            decimal finalBalance;
            string finalBalanceType;

            if (supplier.GiveCreditOnParchi && supplier.CreditLimit > 0 && purchaseAmount > 0)
            {
                // Target: Supplier should have -20000 balance (20000 debit)
                // If closing balance is 10000 (credit), parchi = 10000 + 20000 = 30000
                // After payment: 10000 - 30000 = -20000 (target achieved)

                parchiAmount = closingBalance + supplier.CreditLimit;
                finalBalance = closingBalance - parchiAmount;
                finalBalanceType = "Debit"; // Will always be debit after applying credit limit
            }
            else
            {
                // No credit allowed - pay only if we owe them (positive balance)
                parchiAmount = closingBalance > 0 ? closingBalance : 0;
                finalBalance = closingBalance - parchiAmount;
                finalBalanceType = finalBalance < 0 ? "Debit" : "Credit";
            }

            return new ParchiDto
            {
                AccountCode = supplier.Account.AccountCode,
                AccountName = supplier.Account.Name,
                AccountNameUrdu=supplier.NameUrdu,
                KhataNumber = supplier.KhataNumber,
                DodhiId = supplier.DodhiId,
                DodhiName = supplier.Dodhi?.FullName,

                PreviousBalance = Math.Abs(previousBalance),
                PreviousBalanceType = previousBalance < 0 ? "Debit" : "Credit",

                TotalLiters = Math.Round(totalLiters, 2),
                PurchaseAmount = Math.Round(purchaseAmount, 0),
                PaymentsInPeriod = Math.Round(paymentsInPeriod, 0),
                ReceiptsInPeriod = Math.Round(receiptsInPeriod, 0),

                ClosingBalance = Math.Round(Math.Abs(closingBalance), 0),
                ClosingBalanceType = closingBalance < 0 ? "Debit" : "Credit",

                CreditLimit = supplier.CreditLimit,
                IsCreditAllowed = supplier.GiveCreditOnParchi,
                ParchiAmount = Math.Round(parchiAmount, 0),
                FinalBalance = Math.Round(Math.Abs(finalBalance), 0),
                FinalBalanceType = finalBalanceType
            };
        }






        //no it is not picking up any thing like payment , liters etc i wanted to ask you that we will do every thing on the base fo account id not supplier id becuase journal entry lines contain account id not the the supplier id the supplier table is for the detail of supplier account like its rate, dodhi, and other info so we can get suppliers account id and name of specific dodhi from suppliers table as it has acccount id as foreign key and also in purchase table the purchase is storing with the account id not supplier id again here is the db schema so you know better and i need account code and name not supplier id and name as all the trasaction on the base of account code and name


    }
}