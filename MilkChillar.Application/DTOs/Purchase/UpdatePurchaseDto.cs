using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Purchase
{
    public class UpdatePurchaseDto
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required]
        [RegularExpression("^(morning|evening)$", ErrorMessage = "TimeOfDay must be either 'morning' or 'evening'")]
        public string TimeOfDay { get; set; } = default!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "AccountId must be greater than 0")]
        public int AccountId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ExpenseAccountId must be greater than 0")]
        public int ExpenseAccountId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "DodhiId must be greater than 0")]
        public int DodhiId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "GrossLiters must be greater than 0")]
        public decimal GrossLiters { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be greater than 0")]
        public decimal Rate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Balance must be greater than or equal to 0")]
        public decimal Balance { get; set; }
    }
}
