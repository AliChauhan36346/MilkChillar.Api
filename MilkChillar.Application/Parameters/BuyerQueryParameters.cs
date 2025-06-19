using System;

namespace MilkChillar.Application.Parameters
{
    public class BuyerQueryParameters
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Filtering
        public string? Search { get; set; }
        public bool? IsActive { get; set; }

        // Tenant
        public int TenantId { get; set; }
    }
}
