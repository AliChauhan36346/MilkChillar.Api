using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class RolePermission
    {
        public int TenantId { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation Properties
        public Tenant Tenant { get; set; } = default!;
        public Role Role { get; set; } = default!;
        public Permission Permission { get; set; } = default!;


    }
}
