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
        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Column("permission_id")]
        public int PermissionId { get; set; }
        public Permission Permission { get; set; }
    }
}
