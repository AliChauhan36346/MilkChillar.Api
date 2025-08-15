using MilkChillar.Application.DTOs.ChillarReceive;
using MilkChillar.Application.DTOs.Purchase;

namespace MilkChillar.Application.DTOs.Dashboard
{
    public class DodhiPurchaseReportDto
    {
        public List<PurchaseDto> Purchases { get; set; }
        public List<ChillarReceiveDto> Receives { get; set; }
    }
}
