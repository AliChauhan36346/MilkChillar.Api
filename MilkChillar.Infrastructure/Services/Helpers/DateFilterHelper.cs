using MilkChillar.Application.Parameters;
using System;
using System.Linq;

namespace MilkChillar.Infrastructure.Services.Helpers
{
    /// <summary>
    /// Helper class for date range and time of day filtering logic
    /// </summary>
    public static class DateFilterHelper
    {
        /// <summary>
        /// Apply date and time of day filters to a query
        /// </summary>
        public static IQueryable<T> ApplyDateFilter<T>(
            IQueryable<T> query,
            DateTime startDate,
            DateTime endDate,
            string timeOfDay,
            Func<T, DateTime> dateSelector,
            Func<T, string> timeSelector) where T : class
        {
            if (startDate == default || endDate == default)
                return query;

            return query.Where(x =>
                (dateSelector(x) > startDate && dateSelector(x) < endDate) ||
                (dateSelector(x) == startDate &&
                    (string.IsNullOrWhiteSpace(timeOfDay) ||
                     timeOfDay.ToLower() == "morning" ||
                     (timeOfDay.ToLower() == "evening" && timeSelector(x).ToLower() == "evening"))) ||
                (dateSelector(x) == endDate &&
                    (string.IsNullOrWhiteSpace(timeOfDay) ||
                     timeOfDay.ToLower() == "evening" ||
                     (timeOfDay.ToLower() == "morning" && timeSelector(x).ToLower() == "morning")))
            );
        }

        /// <summary>
        /// Get previous stock date range (before StartDate, adjusted for time of day)
        /// </summary>
        public static (DateTime Start, DateTime End) GetPreviousStockRange(
            DateTime startDate,
            string startTimeOfDay)
        {
            return (DateTime.MinValue, startDate);
        }

        /// <summary>
        /// Check if should include morning transactions on start date
        /// </summary>
        public static bool IncludeMorningOnStartDate(string startTimeOfDay)
        {
            return string.IsNullOrWhiteSpace(startTimeOfDay) || startTimeOfDay.ToLower() == "morning";
        }

        /// <summary>
        /// Check if should include evening transactions on start date
        /// </summary>
        public static bool IncludeEveningOnStartDate(string startTimeOfDay)
        {
            return string.IsNullOrWhiteSpace(startTimeOfDay) || startTimeOfDay.ToLower() == "evening";
        }

        /// <summary>
        /// Check if should include all transactions on end date
        /// </summary>
        public static bool IncludeAllOnEndDate(string endTimeOfDay)
        {
            return string.IsNullOrWhiteSpace(endTimeOfDay) || endTimeOfDay.ToLower() == "evening";
        }
    }
}
