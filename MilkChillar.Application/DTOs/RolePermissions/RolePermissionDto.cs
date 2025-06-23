using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.RolePermissions
{
    public class RolePermissionDto
    {
        public int TenantId { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        public string RoleName { get; set; } = default!;
        public string PermissionName { get; set; } = default!;
    }
}

