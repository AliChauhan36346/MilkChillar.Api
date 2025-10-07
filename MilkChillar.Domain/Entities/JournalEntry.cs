using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class JournalEntry
    {
        public int JournalEntryId { get; set; }
        public int TenantId { get; set; }
        public DateTime EntryDate { get; set; }
        public string? SourceTable { get; set; }
        public int? SourceId { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Description { get; set; }
        public int? AddedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Tenant Tenant { get; set; } = null!;
        public User? User { get; set; }
        public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
    }
}
