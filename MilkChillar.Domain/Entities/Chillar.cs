using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class Chillar
    {
        public int ChillarId { get; set; }
        public int TenantId { get; set; }

        public string Name { get; set; } = default!;
        public string? Location { get; set; }
        public bool IsActive { get; set; } = true;

        public Tenant Tenant { get; set; } = default!;
    }
}

