using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.AccountLedger
{
    public class MilkCardDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // "Purchase" or "Sale"
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;

        // Transaction lines
        public List<MilkCardLineDto> Lines { get; set; } = new();

        // Morning totals
        public decimal TotalMorningQuantity { get; set; }
        public decimal TotalMorningAmount { get; set; }
        public decimal AverageMorningRate { get; set; }

        // Evening totals
        public decimal TotalEveningQuantity { get; set; }
        public decimal TotalEveningAmount { get; set; }
        public decimal AverageEveningRate { get; set; }

        // Grand totals
        public decimal GrandTotalQuantity { get; set; }
        public decimal GrandTotalAmount { get; set; }
        public decimal AverageTotalRate { get; set; }

        public int TransactionCount { get; set; }
    }

    public class MilkCardLineDto
    {
        public DateTime Date { get; set; }
        public decimal MorningQuantity { get; set; }
        public decimal MorningRate { get; set; }
        public decimal MorningAmount { get; set; }
        public decimal EveningQuantity { get; set; }
        public decimal EveningRate { get; set; }
        public decimal EveningAmount { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remarks { get; set; }
    }
}
