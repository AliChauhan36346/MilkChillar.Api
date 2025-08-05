using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Chillar
{
    public class CreateChillarDto
    {
        public int TenantId { get; set; }
        public string Name { get; set; } = default!;
        public string? Location { get; set; }

        public int NumberOfChillars { get; set; }   // ✅ Add
        public decimal Capacity { get; set; }       // ✅ Add
    }

}
