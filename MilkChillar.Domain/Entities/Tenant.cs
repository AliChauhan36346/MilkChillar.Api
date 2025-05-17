using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<MainAccount> MainAccounts { get; set; } = new List<MainAccount>();
        public ICollection<SubAccount> SubAccounts { get; set; } = new List<SubAccount>();
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

}
