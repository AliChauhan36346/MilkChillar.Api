using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class AccountOpeningBalance
    {
        public int OpeningBalanceId { get; set; }
        public int TenantId { get; set; }
        public int AccountId { get; set; }
        public DateTime OpeningDate { get; set; }
        public decimal DebitOpening { get; set; }
        public decimal CreditOpening { get; set; }
        public string? Description { get; set; }
        public int AddedBy { get; set; }
        public int? JournalEntryId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Tenant Tenant { get; set; } = null!;
        public Account Account { get; set; } = null!;
        public User User { get; set; } = null!;
        public JournalEntry? JournalEntry { get; set; }
    }
}
