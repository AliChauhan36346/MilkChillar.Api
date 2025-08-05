using MilkChillar.Application.DTOs.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Sales
{
    public class SalesMetadataDto
    {
        public int? ChillarId { get; set; }
        public List<RemainingAccountDto> RemainingAccounts { get; set; } = new();
        public List<SaleDto> AddedSales { get; set; } = new();
        public List<RevenueAccountDto> RevenueAccounts { get; set; } = new();
    }
}
