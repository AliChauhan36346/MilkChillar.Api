using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class ChillarInchargeDashboardQuery
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? StartTimeOfDay { get; set; } // "morning", "evening", null
        public string? EndTimeOfDay { get; set; }   // "morning", "evening", null
        public int? ChillarId { get; set; }
        public int? ChillarInchargeId { get; set; }
        public int? DodhiId { get; set; } // NEW filter
    }

}
