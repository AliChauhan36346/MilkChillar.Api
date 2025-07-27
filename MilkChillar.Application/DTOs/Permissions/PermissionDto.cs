using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Permissions
{
    public class PermissionDto
    {
        public int PermissionId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }
}

