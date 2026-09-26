using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Accounts
{
    public class ChartOfAccountDto
    {
        public int MainAccountId { get; set; }
        public string MainAccountCode { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string FinancialStatementComponent { get; set; } = default!;
        public decimal DebitTotal { get; set; } = 0;
        public decimal CreditTotal { get; set; } = 0;
        public decimal Balance { get; set; } = 0;
        public string BalanceType { get; set; } = "Dr";
        public List<SubAccountNodeDto> SubAccounts { get; set; } = new();
    }

    public class SubAccountNodeDto
    {
        public int SubAccountId { get; set; }
        public string SubAccountCode { get; set; } = default!;
        public string Name { get; set; } = default!;
        public decimal DebitTotal { get; set; } = 0;
        public decimal CreditTotal { get; set; } = 0;
        public decimal Balance { get; set; } = 0;
        public string BalanceType { get; set; } = "Dr";
        public List<AccountNodeDto> Accounts { get; set; } = new();
    }

    public class AccountNodeDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = default!;
        public string FullCode { get; set; } = default!;
        public string Name { get; set; } = default!;
        public decimal DebitTotal { get; set; } = 0;
        public decimal CreditTotal { get; set; } = 0;
        public decimal Balance { get; set; } = 0;
        public string BalanceType { get; set; } = "Dr";
    }

}
