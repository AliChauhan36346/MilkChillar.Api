using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.UserPermissions
{
    public class CreateUserPermissionDto
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }
    }
}

