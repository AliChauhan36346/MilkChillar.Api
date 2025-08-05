using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public int TenantId { get; set; }
        public DateOnly Date { get; set; }
        public string TimeOfDay { get; set; } = default!; // "morning" or "evening"
        public int AccountId { get; set; }
        public int ExpenseAccountId { get; set; }
        public int DodhiId { get; set; }
        public decimal GrossLiters { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalAmount => GrossLiters * Rate;
        public decimal Balance { get; set; }

        // Navigation properties
        public Account Account { get; set; } = default!;
        public Account ExpenseAccount { get; set; } = default!;
        public Employee Dodhi { get; set; } = default!;
        public Tenant Tenant { get; set; } = default!;
    }
}