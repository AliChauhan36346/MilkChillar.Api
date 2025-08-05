using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.StockEntry
{
    public class UpdateStockEntryDto
    {
        public DateOnly Date { get; set; }
        public string TimeOfDay { get; set; } = null!;
        public int ChillarId { get; set; }
        public decimal TheoreticalLiters { get; set; }
        public decimal MeasuredLiters { get; set; }
    }
}
