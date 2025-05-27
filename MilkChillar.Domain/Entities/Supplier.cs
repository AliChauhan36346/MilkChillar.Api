using System;

namespace MilkChillar.Domain.Entities
{
    public class Supplier
    {
        public int SupplierId { get; set; }

        public int TenantId { get; set; }
        public int AccountId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public decimal Rate { get; set; }
        public string KhataNumber { get; set; } = string.Empty;

        public decimal CreditLimit { get; set; }

        public int? DodhiId { get; set; }
        public string? Address { get; set; }

        public bool GiveCreditOnParchi { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (if you will use them)
        public Tenant Tenant { get; set; } = null!;
        public Account Account { get; set; } = null!;
        public Employee? Dodhi { get; set; }
    }
}
