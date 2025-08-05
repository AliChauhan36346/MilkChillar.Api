using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Employees
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public decimal? Salary { get; set; }
        public bool IsActive { get; set; }
        public int? ChillarId { get; set; }
        public String ChillarName { get; set; } = string.Empty;
    }
}

