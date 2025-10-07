using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.CashPayments
{
    public class CashPaymentLineDto
    {
        public int CashPaymentLineId { get; set; }
        public int CashPaymentId { get; set; }
        public int AccountId { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }

        // Related data
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? AccountFullCode { get; set; }
    }
}
