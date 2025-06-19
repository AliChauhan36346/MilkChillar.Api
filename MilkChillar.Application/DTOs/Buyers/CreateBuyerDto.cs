using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Buyers
{
    public class CreateBuyerDto
    {
        public int AccountId { get; set; }
        public decimal Rate { get; set; }
        public string KhataNumber { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }
}

