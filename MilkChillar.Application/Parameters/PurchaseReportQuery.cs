using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class PurchaseReportQuery
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? TimeOfDay { get; set; } // "morning" or "evening"
        public int? DodhiId { get; set; }
        public int? ChillarId { get; set; }
        public string? SupplierCode { get; set; } // Optional: filter by specific supplier

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
