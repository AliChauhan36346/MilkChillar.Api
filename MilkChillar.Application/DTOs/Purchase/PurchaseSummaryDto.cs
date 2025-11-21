using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Purchase
{
    public class PurchaseSummaryDto
    {
        public DateOnly Date { get; set; }
        public int DodhiId { get; set; }

        public TimeSummary Morning { get; set; }
        public TimeSummary Evening { get; set; }

        public class TimeSummary
        {
            public decimal TotalLiters { get; set; }
            public decimal TotalAmount { get; set; }
            public int Count { get; set; }
        }

        public TimeSummary Combined => new TimeSummary
        {
            TotalLiters = Morning.TotalLiters + Evening.TotalLiters,
            TotalAmount = Morning.TotalAmount + Evening.TotalAmount,
            Count = Morning.Count + Evening.Count
        };
    }
}
