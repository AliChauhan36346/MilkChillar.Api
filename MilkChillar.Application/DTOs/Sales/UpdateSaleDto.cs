using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Sales
{
    public class UpdateSaleDto
    {
        public DateOnly Date { get; set; }

        public int AccountId { get; set; }
        public int RevenueAccountId { get; set; }
        public int ChillarId { get; set; }

        public decimal GrossLiters { get; set; }
        public decimal? LR { get; set; }
        public decimal? Fat { get; set; }
        public decimal NetLiters { get; set; }
        public decimal Rate { get; set; }

        public decimal AmountReceived { get; set; }
    }
}

