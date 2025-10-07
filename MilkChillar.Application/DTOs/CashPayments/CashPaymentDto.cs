using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.CashPayments
{
    public class CashPaymentDto
    {
        public int CashPaymentId { get; set; }
        public int TenantId { get; set; }
        public int VoucherNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? JobDescription { get; set; }
        public int CashAccountId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remarks { get; set; }
        public int AddedBy { get; set; }
        public int? JournalEntryId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Related data
        public string? CashAccountCode { get; set; }
        public string? CashAccountName { get; set; }
        public string? AddedByUsername { get; set; }
        public string? JournalDescription { get; set; }

        // Payment lines
        public List<CashPaymentLineDto> PaymentLines { get; set; } = new List<CashPaymentLineDto>();

        // Calculated fields
        public decimal LinesTotal => PaymentLines?.Sum(l => l.Amount) ?? 0;
        public bool IsBalanced => Math.Abs(TotalAmount - LinesTotal) < 0.01m;
    }
}
