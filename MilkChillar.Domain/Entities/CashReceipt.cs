using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class CashReceipt
    {
        public int CashReceiptId { get; set; }
        public int TenantId { get; set; }
        public int ReceiptNo { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string? JobDescription { get; set; }
        public int CashAccountId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remarks { get; set; }
        public int AddedBy { get; set; }
        public int? JournalEntryId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Tenant Tenant { get; set; } = null!;
        public Account CashAccount { get; set; } = null!;
        public User User { get; set; } = null!;
        public JournalEntry? JournalEntry { get; set; }
        public ICollection<CashReceiptLine> CashReceiptLines { get; set; } = new List<CashReceiptLine>();
    }
}
