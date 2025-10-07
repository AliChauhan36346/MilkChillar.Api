using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class CashPaymentQueryParameters : PaginationParameters
    {
        public int TenantId { get; set; }
        public string? Search { get; set; }
        public int? VoucherNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CashAccountId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool? HasJournalEntry { get; set; }
    }
}
