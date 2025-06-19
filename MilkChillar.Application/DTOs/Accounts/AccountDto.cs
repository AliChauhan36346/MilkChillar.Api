using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Accounts
{
    // AccountDto.cs
    public class AccountDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = default!;
        public string FullCode { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int SubAccountId { get; set; }
    }

}
