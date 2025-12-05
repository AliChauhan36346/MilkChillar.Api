using MilkChillar.Application.DTOs.Maintenance;

namespace MilkChillar.Application.Interfaces
{
    public interface IMaintenanceService
    {
        Task<bool> MassUpdateSupplierRatesByDodhi(int dodhiId, decimal newRate, int tenantId);
        Task<SupplierRateSummaryDto> GetSupplierRateSummaryForPeriod(int accountId, DateOnly startDate, DateOnly endDate, int tenantId);
        Task<bool> UpdateSupplierRateForPeriod(int accountId, decimal newRate, DateOnly startDate, DateOnly endDate, int tenantId);
        Task<bool> MassUpdateSupplierRates(decimal newRate, int tenantId);
        Task<bool> MassUpdateBuyerRates(decimal newRate, int tenantId);
        Task<bool> UpdateBuyerRateForPeriod(int accountId, decimal newRate, DateOnly startDate, DateOnly endDate, int tenantId);
        Task<bool> MassUpdateSupplierDodhi(int dodhiId, int[] supplierIds, int tenantId);
    }




}
