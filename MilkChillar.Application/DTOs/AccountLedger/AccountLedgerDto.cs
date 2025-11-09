using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.AccountLedger
{
    public class AccountLedgerDto
    {
        public int JournalLineId { get; set; }
        public int JournalEntryId { get; set; }
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Description { get; set; }
        public string? Narration { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
        public string? SourceTable { get; set; }
        public int? SourceId { get; set; }

        // New properties for grouped transactions
        public bool IsGrouped { get; set; }
        public int? GroupedTransactionCount { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
    }
}
