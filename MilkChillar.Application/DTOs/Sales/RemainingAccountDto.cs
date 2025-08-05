using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Sales
{
    public class RemainingAccountDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = default!;
        public string AccountCode { get; set; } = default!;
        public decimal Rate { get; set; }
    }
}
