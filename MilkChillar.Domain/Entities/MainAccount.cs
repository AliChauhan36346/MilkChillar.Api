using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class MainAccount
    {
        public int MainAccountId { get; set; }
        public int TenantId { get; set; }
        public string MainAccountCode { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string FinancialStatementComponent { get; set; } = default!;

        public Tenant Tenant { get; set; } = default!;
        public ICollection<SubAccount> SubAccounts { get; set; } = new List<SubAccount>();
    }

}
