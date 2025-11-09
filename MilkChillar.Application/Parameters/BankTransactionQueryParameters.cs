using System;

namespace MilkChillar.Application.Parameters
{
    public class BankTransactionQueryParameters
    {
        public int TenantId { get; set; }
        public string? Search { get; set; }
        public int? VoucherNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? BankAccountId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool? HasJournalEntry { get; set; }
        public string? InstrumentNo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}