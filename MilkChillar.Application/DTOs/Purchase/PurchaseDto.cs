using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Purchase
{
    public class PurchaseDto
    {
        public int PurchaseId { get; set; }
        public DateOnly Date { get; set; }
        public string TimeOfDay { get; set; } = default!;
        public int AccountId { get; set; }
        public string AccountName { get; set; } = default!;
        public string AccountCode { get; set; } = default!;
        public string ExpenseAccountName { get; set; } = default!;
        public string DodhiName { get; set; } = default!;
        public decimal GrossLiters { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Balance { get; set; }
    }
}
