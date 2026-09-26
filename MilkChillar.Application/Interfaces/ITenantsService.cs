using MilkChillar.Application.DTOs.Tenant;

namespace MilkChillar.Application.Interfaces
{
    /// <summary>
    /// Service for tenant management
    /// </summary>
    public interface ITenantsService
    {
        /// <summary>
        /// Create a new tenant with automatic startup accounts
        /// </summary>
        Task<TenantSetupResponse> CreateTenantAsync(CreateTenantRequest request);

        /// <summary>
        /// Get tenant by ID
        /// </summary>
        Task<Domain.Entities.Tenant?> GetTenantByIdAsync(int tenantId);

        /// <summary>
        /// Get all tenants
        /// </summary>
        Task<List<Domain.Entities.Tenant>> GetAllTenantsAsync();
    }
}
