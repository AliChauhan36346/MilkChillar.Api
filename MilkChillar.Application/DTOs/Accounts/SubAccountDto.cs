using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Accounts
{
    // SubAccountDto.cs
    public class SubAccountDto
    {
        public int SubAccountId { get; set; }
        public string SubAccountCode { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int MainAccountId { get; set; }
    }

}
