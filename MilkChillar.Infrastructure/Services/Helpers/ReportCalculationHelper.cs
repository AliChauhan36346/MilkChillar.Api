namespace MilkChillar.Infrastructure.Services.Helpers
{
    /// <summary>
    /// Helper class for common report calculations (summaries, averages, etc.)
    /// </summary>
    public static class ReportCalculationHelper
    {
        /// <summary>
        /// Calculate average rate (amount / quantity)
        /// </summary>
        public static decimal CalculateAverageRate(decimal totalAmount, decimal totalQuantity)
        {
            return totalQuantity > 0 ? totalAmount / totalQuantity : 0;
        }

        /// <summary>
        /// Calculate net balance (previous + current - sales)
        /// </summary>
        public static decimal CalculateNetBalance(decimal previousStock, decimal receives, decimal sales)
        {
            return previousStock + receives - sales;
        }

        /// <summary>
        /// Calculate gross profit (sales amount - purchase cost)
        /// </summary>
        public static decimal CalculateGrossProfit(decimal salesAmount, decimal purchaseAmount)
        {
            return salesAmount - purchaseAmount;
        }

        /// <summary>
        /// Calculate loss percentage
        /// </summary>
        public static decimal CalculateLossPercentage(decimal grossLiters, decimal netLiters)
        {
            if (grossLiters == 0) return 0;
            return ((grossLiters - netLiters) / grossLiters) * 100;
        }

        /// <summary>
        /// Calculate average value from list of items
        /// </summary>
        public static decimal CalculateAverage(decimal total, int count)
        {
            return count > 0 ? total / count : 0;
        }
    }
}
