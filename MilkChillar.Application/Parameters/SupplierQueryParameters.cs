using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class SupplierQueryParameters
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Search
        public string? Search { get; set; }

        // Filtering
        public bool? IsActive { get; set; }
        public bool? GiveCreditOnParchi { get; set; }
        public int? DodhiId { get; set; }

        public int TenantId { get; set; } = 0; // Default to 0 if not set
    }

}
