using System;

namespace MilkChillar.Infrastructure.Services.Helpers
{
    /// <summary>
    /// Helper class to map account types to their corresponding account code prefixes
    /// </summary>
    public static class AccountCodeHelper
    {
        /// <summary>
        /// Get account code prefix and whether it's a supplier account (affects balance calculation)
        /// </summary>
        public static (string Prefix, bool IsSupplier) GetAccountCodePrefix(string accountType)
        {
            return accountType.ToLower() switch
            {
                "supplier" => ("200", true),
                "buyer" => ("100", false),
                "cash" => ("110", false),
                "bank" => ("120", false),
                _ => throw new ArgumentException("Account type must be 'Supplier', 'Buyer', 'Cash', or 'Bank'")
            };
        }

        /// <summary>
        /// Calculate balance based on account type
        /// </summary>
        public static decimal CalculateBalance(decimal debitTotal, decimal creditTotal, bool isSupplier)
        {
            return isSupplier
                ? creditTotal - debitTotal  // Suppliers: Credit - Debit (we owe them)
                : debitTotal - creditTotal; // Others: Debit - Credit
        }
    }
}
