using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Accounts
{
    // MainAccountDto.cs
    public class MainAccountDto
    {
        public int MainAccountId { get; set; }
        public string MainAccountCode { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string FinancialStatementComponent { get; set; } = default!;
    }

}
