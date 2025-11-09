using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Parameters
{
    public class MilkCardQueryParameters
    {
        public int TenantId { get; set; }
        public int AccountId { get; set; }
        public DateTime Date { get; set; }
        public string TransactionType { get; set; } = "Purchase"; // "Purchase" or "Sale"
    }
}
