using System;

namespace MilkChillar.Domain.Entities
{
    public class Buyer
    {
        public int BuyerId { get; set; }

        // Foreign Keys
        public int TenantId { get; set; }
        public int AccountId { get; set; }

        // Buyer Details
        public string FullName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string KhataNumber { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public string? Address { get; set; }

        public bool IsActive { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties (optional, for EF Core relationships)
        public Tenant Tenant { get; set; }
        public Account Account { get; set; }
    }
}
