using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Chillar
{
    public class CreateChillarDto
    {
        public int TenantId { get; set; }  // Optional: if you extract it from token, omit this.
        public string Name { get; set; } = default!;
        public string? Location { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
