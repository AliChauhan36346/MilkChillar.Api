using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Reports
{
    public class ChillarInchargeDashboardStatsDto
    {
        public decimal PreviousStock { get; set; }
        public decimal TotalChillarReceive { get; set; }
        public decimal TotalSales { get; set; }
        public decimal CurrentStock { get; set; }
    }
}
