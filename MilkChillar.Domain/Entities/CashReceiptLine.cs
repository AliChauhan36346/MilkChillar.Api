using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class CashReceiptLine
    {
        public int CashReceiptLineId { get; set; }
        public int CashReceiptId { get; set; }
        public int AccountId { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }

        // Navigation properties
        public CashReceipt CashReceipt { get; set; } = null!;
        public Account Account { get; set; } = null!;
    }
}
