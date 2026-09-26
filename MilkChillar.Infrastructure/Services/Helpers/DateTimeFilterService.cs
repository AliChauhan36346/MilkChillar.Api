using MilkChillar.Domain.Entities;
using MilkChillar.Application.Interfaces;

namespace MilkChillar.Infrastructure.Services.Helpers
{
    /// <summary>
    /// Implementation of date/time filtering for different entities
    /// Provides consistent filtering logic across all reports
    /// </summary>
    public class DateTimeFilterService : IDateTimeFilterService
    {
        public IQueryable<ChillarReceive> ApplyChillarReceiveTimeFilter(
            IQueryable<ChillarReceive> query,
            DateOnly dateStart,
            DateOnly dateEnd,
            string? startTimeOfDay,
            string? endTimeOfDay)
        {
            // If no time filters, return as is
            if (string.IsNullOrWhiteSpace(startTimeOfDay) && string.IsNullOrWhiteSpace(endTimeOfDay))
                return query;

            // Same day filtering
            if (dateStart == dateEnd)
            {
                if (!string.IsNullOrWhiteSpace(startTimeOfDay) && !string.IsNullOrWhiteSpace(endTimeOfDay))
                {
                    var startTime = startTimeOfDay.ToLower();
                    var endTime = endTimeOfDay.ToLower();

                    if (startTime == endTime)
                    {
                        // Both same time: only that time
                        query = query.Where(r => r.Date == dateStart && r.TimeOfDay.ToLower() == startTime);
                    }
                    else if (startTime == "morning" && endTime == "evening")
                    {
                        // Morning to evening on same day: full day
                        query = query.Where(r => r.Date == dateStart);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(startTimeOfDay))
                {
                    var startTime = startTimeOfDay.ToLower();
                    query = query.Where(r => r.Date == dateStart && r.TimeOfDay.ToLower() == startTime);
                }
                else if (!string.IsNullOrWhiteSpace(endTimeOfDay))
                {
                    var endTime = endTimeOfDay.ToLower();
                    query = query.Where(r => r.Date == dateStart && r.TimeOfDay.ToLower() == endTime);
                }
            }
            else
            {
                // Multi-day filtering
                // Start date filter
                if (!string.IsNullOrWhiteSpace(startTimeOfDay))
                {
                    var startTime = startTimeOfDay.ToLower();
                    if (startTime == "morning")
                    {
                        query = query.Where(r =>
                            (r.Date > dateStart) ||
                            (r.Date == dateStart && r.TimeOfDay.ToLower() == "morning"));
                    }
                    else if (startTime == "evening")
                    {
                        query = query.Where(r =>
                            (r.Date > dateStart) ||
                            (r.Date == dateStart && r.TimeOfDay.ToLower() == "evening"));
                    }
                }
                else
                {
                    query = query.Where(r => r.Date >= dateStart);
                }

                // End date filter
                if (!string.IsNullOrWhiteSpace(endTimeOfDay))
                {
                    var endTime = endTimeOfDay.ToLower();
                    if (endTime == "morning")
                    {
                        query = query.Where(r =>
                            (r.Date < dateEnd) ||
                            (r.Date == dateEnd && r.TimeOfDay.ToLower() == "morning"));
                    }
                    else if (endTime == "evening")
                    {
                        query = query.Where(r => r.Date <= dateEnd);
                    }
                }
                else
                {
                    query = query.Where(r => r.Date <= dateEnd);
                }
            }

            return query;
        }

        public IQueryable<Purchase> ApplyPurchaseTimeFilter(
            IQueryable<Purchase> query,
            DateOnly dateStart,
            DateOnly dateEnd,
            string? startTimeOfDay,
            string? endTimeOfDay)
        {
            // Same logic as ChillarReceive
            if (string.IsNullOrWhiteSpace(startTimeOfDay) && string.IsNullOrWhiteSpace(endTimeOfDay))
                return query;

            // Same day filtering
            if (dateStart == dateEnd)
            {
                if (!string.IsNullOrWhiteSpace(startTimeOfDay) && !string.IsNullOrWhiteSpace(endTimeOfDay))
                {
                    var startTime = startTimeOfDay.ToLower();
                    var endTime = endTimeOfDay.ToLower();

                    if (startTime == endTime)
                    {
                        query = query.Where(p => p.Date == dateStart && p.TimeOfDay.ToLower() == startTime);
                    }
                    else if (startTime == "morning" && endTime == "evening")
                    {
                        query = query.Where(p => p.Date == dateStart);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(startTimeOfDay))
                {
                    var startTime = startTimeOfDay.ToLower();
                    query = query.Where(p => p.Date == dateStart && p.TimeOfDay.ToLower() == startTime);
                }
                else if (!string.IsNullOrWhiteSpace(endTimeOfDay))
                {
                    var endTime = endTimeOfDay.ToLower();
                    query = query.Where(p => p.Date == dateStart && p.TimeOfDay.ToLower() == endTime);
                }
            }
            else
            {
                // Multi-day filtering
                if (!string.IsNullOrWhiteSpace(startTimeOfDay))
                {
                    var startTime = startTimeOfDay.ToLower();
                    if (startTime == "morning")
                    {
                        query = query.Where(p =>
                            (p.Date > dateStart) ||
                            (p.Date == dateStart && p.TimeOfDay.ToLower() == "morning"));
                    }
                    else if (startTime == "evening")
                    {
                        query = query.Where(p =>
                            (p.Date > dateStart) ||
                            (p.Date == dateStart && p.TimeOfDay.ToLower() == "evening"));
                    }
                }
                else
                {
                    query = query.Where(p => p.Date >= dateStart);
                }

                if (!string.IsNullOrWhiteSpace(endTimeOfDay))
                {
                    var endTime = endTimeOfDay.ToLower();
                    if (endTime == "morning")
                    {
                        query = query.Where(p =>
                            (p.Date < dateEnd) ||
                            (p.Date == dateEnd && p.TimeOfDay.ToLower() == "morning"));
                    }
                    else if (endTime == "evening")
                    {
                        query = query.Where(p => p.Date <= dateEnd);
                    }
                }
                else
                {
                    query = query.Where(p => p.Date <= dateEnd);
                }
            }

            return query;
        }

        public IQueryable<Sales> ApplySalesTimeFilter(
            IQueryable<Sales> query,
            DateOnly dateStart,
            DateOnly dateEnd,
            string? startTimeOfDay,
            string? endTimeOfDay)
        {
            // Special logic for Sales (no TimeOfDay field)
            // StartDate logic:
            // - "morning" ? Include sales of startDate
            // - "evening" ? DO NOT include sales of startDate
            // EndDate logic:
            // - Both "morning" & "evening" ? Always include sales of endDate

            if (string.IsNullOrWhiteSpace(startTimeOfDay) && string.IsNullOrWhiteSpace(endTimeOfDay))
            {
                // No time filters: apply only date range
                return query.Where(s => s.Date >= dateStart && s.Date <= dateEnd);
            }

            // Start date logic
            if (!string.IsNullOrWhiteSpace(startTimeOfDay))
            {
                var startTime = startTimeOfDay.ToLower();
                if (startTime == "morning")
                {
                    // Include sales of startDate
                    query = query.Where(s => s.Date >= dateStart);
                }
                else if (startTime == "evening")
                {
                    // DO NOT include sales of startDate
                    query = query.Where(s => s.Date > dateStart);
                }
            }
            else
            {
                // No start time specified
                query = query.Where(s => s.Date >= dateStart);
            }

            // End date logic
            if (!string.IsNullOrWhiteSpace(endTimeOfDay))
            {
                // Both "morning" & "evening": Always include sales of endDate
                query = query.Where(s => s.Date <= dateEnd);
            }
            else
            {
                // No end time specified
                query = query.Where(s => s.Date <= dateEnd);
            }

            return query;
        }
    }
}
