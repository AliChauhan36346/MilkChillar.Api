using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Users
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = default!;
        public string UserType { get; set; } = default!;
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }

        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }

        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
    }
}

