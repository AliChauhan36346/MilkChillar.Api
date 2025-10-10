using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Suppliers
{
    public class CreateSupplierDto
    {
        public int AccountId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? NameUrdu { get; set; }
        public decimal Rate { get; set; }
        public string KhataNumber { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public int? DodhiId { get; set; }
        public string? Address { get; set; }
        public bool GiveCreditOnParchi { get; set; }
        public bool IsActive { get; set; }
    }
}

