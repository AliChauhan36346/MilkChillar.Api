using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class ParchiQueryParameters
    {
        public int TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? DodhiId { get; set; }
        public int? SupplierId { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
    }
}
