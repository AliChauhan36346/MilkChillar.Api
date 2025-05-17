using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class SubAccount
    {
        public int SubAccountId { get; set; }
        public int TenantId { get; set; }
        public int MainAccountId { get; set; }
        public string SubAccountCode { get; set; } = default!;
        public string Name { get; set; } = default!;

        public Tenant Tenant { get; set; } = default!;
        public MainAccount MainAccount { get; set; } = default!;
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }

}
