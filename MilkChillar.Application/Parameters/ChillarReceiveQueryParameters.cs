using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class ChillarReceiveQueryParameters : PaginationParameters
    {
        public int TenantId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string? TimeOfDay { get; set; }
        public int? ChillarId { get; set; }
    }

}
