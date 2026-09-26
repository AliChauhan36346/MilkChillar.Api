using MilkChillar.Application;
using MilkChillar.Application.Configuration;
using MilkChillar.Application.DTOs.Tenant;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MilkChillar.Infrastructure.Services
{
    /// <summary>
    /// Service for setting up starter accounts when a new tenant is created
    /// </summary>
    public class TenantSetupService : ITenantSetupService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TenantSetupService> _logger;

        public TenantSetupService(ApplicationDbContext dbContext, ILogger<TenantSetupService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<AccountSetupStats> SetupStarterAccountsAsync(int tenantId)
        {
            try
            {
                var stats = new AccountSetupStats();
                var config = TenantSetupConfiguration.GetStarterAccounts();

                _logger.LogInformation($"Starting account setup for tenant {tenantId}");

                foreach (var mainAccountTemplate in config)
                {
                    // Create Main Account
                    var mainAccount = new MainAccount
                    {
                        TenantId = tenantId,
                        Name = mainAccountTemplate.Name,
                        FinancialStatementComponent = mainAccountTemplate.FinancialStatementComponent,
                        MainAccountCode = mainAccountTemplate.MainAccountCode
                    };

                    _dbContext.MainAccounts.Add(mainAccount);
                    await _dbContext.SaveChangesAsync();
                    stats.MainAccountsCreated++;

                    _logger.LogInformation($"Created MainAccount: {mainAccount.Name} ({mainAccount.MainAccountCode})");

                    // Create Sub Accounts
                    foreach (var subAccountTemplate in mainAccountTemplate.SubAccounts)
                    {
                        var subAccount = new SubAccount
                        {
                            TenantId = tenantId,
                            MainAccountId = mainAccount.MainAccountId,
                            Name = subAccountTemplate.Name,
                            SubAccountCode = subAccountTemplate.SubAccountCode
                        };

                        _dbContext.SubAccounts.Add(subAccount);
                        await _dbContext.SaveChangesAsync();
                        stats.SubAccountsCreated++;

                        _logger.LogInformation($"Created SubAccount: {subAccount.Name} ({subAccount.SubAccountCode})");

                        // Create Accounts
                        int accountIndex = 1;
                        foreach (var accountTemplate in subAccountTemplate.Accounts)
                        {
                            var accountCode = $"{subAccountTemplate.SubAccountCode}{accountIndex:D3}";
                            var fullCode = $"{mainAccountTemplate.MainAccountCode}-{subAccountTemplate.SubAccountCode}-{accountCode}";

                            var account = new Account
                            {
                                TenantId = tenantId,
                                SubAccountId = subAccount.SubAccountId,
                                Name = accountTemplate.Name,
                                AccountCode = accountCode,
                                FullCode = fullCode
                            };

                            _dbContext.Accounts.Add(account);
                            await _dbContext.SaveChangesAsync();
                            stats.AccountsCreated++;

                            _logger.LogInformation($"Created Account: {account.Name} ({fullCode})");

                            accountIndex++;
                        }
                    }
                }

                stats.TotalAccountsCreated = stats.MainAccountsCreated + stats.SubAccountsCreated + stats.AccountsCreated;

                _logger.LogInformation($"Tenant {tenantId} setup completed successfully. " +
                    $"Created {stats.MainAccountsCreated} main accounts, " +
                    $"{stats.SubAccountsCreated} sub accounts, " +
                    $"{stats.AccountsCreated} accounts. " +
                    $"Total: {stats.TotalAccountsCreated}");

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tenant {tenantId} setup failed.");
                throw;
            }
        }

        public async Task<bool> ValidateSetupAsync(int tenantId)
        {
            try
            {
                var mainAccountCount = await _dbContext.MainAccounts
                    .Where(m => m.TenantId == tenantId)
                    .CountAsync();

                var subAccountCount = await _dbContext.SubAccounts
                    .Where(s => s.TenantId == tenantId)
                    .CountAsync();

                var accountCount = await _dbContext.Accounts
                    .Where(a => a.TenantId == tenantId)
                    .CountAsync();

                var isValid = mainAccountCount > 0 && subAccountCount > 0 && accountCount > 0;

                _logger.LogInformation($"Validation for tenant {tenantId}: " +
                    $"MainAccounts={mainAccountCount}, SubAccounts={subAccountCount}, Accounts={accountCount}. Valid={isValid}");

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Validation for tenant {tenantId} failed");
                return false;
            }
        }
    }
}
