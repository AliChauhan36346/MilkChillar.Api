namespace MilkChillar.Application.Configuration
{
    /// <summary>
    /// Configuration for tenant startup accounts setup
    /// Defines the complete chart of accounts structure for new tenants
    /// </summary>
    public class TenantSetupConfiguration
    {
        /// <summary>
        /// Get the default starter accounts structure for a new tenant
        /// Based on actual business requirements
        /// </summary>
        public static List<MainAccountTemplate> GetStarterAccounts()
        {
            return new List<MainAccountTemplate>
            {
                // 1. BUYERS (Current Assets)
                new MainAccountTemplate
                {
                    MainAccountCode = "100",
                    Name = "Buyers",
                    FinancialStatementComponent = "CurrentAssets",
                    SubAccounts = new List<SubAccountTemplate>
                    {
                        new SubAccountTemplate
                        {
                            SubAccountCode = "10001",
                            Name = "Cash Buyers",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Cash Sale" }
                            }
                        },
                        new SubAccountTemplate
                        {
                            SubAccountCode = "10002",
                            Name = "Other Buyers",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Credit Sale" }
                            }
                        }
                    }
                },

                // 2. SUPPLIERS (Current Liabilities)
                new MainAccountTemplate
                {
                    MainAccountCode = "200",
                    Name = "Suppliers",
                    FinancialStatementComponent = "CurrentLiabilities",
                    SubAccounts = new List<SubAccountTemplate>
                    {
                        new SubAccountTemplate
                        {
                            SubAccountCode = "20001",
                            Name = "Cash Supplier",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Cash Payment" }
                            }
                        },
                        new SubAccountTemplate
                        {
                            SubAccountCode = "20002",
                            Name = "Other Supplier",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Credit Payment" }
                            }
                        }
                    }
                },

                // 3. CASH (Current Assets)
                new MainAccountTemplate
                {
                    MainAccountCode = "110",
                    Name = "Cash",
                    FinancialStatementComponent = "CurrentAssets",
                    SubAccounts = new List<SubAccountTemplate>
                    {
                        new SubAccountTemplate
                        {
                            SubAccountCode = "11001",
                            Name = "Cash",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Cash in Hand" }
                            }
                        }
                    }
                },

                // 4. BANKS (Current Assets)
                new MainAccountTemplate
                {
                    MainAccountCode = "120",
                    Name = "Banks",
                    FinancialStatementComponent = "CurrentAssets",
                    SubAccounts = new List<SubAccountTemplate>
                    {
                        new SubAccountTemplate
                        {
                            SubAccountCode = "12001",
                            Name = "Banks",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Bank Account" }
                            }
                        }
                    }
                },

                // 5. REVENUE
                new MainAccountTemplate
                {
                    MainAccountCode = "400",
                    Name = "Revenue",
                    FinancialStatementComponent = "Revenue",
                    SubAccounts = new List<SubAccountTemplate>
                    {
                        new SubAccountTemplate
                        {
                            SubAccountCode = "40001",
                            Name = "Revenue",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Sales" }
                            }
                        }
                    }
                },

                // 6. COST OF SALES
                new MainAccountTemplate
                {
                    MainAccountCode = "500",
                    Name = "Cost of Sales",
                    FinancialStatementComponent = "CostOfSales",
                    SubAccounts = new List<SubAccountTemplate>
                    {
                        new SubAccountTemplate
                        {
                            SubAccountCode = "50001",
                            Name = "Material",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Purchases" }
                            }
                        }
                    }
                },

                // 7. OPERATING EXPENSES
                new MainAccountTemplate
                {
                    MainAccountCode = "600",
                    Name = "Operating Expenses",
                    FinancialStatementComponent = "OperatingExpenses",
                    SubAccounts = new List<SubAccountTemplate>
                    {
                        new SubAccountTemplate
                        {
                            SubAccountCode = "60001",
                            Name = "Collection Expenses",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Petrol Expense" },
                                new AccountTemplate { Name = "Repairing Expenses" }
                            }
                        },
                        new SubAccountTemplate
                        {
                            SubAccountCode = "60002",
                            Name = "Storage Expenses",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Chillar Repairing Expense" },
                                new AccountTemplate { Name = "Electricity Expense" }
                            }
                        },
                        new SubAccountTemplate
                        {
                            SubAccountCode = "60003",
                            Name = "Employees Salary",
                            Accounts = new List<AccountTemplate>
                            {
                                new AccountTemplate { Name = "Staff Salary" }
                            }
                        }
                    }
                }
            };
        }
    }

    /// <summary>
    /// Template for Main Account configuration
    /// </summary>
    public class MainAccountTemplate
    {
        public string MainAccountCode { get; set; }
        public string Name { get; set; }
        public string FinancialStatementComponent { get; set; }
        public List<SubAccountTemplate> SubAccounts { get; set; } = new();
    }

    /// <summary>
    /// Template for Sub Account configuration
    /// </summary>
    public class SubAccountTemplate
    {
        public string SubAccountCode { get; set; }
        public string Name { get; set; }
        public List<AccountTemplate> Accounts { get; set; } = new();
    }

    /// <summary>
    /// Template for Account configuration
    /// </summary>
    public class AccountTemplate
    {
        public string Name { get; set; }
    }
}
