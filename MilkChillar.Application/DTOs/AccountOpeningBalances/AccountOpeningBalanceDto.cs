using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.AccountOpeningBalances
{
    public class AccountOpeningBalanceDto
    {
        public int OpeningBalanceId { get; set; }
        public int TenantId { get; set; }
        public int AccountId { get; set; }
        public DateTime OpeningDate { get; set; }
        public decimal DebitOpening { get; set; }
        public decimal CreditOpening { get; set; }
        public string? Description { get; set; }
        public int AddedBy { get; set; }
        public int? JournalEntryId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Related data
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? AccountFullCode { get; set; }
        public string? AddedByUsername { get; set; }
        public string? JournalDescription { get; set; }

        // Calculated fields
        public decimal NetBalance => DebitOpening - CreditOpening;
        public string BalanceType => NetBalance >= 0 ? "Debit" : "Credit";
        public decimal AbsoluteBalance => Math.Abs(NetBalance);
    }

}
