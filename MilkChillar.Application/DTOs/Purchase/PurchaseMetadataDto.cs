using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Purchase
{
    public class PurchaseMetadataDto
    {
        public List<PurchaseDto> AddedPurchases { get; set; } = new List<PurchaseDto>();
        public List<RemainingSupplierDto> RemainingSuppliers { get; set; } = new List<RemainingSupplierDto>();
        public List<ExpenseAccountDto> ExpenseAccounts { get; set; } = new List<ExpenseAccountDto>();
        public List<DodhiDto> Dodhis { get; set; } = new List<DodhiDto>();
    }
}
