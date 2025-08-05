using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.StockEntry
{
    public class StockEntryDto
    {
        public int StockEntryId { get; set; }
        public DateOnly Date { get; set; }
        public string TimeOfDay { get; set; } = null!;
        public string ChillarName { get; set; } = null!;
        public decimal TheoreticalLiters { get; set; }
        public decimal MeasuredLiters { get; set; }
        public decimal Variance { get; set; }
    }
}
