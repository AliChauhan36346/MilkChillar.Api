using MilkChillar.Application.DTOs.Dashboard;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application.DTOs.ChillarReceive;
using MilkChillar.Application.DTOs.Purchase;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.DTOs.Sales;

namespace MilkChillar.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(DashboardStatsQuery query, int tenantId)
        {
            var purchases = _dbContext.Purchases.AsQueryable();
            var receives = _dbContext.ChillarReceives.AsQueryable();
            var employees = _dbContext.Employees.AsQueryable();

            // Tenant and Date filter
            purchases = purchases.Where(p => p.TenantId == tenantId && p.Date >= query.StartDate && p.Date <= query.EndDate);
            receives = receives.Where(r => r.TenantId == tenantId && r.Date >= query.StartDate && r.Date <= query.EndDate);

            // Time of day filter
            if (!string.IsNullOrWhiteSpace(query.TimeOfDay))
            {
                var time = query.TimeOfDay.ToLower();
                purchases = purchases.Where(p => p.TimeOfDay.ToLower() == time);
                receives = receives.Where(r => r.TimeOfDay.ToLower() == time);
            }

            // Dodhi Filter
            if (query.DodhiId.HasValue)
            {
                purchases = purchases.Where(p => p.DodhiId == query.DodhiId);
                receives = receives.Where(r => r.DodhiId == query.DodhiId);
            }

            // ChillarId filter (by dodhi association)
            if (query.ChillarId.HasValue)
            {
                var chillarId = query.ChillarId.Value;

                // All employees (dodhis) assigned to this chillar
                var dodhiIds = await employees
                    .Where(e => e.TenantId == tenantId && e.ChillarId == chillarId)
                    .Select(e => e.EmployeeId)
                    .ToListAsync();

                purchases = purchases.Where(p => dodhiIds.Contains(p.DodhiId));
                receives = receives.Where(r => r.ChillarId == chillarId);
            }

            var totalPurchaseLiters = await purchases.SumAsync(p => p.GrossLiters);
            var totalReceiveLiters = await receives.SumAsync(r => r.GrossLiters);

            return new DashboardStatsDto
            {
                TotalPurchaseLiters = totalPurchaseLiters,
                TotalReceivedLiters = totalReceiveLiters
            };
        }

        public async Task<DodhiPurchaseReportDto> GetDashboardRecordsAsync(DashboardStatsQuery query, int tenantId)
        {
            var purchasesQuery = _dbContext.Purchases
                .Where(p => p.TenantId == tenantId &&
                            p.Date >= query.StartDate &&
                            p.Date <= query.EndDate);

            var receivesQuery = _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId &&
                            r.Date >= query.StartDate &&
                            r.Date <= query.EndDate);

            // Time of Day filter
            if (!string.IsNullOrWhiteSpace(query.TimeOfDay))
            {
                var time = query.TimeOfDay.ToLower();
                purchasesQuery = purchasesQuery.Where(p => p.TimeOfDay.ToLower() == time);
                receivesQuery = receivesQuery.Where(r => r.TimeOfDay.ToLower() == time);
            }

            // Dodhi filter
            if (query.DodhiId.HasValue)
            {
                purchasesQuery = purchasesQuery.Where(p => p.DodhiId == query.DodhiId.Value);
                receivesQuery = receivesQuery.Where(r => r.DodhiId == query.DodhiId.Value);
            }

            // Chillar filter (by dodhi association)
            if (query.ChillarId.HasValue)
            {
                var chillarId = query.ChillarId.Value;

                var dodhiIds = await _dbContext.Employees
                    .Where(e => e.TenantId == tenantId && e.ChillarId == chillarId)
                    .Select(e => e.EmployeeId)
                    .ToListAsync();

                purchasesQuery = purchasesQuery.Where(p => dodhiIds.Contains(p.DodhiId));
                receivesQuery = receivesQuery.Where(r => r.ChillarId == chillarId);
            }

            // Project to DTOs
            var purchaseDtos = await purchasesQuery
                .Select(p => new PurchaseDto
                {
                    PurchaseId = p.PurchaseId,
                    Date = p.Date,
                    TimeOfDay = p.TimeOfDay,
                    AccountId = p.AccountId,
                    AccountName = p.Account.Name,
                    AccountCode = p.Account.AccountCode,
                    ExpenseAccountName = p.ExpenseAccount.Name,
                    DodhiName = p.Dodhi.FullName,
                    GrossLiters = p.GrossLiters,
                    Rate = p.Rate,
                    TotalAmount = p.GrossLiters * p.Rate,
                    Balance = p.Balance
                })
                .ToListAsync();

            var receiveDtos = await receivesQuery
                .Select(r => new ChillarReceiveDto
                {
                    ReceiveId = r.ReceiveId,
                    Date = r.Date,
                    TimeOfDay = r.TimeOfDay,
                    ChillarName = r.Chillar.Name,
                    InchargeName = r.ChillarIncharge.FullName,
                    DodhiName = r.Dodhi.FullName,
                    dodhiID = r.DodhiId,
                    GrossLiters = r.GrossLiters,
                    LR = r.LR,
                    Fat = r.Fat,
                    NetLiters = r.NetLiters
                })
                .ToListAsync();

            return new DodhiPurchaseReportDto
            {
                Purchases = purchaseDtos,
                Receives = receiveDtos
            };
        }

        //    public async Task<ChillarInchargeDashboardStatsDto> GetChillarInchargeDashboardStatsAsync(
        //ChillarInchargeDashboardQuery query,
        //int tenantId)
        //    {
        //        var receives = _dbContext.ChillarReceives.AsQueryable();
        //        var sales = _dbContext.Sales.AsQueryable();

        //        // Filter by tenant
        //        receives = receives.Where(r => r.TenantId == tenantId);
        //        sales = sales.Where(s => s.TenantId == tenantId);

        //        // ChillarId filter
        //        if (query.ChillarId.HasValue)
        //        {
        //            receives = receives.Where(r => r.ChillarId == query.ChillarId.Value);
        //            sales = sales.Where(s => s.ChillarId == query.ChillarId.Value);
        //        }

        //        // ChillarIncharge filter
        //        if (query.ChillarInchargeId.HasValue)
        //        {
        //            receives = receives.Where(r => r.ChillarInchargeId == query.ChillarInchargeId.Value);
        //            // Sales don't have incharge directly — only filtered by chillarId
        //        }

        //        // Previous stock calculation
        //        var prevReceivesQuery = receives.Where(r => r.Date < query.StartDate);
        //        var prevSalesQuery = sales.Where(s => s.Date < query.StartDate);

        //        // If StartTimeOfDay = "evening", also include only morning receives of StartDate in prev stock
        //        if (!string.IsNullOrWhiteSpace(query.StartTimeOfDay) && query.StartTimeOfDay.ToLower() == "evening")
        //        {
        //            prevReceivesQuery = prevReceivesQuery
        //                .Concat(receives.Where(r => r.Date == query.StartDate && r.TimeOfDay.ToLower() == "morning"));
        //            // Sales from StartDate are NOT included in prev stock in this case
        //        }
        //        else if (!string.IsNullOrWhiteSpace(query.StartTimeOfDay) && query.StartTimeOfDay.ToLower() == "morning")
        //        {
        //            // Include morning receives of that day in range, sales are part of selected range
        //        }

        //        var previousStockLiters =
        //            await prevReceivesQuery.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m
        //            - await prevSalesQuery.SumAsync(s => (decimal?)s.GrossLiters) ?? 0m;

        //        // Selected range filter for receives
        //        var receivesQuery = receives.Where(r =>
        //            (r.Date > query.StartDate && r.Date < query.EndDate) ||
        //            (r.Date == query.StartDate &&
        //                (string.IsNullOrWhiteSpace(query.StartTimeOfDay)
        //                 || r.TimeOfDay.ToLower() == query.StartTimeOfDay.ToLower()
        //                 || query.StartTimeOfDay.ToLower() == "morning" && r.TimeOfDay.ToLower() == "morning"
        //                 || query.StartTimeOfDay.ToLower() == "evening" && r.TimeOfDay.ToLower() == "evening")) ||
        //            (r.Date == query.EndDate &&
        //                (string.IsNullOrWhiteSpace(query.EndTimeOfDay)
        //                 || query.EndTimeOfDay.ToLower() == "evening"
        //                 || (query.EndTimeOfDay.ToLower() == "morning" && r.TimeOfDay.ToLower() == "morning")))
        //        );

        //        // Selected range filter for sales
        //        var salesQuery = sales.Where(s =>
        //            (s.Date > query.StartDate && s.Date < query.EndDate) ||
        //            (s.Date == query.EndDate) ||
        //            (s.Date == query.StartDate &&
        //                (string.IsNullOrWhiteSpace(query.StartTimeOfDay) || query.StartTimeOfDay.ToLower() == "morning"))
        //        );

        //        var totalReceives = await receivesQuery.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;
        //        var totalSales = await salesQuery.SumAsync(s => (decimal?)s.GrossLiters) ?? 0m;

        //        var currentStock = (previousStockLiters + totalReceives) - totalSales;

        //        return new ChillarInchargeDashboardStatsDto
        //        {
        //            PreviousStock = previousStockLiters,
        //            TotalChillarReceive = totalReceives,
        //            TotalSales = totalSales,
        //            CurrentStock = currentStock
        //        };
        //    }



        public async Task<ChillarInchargeDashboardStatsDto> GetChillarInchargeDashboardStatsAsync(
    ChillarInchargeDashboardQuery query,
    int tenantId)
        {
            var receives = _dbContext.ChillarReceives.AsQueryable();
            var sales = _dbContext.Sales.AsQueryable();

            // Filter by tenant
            receives = receives.Where(r => r.TenantId == tenantId);
            sales = sales.Where(s => s.TenantId == tenantId);

            // ChillarId filter
            if (query.ChillarId.HasValue)
            {
                receives = receives.Where(r => r.ChillarId == query.ChillarId.Value);
                sales = sales.Where(s => s.ChillarId == query.ChillarId.Value);
            }

            // ChillarIncharge filter
            if (query.ChillarInchargeId.HasValue)
            {
                receives = receives.Where(r => r.ChillarInchargeId == query.ChillarInchargeId.Value);
            }

            // Previous stock calculation - FIXED
            var prevReceivesQuery = receives.Where(r => r.Date < query.StartDate);
            var prevSalesQuery = sales.Where(s => s.Date < query.StartDate);

            // Handle StartDate adjustments for previous stock
            if (!string.IsNullOrWhiteSpace(query.StartTimeOfDay) && query.StartTimeOfDay.ToLower() == "evening")
            {
                // If starting from evening, add morning receives and all sales from StartDate to previous stock
                prevReceivesQuery = prevReceivesQuery
                    .Concat(receives.Where(r => r.Date == query.StartDate && r.TimeOfDay.ToLower() == "morning"));
                prevSalesQuery = prevSalesQuery
                    .Concat(sales.Where(s => s.Date == query.StartDate));
            }

            // Previous stock calculation - Build base queries
            var prevReceivesQueryb = receives.Where(r => r.Date < query.StartDate);
            var prevSalesQueryb = sales.Where(s => s.Date < query.StartDate);

            // Handle StartDate adjustments for previous stock
            if (!string.IsNullOrWhiteSpace(query.StartTimeOfDay) && query.StartTimeOfDay.ToLower() == "evening")
            {
                // If starting from evening, add morning receives and all sales from StartDate to previous stock
                prevReceivesQuery = prevReceivesQuery
                    .Concat(receives.Where(r => r.Date == query.StartDate && r.TimeOfDay.ToLower() == "morning"));
                prevSalesQuery = prevSalesQuery
                    .Concat(sales.Where(s => s.Date == query.StartDate));
            }

            // Calculate previous stock properly: Receives - Sales
            var prevReceivesTotal = await prevReceivesQuery.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;
            var prevSalesTotal = await prevSalesQuery.SumAsync(s => (decimal?)s.GrossLiters) ?? 0m;
            var previousStockLiters = prevReceivesTotal - prevSalesTotal;

            // Debug logging to identify the issue
            var debugPrevReceivesCount = await prevReceivesQuery.CountAsync();
            var debugPrevSalesCount = await prevSalesQuery.CountAsync();
            System.Diagnostics.Debug.WriteLine($"Debug - StartDate: {query.StartDate}");
            System.Diagnostics.Debug.WriteLine($"Debug - Previous Receives Count: {debugPrevReceivesCount}, Total: {prevReceivesTotal}");
            System.Diagnostics.Debug.WriteLine($"Debug - Previous Sales Count: {debugPrevSalesCount}, Total: {prevSalesTotal}");
            System.Diagnostics.Debug.WriteLine($"Debug - Previous Stock: {previousStockLiters}");

            // Current range receives filter
            var receivesQuery = receives.Where(r =>
                // Include dates strictly between StartDate and EndDate
                (r.Date > query.StartDate && r.Date < query.EndDate) ||

                // Handle StartDate inclusion based on StartTimeOfDay
                (r.Date == query.StartDate &&
                    (string.IsNullOrWhiteSpace(query.StartTimeOfDay) || // Include all if no time specified
                     query.StartTimeOfDay.ToLower() == "morning" || // Include all if starting from morning
                     (query.StartTimeOfDay.ToLower() == "evening" && r.TimeOfDay.ToLower() == "evening"))) || // Only evening if starting from evening

                // Handle EndDate inclusion based on EndTimeOfDay
                (r.Date == query.EndDate &&
                    (string.IsNullOrWhiteSpace(query.EndTimeOfDay) || // Include all if no end time specified
                     query.EndTimeOfDay.ToLower() == "evening" || // Include all if ending at evening
                     (query.EndTimeOfDay.ToLower() == "morning" && r.TimeOfDay.ToLower() == "morning"))) // Only morning if ending at morning
            );

            // Current range sales filter
            var salesQuery = sales.Where(s =>
                // Include dates strictly between StartDate and EndDate
                (s.Date > query.StartDate && s.Date < query.EndDate) ||

                // Handle StartDate inclusion - only if starting from morning or no time specified
                (s.Date == query.StartDate &&
                    (string.IsNullOrWhiteSpace(query.StartTimeOfDay) || query.StartTimeOfDay.ToLower() == "morning")) ||

                // Handle EndDate inclusion - CORRECTED LOGIC
                // Include ALL sales from EndDate regardless of EndTimeOfDay (since sales don't have time)
                (s.Date == query.EndDate)
            );

            var totalReceives = await receivesQuery.SumAsync(r => (decimal?)r.GrossLiters) ?? 0m;
            var totalSales = await salesQuery.SumAsync(s => (decimal?)s.GrossLiters) ?? 0m;
            var currentStock = (previousStockLiters + totalReceives) - totalSales;

            return new ChillarInchargeDashboardStatsDto
            {
                PreviousStock = previousStockLiters,
                TotalChillarReceive = totalReceives,
                TotalSales = totalSales,
                CurrentStock = currentStock
            };
        }

        public async Task<List<ChillarReceiveDto>> GetChillarReceiveRecordsAsync(
        ChillarInchargeDashboardQuery query,
        int tenantId)
        {
            var receives = _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId);

            // ✅ Filter by Chillar
            if (query.ChillarId.HasValue)
                receives = receives.Where(r => r.ChillarId == query.ChillarId.Value);

            // ✅ Filter by Chillar Incharge
            if (query.ChillarInchargeId.HasValue)
                receives = receives.Where(r => r.ChillarInchargeId == query.ChillarInchargeId.Value);

            // ✅ Filter by Dodhi
            if (query.DodhiId.HasValue)
                receives = receives.Where(r => r.DodhiId == query.DodhiId.Value);

            // ✅ Date filters with morning/evening logic
            if (query.StartDate != default && query.EndDate != default)
            {
                // Start date logic
                if (!string.IsNullOrEmpty(query.StartTimeOfDay))
                {
                    if (query.StartTimeOfDay.Equals("morning", StringComparison.OrdinalIgnoreCase))
                    {
                        receives = receives.Where(r =>
                            (r.Date > query.StartDate) ||
                            (r.Date == query.StartDate && r.TimeOfDay.ToLower() == "morning"));
                    }
                    else if (query.StartTimeOfDay.Equals("evening", StringComparison.OrdinalIgnoreCase))
                    {
                        receives = receives.Where(r =>
                            (r.Date > query.StartDate) ||
                            (r.Date == query.StartDate && r.TimeOfDay.ToLower() == "evening"));
                    }
                }
                else
                {
                    receives = receives.Where(r => r.Date >= query.StartDate);
                }

                // End date logic
                if (!string.IsNullOrEmpty(query.EndTimeOfDay))
                {
                    if (query.EndTimeOfDay.Equals("morning", StringComparison.OrdinalIgnoreCase))
                    {
                        receives = receives.Where(r =>
                            (r.Date < query.EndDate) ||
                            (r.Date == query.EndDate && r.TimeOfDay.ToLower() == "morning"));
                    }
                    else if (query.EndTimeOfDay.Equals("evening", StringComparison.OrdinalIgnoreCase))
                    {
                        receives = receives.Where(r => r.Date <= query.EndDate);
                    }
                }
                else
                {
                    receives = receives.Where(r => r.Date <= query.EndDate);
                }
            }

            // ✅ Projection to DTO
            var result = await receives
                .Select(r => new ChillarReceiveDto
                {
                    ReceiveId = r.ReceiveId,
                    Date = r.Date,
                    TimeOfDay = r.TimeOfDay,
                    ChillarName = r.Chillar.Name,
                    InchargeName = r.ChillarIncharge.FullName,
                    DodhiName = r.Dodhi.FullName,
                    dodhiID = r.DodhiId,
                    GrossLiters = r.GrossLiters,
                    LR = r.LR,
                    Fat = r.Fat,
                    NetLiters = r.NetLiters
                })
                .OrderBy(r => r.Date)
                .ThenBy(r => r.TimeOfDay)
                .ToListAsync();

            return result;
        }

        public async Task<List<SaleDto>> GetSalesReportAsync(SalesReportQuery query, int tenantId)
        {
            var sales = _dbContext.Sales
                .Where(s => s.TenantId == tenantId);

            if (query.ChillarId.HasValue)
            {
                sales = sales.Where(s => s.ChillarId == query.ChillarId.Value);
            }


            // ✅ Date Range Filter
            if (query.StartDate != default && query.EndDate != default)
            {
                sales = sales.Where(s => s.Date >= query.StartDate && s.Date <= query.EndDate);
            }

            // ✅ Buyer Code Filter
            if (!string.IsNullOrEmpty(query.BuyerCode))
            {
                sales = sales.Where(s => s.Account.AccountCode == query.BuyerCode);
            }

            // ✅ Projection
            var result = await sales
                .Select(s => new SaleDto
                {
                    SaleId = s.SaleId,
                    Date = s.Date,
                    AccountId = s.AccountId,
                    AccountCode = s.Account.AccountCode,
                    AccountName = s.Account.Name,
                    RevenueAccountName = s.RevenueAccount.Name,
                    ChillarName = s.Chillar.Name,
                    AddedByName = s.User.Username,
                    GrossLiters = s.GrossLiters,
                    LR = s.LR,
                    Fat = s.Fat,
                    NetLiters = s.NetLiters,
                    Rate = s.Rate,
                    TotalAmount = s.NetLiters * s.Rate,
                    AmountReceived = s.AmountReceived,
                    Balance = s.Balance
                })
                .OrderBy(s => s.Date)
                .ThenBy(s => s.AccountName)
                .ToListAsync();

            return result;
        }

    }
}
