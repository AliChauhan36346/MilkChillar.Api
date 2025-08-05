using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Purchase
{
    public class ExpenseAccountDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = default!;
        public string AccountName { get; set; } = default!;
    }
}
