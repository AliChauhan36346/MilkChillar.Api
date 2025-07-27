using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.ChillarReceive
{
    public class CreateChillarReceiveDto
    {
        public DateOnly Date { get; set; }
        public string TimeOfDay { get; set; } = default!;
        public int ChillarId { get; set; }
        public int ChillarInchargeId { get; set; }
        public int DodhiId { get; set; }
        public decimal GrossLiters { get; set; }
        public decimal? LR { get; set; }
        public decimal? Fat { get; set; }
        public decimal NetLiters { get; set; }
    }

}
