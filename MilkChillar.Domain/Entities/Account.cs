using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class Account
    {
        public int AccountId { get; set; }
        public int TenantId { get; set; }
        public int SubAccountId { get; set; }
        public string AccountCode { get; set; } = default!;

        public string FullCode { get; set; } = null!;
        public string Name { get; set; } = default!;

        public Tenant Tenant { get; set; } = default!;
        public SubAccount SubAccount { get; set; } = default!;
    }

}
