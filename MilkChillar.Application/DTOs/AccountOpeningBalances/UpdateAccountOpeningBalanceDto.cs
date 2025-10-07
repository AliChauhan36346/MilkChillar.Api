using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.AccountOpeningBalances
{
    public class UpdateAccountOpeningBalanceDto
    {
        public int AccountId { get; set; }
        public DateTime OpeningDate { get; set; }
        public decimal DebitOpening { get; set; }
        public decimal CreditOpening { get; set; }
        public string? Description { get; set; }
    }
}
