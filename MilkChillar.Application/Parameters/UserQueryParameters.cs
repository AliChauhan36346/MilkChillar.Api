using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class UserQueryParameters
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Search (by Username, ideally)
        public string? Search { get; set; }

        // Filtering
        public bool? IsBlocked { get; set; }
        public string? UserType { get; set; }
        public int? RoleId { get; set; }

        public int TenantId { get; set; }
    }
}
