using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Parchi
{
    public class ParchiSummaryDto
    {
        public decimal TotalLiters { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalParchiAmount { get; set; }
    }
}
