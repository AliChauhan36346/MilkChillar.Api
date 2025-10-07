using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class AccountLedgerQueryParameters
    {
        public int TenantId { get; set; }
        public int AccountId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; }
        public string? SourceTable { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public bool IncludeZeroTransactions { get; set; } = true;
    }
}
