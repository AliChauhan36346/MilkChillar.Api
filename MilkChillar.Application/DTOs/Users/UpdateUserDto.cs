using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Users
{
    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int? SupplierId { get; set; }
        public int? EmployeeId { get; set; }
        public int? BuyerId { get; set; }
        public int? RoleId { get; set; }
        public string? UserType { get; set; }
        public bool IsBlocked { get; set; }
    }
}

