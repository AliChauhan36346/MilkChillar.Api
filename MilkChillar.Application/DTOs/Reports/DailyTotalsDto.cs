using System;
using System.Collections.Generic;

namespace MilkChillar.Application.DTOs.Reports
{
    public class DailyTotalsDto
    {
        public DateOnly Date { get; set; }
        public decimal TotalPurchaseLiters { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public decimal TotalChillarReceiveLiters { get; set; }
        public decimal DodhiLoss { get; set; }
        public decimal TotalSalesLiters { get; set; }
        public decimal ChillarLoss { get; set; }
        public decimal TsSalesLiters { get; set; }
        public decimal TsDifference { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal GrossProfit { get; set; }
    }

    public class DailyTotalsResultDto
    {
        public List<DailyTotalsDto> Items { get; set; } = new();
    }
}
