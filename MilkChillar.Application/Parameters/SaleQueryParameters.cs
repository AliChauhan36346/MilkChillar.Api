using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class SaleQueryParameters : PaginationParameters
    {
        public int TenantId { get; set; }
    }
}
