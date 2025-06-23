using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.RolePermissions
{
    public class CreateRolePermissionDto
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
    }
}

