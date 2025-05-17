using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class Buyer
    {
        public int BuyerId { get; set; }
        public int TenantId { get; set; }
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public decimal Rate { get; set; }
        public string KhataNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
