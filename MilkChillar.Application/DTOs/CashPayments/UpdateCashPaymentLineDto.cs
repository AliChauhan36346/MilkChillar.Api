using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.CashPayments
{
    public class UpdateCashPaymentLineDto
    {
        public int? CashPaymentLineId { get; set; } // null for new lines

        [Required]
        public int AccountId { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}
