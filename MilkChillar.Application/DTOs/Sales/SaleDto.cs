using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Sales
{
    public class SaleDto
    {
        public int SaleId { get; set; }
        public DateOnly Date { get; set; }

        public int AccountId { get; set; }
        public string AccountCode { get; set; } = default!;
        public string AccountName { get; set; } = default!;
        public int RevenueAccountId { get; set; }
        public string RevenueAccountName { get; set; } = default!;
        public int ChillarId { get; set; }
        public string ChillarName { get; set; } = default!;
        public int AddedById { get; set; }
        public string AddedByName { get; set; } = default!;

        public decimal GrossLiters { get; set; }
        public decimal? LR { get; set; }
        public decimal? Fat { get; set; }
        public decimal NetLiters { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal Balance { get; set; }
    }
}

