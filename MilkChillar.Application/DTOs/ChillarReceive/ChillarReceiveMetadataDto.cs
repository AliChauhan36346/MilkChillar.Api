using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.ChillarReceive
{
    public class ChillarReceiveMetadataDto
    {
        public int ChillarId { get; set; }
        public int ChillarInchargeId { get; set; }
        public List<DodhiSimpleDto> RemainingDodhis { get; set; } = new();
        public List<ChillarReceiveDto> AddedDodhis { get; set; } = new();
    }
}
