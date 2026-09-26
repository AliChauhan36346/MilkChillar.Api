namespace MilkChillar.Application.Interfaces
{
    /// <summary>
    /// Service for calculating stock before a given date/time with optional time filters
    /// Used across multiple reports (DailyTotals, ChillarIncharge Dashboard, Dodhi Summary, etc.)
    /// </summary>
    public interface IStockCalculationService
    {
        /// <summary>
        /// Calculate previous stock before the filtered period starts
        /// 
        /// Logic:
        /// - No time filter: (ChillarReceive - Sales) before startDate
        /// - StartTime = Morning: (ChillarReceive - Sales) before startDate (exclude startDate)
        /// - StartTime = Evening: (ChillarReceive - Sales) before startDate + morning ChillarReceive of startDate + Sales of startDate
        /// </summary>
        Task<decimal> CalculatePreviousStockAsync(
            int chillarId,
            DateOnly startDate,
            string? startTimeOfDay,
            int tenantId);

        /// <summary>
        /// Calculate previous stock for a specific dodhi
        /// </summary>
        Task<decimal> CalculatePreviousStockForDodhiAsync(
            int dodhiId,
            int chillarId,
            DateOnly startDate,
            string? startTimeOfDay,
            int tenantId);
    }
}
