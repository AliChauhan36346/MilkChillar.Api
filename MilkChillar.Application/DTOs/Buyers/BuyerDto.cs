using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Buyers
{
    public class BuyerDto
    {
        public int BuyerId { get; set; }
        public int TenantId { get; set; }
        public int AccountId { get; set; }
        //public string FullName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string KhataNumber { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }

        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
    }
}
