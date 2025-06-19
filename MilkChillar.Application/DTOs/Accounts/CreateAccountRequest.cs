using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Accounts
{
    public class CreateAccountRequest
    {
        public int TenantId { get; set; }
        public int SubAccountId { get; set; }
        public string Name { get; set; } = default!;
    }
}
