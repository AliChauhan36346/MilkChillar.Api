using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class AccountBalance
    {
        public int AccountId { get; set; }
        public int TenantId { get; set; }
        public decimal DebitTotal { get; set; } = 0;
        public decimal CreditTotal { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Account Account { get; set; }
        public Tenant Tenant { get; set; }
    }
}
