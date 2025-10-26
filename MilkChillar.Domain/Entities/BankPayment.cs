using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class BankPayment
    {
        public int BankPaymentId { get; set; }
        public int TenantId { get; set; }
        public int VoucherNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? JobDescription { get; set; }
        public int BankAccountId { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remarks { get; set; }
        public int AddedBy { get; set; }
        public int? JournalEntryId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Tenant Tenant { get; set; } = null!;
        public Account BankAccount { get; set; } = null!;
        public User User { get; set; } = null!;
        public JournalEntry? JournalEntry { get; set; }
        public ICollection<BankPaymentLine> BankPaymentLines { get; set; } = new List<BankPaymentLine>();
    }
}
