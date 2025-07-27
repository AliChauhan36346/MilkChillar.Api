using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Chillar
{
    public class UpdateChillarDto
    {
        public int ChillarId { get; set; }
        public string Name { get; set; } = default!;
        public string? Location { get; set; }
        public bool IsActive { get; set; }
    }
}
