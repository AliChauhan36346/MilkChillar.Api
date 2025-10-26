using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Application/DTOs/Dashboard/AdminDashboardStatsDto.cs
namespace MilkChillar.Application.DTOs.Dashboard
{
    public class AdminDashboardStatsDto
    {
        public decimal CashBalance { get; set; }
        public decimal BankBalance { get; set; }
        public decimal PendingPayments { get; set; }
        public int PendingPaymentsCount { get; set; }
        public decimal DueReceipts { get; set; }
        public int DueReceiptsCount { get; set; }
        public decimal TodayCashChange { get; set; }
        public decimal TodayBankChange { get; set; }
    }

    public class AccountBalanceSummaryDto
    {
        public string AccountType { get; set; } // "Supplier" or "Buyer"
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal NetBalance { get; set; }
        public int AccountCount { get; set; }
    }

    public class AccountBalanceDetailDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public decimal DebitTotal { get; set; }
        public decimal CreditTotal { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PagedAccountBalancesDto
    {
        public List<AccountBalanceDetailDto> Balances { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public AccountBalanceSummaryDto Summary { get; set; }
    }
}