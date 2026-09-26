using System;
using System.Collections.Generic;

namespace MilkChillar.Application.DTOs.Dashboard
{
    public class AdminDashboardStatsDto
    {
        public decimal CashBalance { get; set; }
        public decimal BankBalance { get; set; }
        public decimal TotalLiquidity => CashBalance + BankBalance;

        public decimal PendingPayments { get; set; }
        public int PendingPaymentsCount { get; set; }
        public decimal DueReceipts { get; set; }
        public int DueReceiptsCount { get; set; }
        public decimal NetWorkingPosition => DueReceipts - PendingPayments;

        public decimal TodayCashChange { get; set; }
        public decimal TodayBankChange { get; set; }

        // Today's Operational Pulse
        public decimal TodayPurchaseLiters { get; set; }
        public decimal TodayPurchaseAmount { get; set; }
        public decimal TodaySalesLiters { get; set; }
        public decimal TodaySalesAmount { get; set; }

        // Last 6 Months Financial Trends (Live Real P&L Data)
        public List<MonthlyFinancialTrendDto> MonthlyTrends { get; set; } = new();

        // Recent 5 Transactions (Live Roznamcha Stream)
        public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
    }

    public class MonthlyFinancialTrendDto
    {
        public string MonthLabel { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expense { get; set; }
        public decimal NetProfit => Revenue - Expense;
    }

    public class RecentTransactionDto
    {
        public int JournalEntryId { get; set; }
        public DateTime EntryDate { get; set; }
        public string SourceTable { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "Debit" or "Credit"
    }

    public class AccountBalanceSummaryDto
    {
        public string AccountType { get; set; } = string.Empty; // "Supplier" or "Buyer"
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal NetBalance { get; set; }
        public int AccountCount { get; set; }
    }

    public class AccountBalanceDetailDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal DebitTotal { get; set; }
        public decimal CreditTotal { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PagedAccountBalancesDto
    {
        public List<AccountBalanceDetailDto> Balances { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public AccountBalanceSummaryDto? Summary { get; set; }
    }
}