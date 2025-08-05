using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class Sales
    {
        public int SaleId { get; set; }
        public int TenantId { get; set; }
        public DateOnly Date { get; set; }

        public int AccountId { get; set; }
        public int RevenueAccountId { get; set; }
        public int ChillarId { get; set; }
        public int AddedBy { get; set; }

        public decimal GrossLiters { get; set; }
        public decimal? LR { get; set; }
        public decimal? Fat { get; set; }
        public decimal NetLiters { get; set; }
        public decimal Rate { get; set; }

        public decimal TotalAmount => NetLiters * Rate;
        public decimal AmountReceived { get; set; }
        public decimal Balance { get; set; }

        // Navigation
        public Account Account { get; set; } = default!;
        public Account RevenueAccount { get; set; } = default!;
        public Chillar Chillar { get; set; } = default!;
        public User User { get; set; } = default!;
        public Tenant Tenant { get; set; } = default!;
    }
}
