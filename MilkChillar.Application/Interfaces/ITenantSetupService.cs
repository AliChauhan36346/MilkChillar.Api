using MilkChillar.Application.DTOs.Tenant;

namespace MilkChillar.Application.Interfaces
{
    /// <summary>
    /// Service for tenant startup account setup
    /// </summary>
    public interface ITenantSetupService
    {
        /// <summary>
        /// Setup starter accounts for a new tenant
        /// Creates all main accounts, sub accounts, and accounts based on configuration
        /// </summary>
        Task<AccountSetupStats> SetupStarterAccountsAsync(int tenantId);

        /// <summary>
        /// Validate that setup was successful
        /// </summary>
        Task<bool> ValidateSetupAsync(int tenantId);
    }
}
