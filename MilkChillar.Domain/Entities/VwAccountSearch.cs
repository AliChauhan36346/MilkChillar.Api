using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class VwAccountSearch
    {
        public int AccountId { get; set; }
        public int TenantId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string FinancialStatementComponent { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
