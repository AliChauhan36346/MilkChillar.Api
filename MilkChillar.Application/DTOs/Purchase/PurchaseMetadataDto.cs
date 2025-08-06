using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.DTOs.Purchase
{
    public class PurchaseMetadataDto
    {
        public int? DodhiId { get; set; }
        public List<PurchaseDto> AddedPurchases { get; set; } = new List<PurchaseDto>();
        public List<RemainingSupplierDto> RemainingSuppliers { get; set; } = new List<RemainingSupplierDto>();
        public List<ExpenseAccountDto> ExpenseAccounts { get; set; } = new List<ExpenseAccountDto>();
    }
}
