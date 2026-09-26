using System;
using System.Collections.Generic;

namespace MilkChillar.Domain.Entities
{
    public class FinancialYear
    {
        public int FinancialYearId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. "FY 2024-2025"
        public string Code { get; set; } = string.Empty; // e.g. "FY24-25"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int? ClosedBy { get; set; }
        public int? ClosingJournalEntryId { get; set; }
        public int? RetainedEarningsAccountId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Tenant Tenant { get; set; } = null!;
        public User? ClosedByUser { get; set; }
        public JournalEntry? ClosingJournalEntry { get; set; }
        public Account? RetainedEarningsAccount { get; set; }
    }
}
