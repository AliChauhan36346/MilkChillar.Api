using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Accounts
{
    public class AccountSearchDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

}
