using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class SalesReportQuery
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? BuyerCode { get; set; }
        public int? ChillarId { get; set; }
    }
}
