using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.CashPayments
{
    public class CreateCashPaymentDto
    {
        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        public string? JobDescription { get; set; }

        [Required]
        public int CashAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
        public decimal TotalAmount { get; set; }

        public string? Remarks { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one payment line is required.")]
        public List<CreateCashPaymentLineDto> PaymentLines { get; set; } = new List<CreateCashPaymentLineDto>();
    }
}
