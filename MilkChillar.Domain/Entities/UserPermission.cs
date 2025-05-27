using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class UserPermission
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public DateTime GrantedAt { get; set; }

        public User User { get; set; } = default!;
        public Permission Permission { get; set; } = default!;
    }

}
