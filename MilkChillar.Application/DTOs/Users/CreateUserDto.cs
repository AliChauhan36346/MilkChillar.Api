using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Users
{
    public class CreateUserDto
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public int? SupplierId { get; set; }
        public int? EmployeeId { get; set; }
        public int? BuyerId { get; set; }
        public int? RoleId { get; set; }
        public string UserType { get; set; } = "standard";
        public bool IsBlocked { get; set; } = false;
    }
}

