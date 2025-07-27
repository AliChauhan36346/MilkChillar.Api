using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.ChillarReceive
{
    public class ChillarReceiveDto
    {
        public int ReceiveId { get; set; }
        public DateOnly Date { get; set; }
        public string TimeOfDay { get; set; } = default!;
        public string ChillarName { get; set; } = default!;
        public string InchargeName { get; set; } = default!;
        public string DodhiName { get; set; } = default!;
        public decimal GrossLiters { get; set; }
        public decimal? LR { get; set; }
        public decimal? Fat { get; set; }
        public decimal NetLiters { get; set; }
    }

}
