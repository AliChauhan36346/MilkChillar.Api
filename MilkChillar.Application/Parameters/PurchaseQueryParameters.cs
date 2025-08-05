using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class PurchaseQueryParameters : PaginationParameters
    {
        public int TenantId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string? TimeOfDay { get; set; } // "morning", "evening", or null for both
        public int? AccountId { get; set; }
        public int? DodhiId { get; set; }
    }
}
