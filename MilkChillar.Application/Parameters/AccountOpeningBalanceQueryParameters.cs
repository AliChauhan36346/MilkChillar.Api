using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class AccountOpeningBalanceQueryParameters : PaginationParameters
    {
        public int TenantId { get; set; }
        public string? Search { get; set; }
        public int? AccountId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? HasDebitBalance { get; set; }
        public bool? HasCreditBalance { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}

