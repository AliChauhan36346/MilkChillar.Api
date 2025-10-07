using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class JournalEntryLine
    {
        public int JournalLineId { get; set; }
        public int JournalEntryId { get; set; }
        public int AccountId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Narration { get; set; }

        // Navigation properties
        public JournalEntry JournalEntry { get; set; } = null!;
        public Account Account { get; set; } = null!;
    }
}
