using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class DashboardStatsQuery
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? TimeOfDay { get; set; } // Optional: "morning" or "evening"
        public int? DodhiId { get; set; } // Optional
        public int? ChillarId { get; set; } // Optional
    }

}
