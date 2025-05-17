using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public int PermissionID { get; set; }
        public Permission Permission { get; set; }
    }
}
