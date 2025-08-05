using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Domain/Entities/StockEntry.cs
namespace MilkChillar.Domain.Entities
{
    public class StockEntry
    {
        public int StockEntryId { get; set; }
        public int TenantId { get; set; }
        public int ChillarId { get; set; }
        public DateOnly Date { get; set; }
        public string TimeOfDay { get; set; } = null!;
        public decimal TheoreticalLiters { get; set; }
        public decimal MeasuredLiters { get; set; }
        public decimal Variance { get; private set; } // Auto-calculated in DB

        public int CreatedBy { get; set; }

        public Chillar? Chillar { get; set; }
        public User? CreatedByUser { get; set; }

        public Tenant? Tenant { get; set; }
    }
}

