using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Parchi
{
    public class ParchiResultDto
    {
        public List<ParchiDto> Items { get; set; } = new();
        public ParchiSummaryDto Summary { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
