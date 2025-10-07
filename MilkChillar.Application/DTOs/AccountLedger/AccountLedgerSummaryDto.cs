using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.AccountLedger
{
    public class AccountLedgerSummaryDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal ClosingBalance { get; set; }
        public int TransactionCount { get; set; }
        public DateTime? FirstTransactionDate { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }
}
