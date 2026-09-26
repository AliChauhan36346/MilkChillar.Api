namespace MilkChillar.Application.DTOs.Tenant
{
    /// <summary>
    /// Request to create a new tenant
    /// </summary>
    public class CreateTenantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    /// <summary>
    /// Response after tenant creation with account setup status
    /// </summary>
    public class TenantSetupResponse
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public bool SetupSuccess { get; set; }
        public string SetupMessage { get; set; } = string.Empty;
        public AccountSetupStats? Stats { get; set; }
    }

    /// <summary>
    /// Statistics about accounts created during setup
    /// </summary>
    public class AccountSetupStats
    {
        public int MainAccountsCreated { get; set; }
        public int SubAccountsCreated { get; set; }
        public int AccountsCreated { get; set; }
        public int TotalAccountsCreated { get; set; }
    }
}
