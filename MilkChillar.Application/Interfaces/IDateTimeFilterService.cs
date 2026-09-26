using MilkChillar.Domain.Entities;

namespace MilkChillar.Application.Interfaces
{
    /// <summary>
    /// Service for applying date/time filters to different entities
    /// Provides consistent filtering logic across all reports
    /// </summary>
    public interface IDateTimeFilterService
    {
        /// <summary>
        /// Apply date/time filter to ChillarReceive query
        /// 
        /// Logic:
        /// - Same day: morning-morning (only morning), morning-evening (full day), evening-evening (only evening)
        /// - Multi-day: start date uses start time, end date uses end time, middle dates include full day
        /// </summary>
        IQueryable<ChillarReceive> ApplyChillarReceiveTimeFilter(
            IQueryable<ChillarReceive> query,
            DateOnly dateStart,
            DateOnly dateEnd,
            string? startTimeOfDay,
            string? endTimeOfDay);

        /// <summary>
        /// Apply date/time filter to Purchase query
        /// Same logic as ChillarReceive
        /// </summary>
        IQueryable<Purchase> ApplyPurchaseTimeFilter(
            IQueryable<Purchase> query,
            DateOnly dateStart,
            DateOnly dateEnd,
            string? startTimeOfDay,
            string? endTimeOfDay);

        /// <summary>
        /// Apply date/time filter to Sales query
        /// 
        /// Special Logic (no TimeOfDay field in Sales):
        /// - StartDate with "morning" ? Include sales of startDate
        /// - StartDate with "evening" ? DO NOT include sales of startDate
        /// - EndDate (both "morning" & "evening") ? Always include sales of endDate
        /// </summary>
        IQueryable<Sales> ApplySalesTimeFilter(
            IQueryable<Sales> query,
            DateOnly dateStart,
            DateOnly dateEnd,
            string? startTimeOfDay,
            string? endTimeOfDay);
    }
}
