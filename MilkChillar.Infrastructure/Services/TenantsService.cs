using MilkChillar.Application;
using MilkChillar.Application.DTOs.Tenant;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MilkChillar.Infrastructure.Services
{
    /// <summary>
    /// Service for managing tenants
    /// </summary>
    public class TenantsService : ITenantsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITenantSetupService _tenantSetupService;
        private readonly ILogger<TenantsService> _logger;

        public TenantsService(
            ApplicationDbContext dbContext,
            ITenantSetupService tenantSetupService,
            ILogger<TenantsService> logger)
        {
            _dbContext = dbContext;
            _tenantSetupService = tenantSetupService;
            _logger = logger;
        }

        public async Task<TenantSetupResponse> CreateTenantAsync(CreateTenantRequest request)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation($"Creating new tenant: {request.Name}");

                // 1. Create Tenant
                var tenant = new Tenant
                {
                    Name = request.Name,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Tenants.Add(tenant);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Tenant created: {tenant.TenantId} - {tenant.Name}");

                // 2. Setup Starter Accounts
                var stats = await _tenantSetupService.SetupStarterAccountsAsync(tenant.TenantId);

                // 3. Validate Setup
                var isValid = await _tenantSetupService.ValidateSetupAsync(tenant.TenantId);

                if (!isValid)
                {
                    throw new Exception("Account setup validation failed");
                }

                await transaction.CommitAsync();

                _logger.LogInformation($"Tenant {tenant.TenantId} created and setup completed successfully");

                return new TenantSetupResponse
                {
                    TenantId = tenant.TenantId,
                    TenantName = tenant.Name,
                    SetupSuccess = true,
                    SetupMessage = "Tenant created successfully with starter accounts",
                    Stats = stats
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Tenant creation failed");

                return new TenantSetupResponse
                {
                    SetupSuccess = false,
                    SetupMessage = $"Tenant creation failed: {ex.Message}"
                };
            }
        }

        public async Task<Tenant?> GetTenantByIdAsync(int tenantId)
        {
            return await _dbContext.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);
        }

        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await _dbContext.Tenants.ToListAsync();
        }
    }
}
