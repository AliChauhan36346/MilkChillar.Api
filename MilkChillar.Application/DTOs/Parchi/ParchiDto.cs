using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Application/DTOs/Parchi/ParchiDto.cs
namespace MilkChillar.Application.DTOs.Parchi
{
    public class ParchiDto
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountNameUrdu { get; set; }
        public string KhataNumber { get; set; } = string.Empty;
        public int? DodhiId { get; set; }
        public string? DodhiName { get; set; }

        // Previous Balance (before period)
        public decimal PreviousBalance { get; set; }
        public string PreviousBalanceType { get; set; } = string.Empty;

        // Period Transactions
        public decimal TotalLiters { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal PaymentsInPeriod { get; set; }
        public decimal ReceiptsInPeriod { get; set; }

        // Closing Balance
        public decimal ClosingBalance { get; set; }
        public string ClosingBalanceType { get; set; } = string.Empty;

        // Credit Logic
        public decimal CreditLimit { get; set; }
        public bool IsCreditAllowed { get; set; }
        public decimal ParchiAmount { get; set; }
        public decimal FinalBalance { get; set; }
        public string FinalBalanceType { get; set; } = string.Empty;
    }
}