using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public int TenantId { get; set; }
        public string FullName { get; set; } = default!;
        public string Designation { get; set; } = default!;
        public string? ContactNumber { get; set; }
        public decimal? Salary { get; set; }
        public bool IsActive { get; set; } = true;

        public Tenant Tenant { get; set; } = default!;
    }

}
