using MilkChillar.Application;
using MilkChillar.Application.DTOs.Reports;
using MilkChillar.Application.Parameters;
using MilkChillar.Domain.Entities;
using MilkChillar.Infrastructure.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MilkChillar.Infrastructure.Services.Reports
{
    /// <summary>
    /// Service for dodhi-wise purchase vs receive comparison reports
    /// </summary>
    public class DodhiReportService
    {
        private readonly ApplicationDbContext _dbContext;

        public DodhiReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get overall summary for a chillar (total purchases, receives, losses)
        /// </summary>
        public async Task<DodhiReportOverallSummaryDto> GetOverallDodhiSummaryAsync(
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId)
        {
            // Convert dates to DateOnly for filtering
            var dateStart = DateOnly.FromDateTime(startDate.Date);
            var dateEnd = DateOnly.FromDateTime(endDate.Date);

            // Get all dodhis in the chillar
            var dodhiIds = await _dbContext.Employees
                .Where(e => e.TenantId == tenantId && e.ChillarId == chillarId)
                .Select(e => e.EmployeeId)
                .ToListAsync();

            if (!dodhiIds.Any())
            {
                return new DodhiReportOverallSummaryDto();
            }

            // Get purchase totals
            var purchaseData = await _dbContext.Purchases
                .Where(p => p.TenantId == tenantId &&
                            dodhiIds.Contains(p.DodhiId) &&
                            p.Date >= dateStart &&
                            p.Date <= dateEnd)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalPurchasedLiters = g.Sum(p => p.GrossLiters),
                    TotalPurchaseAmount = g.Sum(p => p.GrossLiters * p.Rate),
                    PurchaseTransactionCount = g.Count()
                })
                .FirstOrDefaultAsync();

            // Get receive totals
            var receiveData = await _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId &&
                            r.ChillarId == chillarId &&
                            dodhiIds.Contains(r.DodhiId) &&
                            r.Date >= dateStart &&
                            r.Date <= dateEnd)
                .GroupBy(r => 1)
                .Select(g => new
                {
                    TotalReceivedLiters = g.Sum(r => r.GrossLiters),
                    TotalNetLiters = g.Sum(r => r.NetLiters),
                    TotalReceptionLoss = g.Sum(r => r.GrossLiters - r.NetLiters),
                    ReceiveTransactionCount = g.Count()
                })
                .FirstOrDefaultAsync();

            var totalPurchased = purchaseData?.TotalPurchasedLiters ?? 0m;
            var totalPurchaseAmount = purchaseData?.TotalPurchaseAmount ?? 0m;
            var totalReceived = receiveData?.TotalReceivedLiters ?? 0m;
            var totalNetLiters = receiveData?.TotalNetLiters ?? 0m;
            var totalLoss = receiveData?.TotalReceptionLoss ?? 0m;

            var overallSummary = new DodhiReportOverallSummaryDto
            {
                TotalPurchasedLiters = totalPurchased,
                TotalPurchaseAmount = totalPurchaseAmount,
                AveragePurchaseRate = totalPurchased > 0 ? totalPurchaseAmount / totalPurchased : 0m,
                TotalPurchaseTransactions = purchaseData?.PurchaseTransactionCount ?? 0,
                TotalReceivedLiters = totalReceived,
                TotalNetLiters = totalNetLiters,
                TotalReceptionLoss = totalLoss,
                OverallReceptionLossPercentage = ReportCalculationHelper.CalculateLossPercentage(totalReceived, totalNetLiters),
                TotalReceiveTransactions = receiveData?.ReceiveTransactionCount ?? 0,
                TotalPurchaseReceiveDifference = totalPurchased - totalReceived,
                TotalDodhis = dodhiIds.Count
            };

            return overallSummary;
        }

        /// <summary>
        /// Get dodhi-wise details (individual rows for each dodhi)
        /// </summary>
        public async Task<List<DodhiSummaryDto>> GetSingleDodhiSummaryAsync(
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId)
        {
            // Convert dates to DateOnly for filtering
            var dateStart = DateOnly.FromDateTime(startDate.Date);
            var dateEnd = DateOnly.FromDateTime(endDate.Date);

            // Get all dodhis in the chillar
            var dodhiIds = await _dbContext.Employees
                .Where(e => e.TenantId == tenantId && e.ChillarId == chillarId)
                .Select(e => e.EmployeeId)
                .ToListAsync();

            if (!dodhiIds.Any())
            {
                return new List<DodhiSummaryDto>();
            }

            // Get purchase data by dodhi
            var purchaseData = await _dbContext.Purchases
                .Include(p => p.Dodhi)
                .Where(p => p.TenantId == tenantId &&
                            dodhiIds.Contains(p.DodhiId) &&
                            p.Date >= dateStart &&
                            p.Date <= dateEnd)
                .GroupBy(p => new { p.DodhiId, p.Dodhi.FullName })
                .Select(g => new
                {
                    DodhiId = g.Key.DodhiId,
                    DodhiName = g.Key.FullName,
                    TotalPurchasedLiters = g.Sum(p => p.GrossLiters),
                    TotalPurchaseAmount = g.Sum(p => p.GrossLiters * p.Rate),
                    PurchaseTransactionCount = g.Count()
                })
                .ToListAsync();

            // Get receive data by dodhi
            var receiveData = await _dbContext.ChillarReceives
                .Include(r => r.Dodhi)
                .Include(r => r.Chillar)
                .Where(r => r.TenantId == tenantId &&
                            r.ChillarId == chillarId &&
                            dodhiIds.Contains(r.DodhiId) &&
                            r.Date >= dateStart &&
                            r.Date <= dateEnd)
                .GroupBy(r => new { r.DodhiId, r.Dodhi.FullName, r.Chillar.Name })
                .Select(g => new
                {
                    DodhiId = g.Key.DodhiId,
                    DodhiName = g.Key.FullName,
                    ChillarName = g.Key.Name,
                    TotalReceivedLiters = g.Sum(r => r.GrossLiters),
                    TotalNetLiters = g.Sum(r => r.NetLiters),
                    ReceptionLoss = g.Sum(r => r.GrossLiters - r.NetLiters),
                    ReceiveTransactionCount = g.Count()
                })
                .ToListAsync();

            // Get chillar name for reference
            var chillar = await _dbContext.Chillars.FirstOrDefaultAsync(c => c.ChillarId == chillarId);

            // Combine data
            var dodhiSummaries = new List<DodhiSummaryDto>();
            var allDodhiIds = new HashSet<int>(purchaseData.Select(p => p.DodhiId)
                .Union(receiveData.Select(r => r.DodhiId)));

            foreach (var dodhiId in allDodhiIds)
            {
                var purchase = purchaseData.FirstOrDefault(p => p.DodhiId == dodhiId);
                var receive = receiveData.FirstOrDefault(r => r.DodhiId == dodhiId);

                var totalPurchased = purchase?.TotalPurchasedLiters ?? 0m;
                var totalReceived = receive?.TotalReceivedLiters ?? 0m;
                var totalNetLiters = receive?.TotalNetLiters ?? 0m;
                var receptionLoss = receive?.ReceptionLoss ?? 0m;
                var purchaseAmount = purchase?.TotalPurchaseAmount ?? 0m;

                var receptionLossPercentage = totalReceived > 0
                    ? (receptionLoss / totalReceived) * 100
                    : 0m;

                var difference = totalPurchased - totalReceived;
                var differencePercentage = totalPurchased > 0
                    ? (difference / totalPurchased) * 100
                    : 0m;

                var dodhiName = purchase?.DodhiName ?? receive?.DodhiName ?? "Unknown";

                dodhiSummaries.Add(new DodhiSummaryDto
                {
                    DodhiId = dodhiId,
                    DodhiName = dodhiName,
                    ChillarId = chillarId,
                    ChillarName = chillar?.Name ?? "Unknown",
                    TotalPurchasedLiters = totalPurchased,
                    TotalPurchaseAmount = purchaseAmount,
                    AveragePurchaseRate = totalPurchased > 0 ? purchaseAmount / totalPurchased : 0m,
                    PurchaseTransactionCount = purchase?.PurchaseTransactionCount ?? 0,
                    TotalReceivedLiters = totalReceived,
                    TotalNetLiters = totalNetLiters,
                    ReceptionLoss = receptionLoss,
                    ReceptionLossPercentage = ReportCalculationHelper.CalculateLossPercentage(totalReceived, totalNetLiters),
                    ReceiveTransactionCount = receive?.ReceiveTransactionCount ?? 0,
                    PurchaseReceiveDifference = difference,
                    PurchaseReceiveDifferencePercentage = differencePercentage
                });
            }

            // Sort by total purchased (descending)
            return dodhiSummaries.OrderByDescending(d => d.TotalPurchasedLiters).ToList();
        }

        /// <summary>
        /// Get dodhi-wise purchase and receive comparison report (combined response)
        /// </summary>
        public async Task<DodhiWisePurchaseReceiveReportDto> GetDodhiWisePurchaseReceiveReportAsync(
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId)
        {
            var details = await GetSingleDodhiSummaryAsync(chillarId, startDate, endDate, tenantId);
            var summary = await GetOverallDodhiSummaryAsync(chillarId, startDate, endDate, tenantId);

            return new DodhiWisePurchaseReceiveReportDto
            {
                DodhiSummaries = details,
                OverallSummary = summary
            };
        }

        /// <summary>
        /// Get detailed transaction-wise data for a specific dodhi (date-wise purchases and receives)
        /// </summary>
        public async Task<DodhiPurchaseReceiveDetailDto> GetDodhiPurchaseReceiveDetailAsync(
            int dodhiId,
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId)
        {
            // Convert dates to DateOnly for filtering
            var dateStart = DateOnly.FromDateTime(startDate.Date);
            var dateEnd = DateOnly.FromDateTime(endDate.Date);

            // Get dodhi info
            var dodhi = await _dbContext.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == dodhiId && e.TenantId == tenantId && e.ChillarId == chillarId);

            if (dodhi == null)
            {
                throw new Exception("Dodhi not found");
            }

            // Get chillar info
            var chillar = await _dbContext.Chillars.FirstOrDefaultAsync(c => c.ChillarId == chillarId);

            // Get purchase transactions for this dodhi
            var purchaseTransactions = await _dbContext.Purchases
                .Include(p => p.Account)
                .Where(p => p.TenantId == tenantId &&
                            p.DodhiId == dodhiId &&
                            p.Date >= dateStart &&
                            p.Date <= dateEnd)
                .OrderBy(p => p.Date)
                .ThenBy(p => p.TimeOfDay)
                .Select(p => new DodhiPurchaseTransactionDto
                {
                    PurchaseId = p.PurchaseId,
                    Date = p.Date.ToDateTime(TimeOnly.MinValue),
                    TimeOfDay = p.TimeOfDay,
                    SupplierCode = p.Account.AccountCode,
                    SupplierName = p.Account.Name,
                    GrossLiters = p.GrossLiters,
                    Rate = p.Rate,
                    TotalAmount = p.GrossLiters * p.Rate
                })
                .ToListAsync();

            // Get receive transactions for this dodhi
            var receiveTransactions = await _dbContext.ChillarReceives
                .Where(r => r.TenantId == tenantId &&
                            r.DodhiId == dodhiId &&
                            r.ChillarId == chillarId &&
                            r.Date >= dateStart &&
                            r.Date <= dateEnd)
                .OrderBy(r => r.Date)
                .ThenBy(r => r.TimeOfDay)
                .Select(r => new DodhiReceiveTransactionDto
                {
                    ReceiveId = r.ReceiveId,
                    Date = r.Date.ToDateTime(TimeOnly.MinValue),
                    TimeOfDay = r.TimeOfDay,
                    GrossLiters = r.GrossLiters,
                    LR = r.LR,
                    Fat = r.Fat,
                    NetLiters = r.NetLiters
                })
                .ToListAsync();

            // Calculate summary
            var totalPurchased = purchaseTransactions.Sum(p => p.GrossLiters);
            var totalPurchaseAmount = purchaseTransactions.Sum(p => p.TotalAmount);
            var totalReceived = receiveTransactions.Sum(r => r.GrossLiters);
            var totalNetLiters = receiveTransactions.Sum(r => r.NetLiters);
            var totalReceptionLoss = totalReceived - totalNetLiters;

            var summary = new DodhiDetailSummaryDto
            {
                TotalPurchasedLiters = totalPurchased,
                TotalPurchaseAmount = totalPurchaseAmount,
                AveragePurchaseRate = totalPurchased > 0 ? totalPurchaseAmount / totalPurchased : 0m,
                PurchaseTransactionCount = purchaseTransactions.Count,
                TotalReceivedLiters = totalReceived,
                TotalNetLiters = totalNetLiters,
                TotalReceptionLoss = totalReceptionLoss,
                ReceptionLossPercentage = ReportCalculationHelper.CalculateLossPercentage(totalReceived, totalNetLiters),
                ReceiveTransactionCount = receiveTransactions.Count,
                PurchaseReceiveDifference = totalPurchased - totalReceived,
                PurchaseReceiveDifferencePercentage = totalPurchased > 0 ? ((totalPurchased - totalReceived) / totalPurchased) * 100 : 0m
            };

            return new DodhiPurchaseReceiveDetailDto
            {
                DodhiId = dodhiId,
                DodhiName = dodhi.FullName,
                ChillarId = chillarId,
                ChillarName = chillar?.Name ?? "Unknown",
                PurchaseTransactions = purchaseTransactions,
                ReceiveTransactions = receiveTransactions,
                Summary = summary
            };
        }

        /// <summary>
        /// Get dodhi performance metrics (detailed comparison)
        /// </summary>
        public async Task<List<DodhiPerformanceMetricDto>> GetDodhiPerformanceMetricsAsync(
            int chillarId,
            DateTime startDate,
            DateTime endDate,
            int tenantId)
        {
            var report = await GetDodhiWisePurchaseReceiveReportAsync(chillarId, startDate, endDate, tenantId);

            return report.DodhiSummaries.Select(d => new DodhiPerformanceMetricDto
            {
                DodhiId = d.DodhiId,
                DodhiName = d.DodhiName,
                TotalPurchasedLiters = d.TotalPurchasedLiters,
                TotalReceivedLiters = d.TotalReceivedLiters,
                TotalNetLiters = d.TotalNetLiters,
                ReceptionLossPercentage = d.ReceptionLossPercentage,
                AveragePurchaseRate = d.AveragePurchaseRate,
                Performance = CalculatePerformanceRating(d)
            }).OrderByDescending(d => d.TotalPurchasedLiters).ToList();
        }

        /// <summary>
        /// Calculate performance rating for a dodhi
        /// </summary>
        private string CalculatePerformanceRating(DodhiSummaryDto dodhi)
        {
            // Rating based on reception loss and purchase receive difference
            if (dodhi.ReceptionLossPercentage < 2 && Math.Abs(dodhi.PurchaseReceiveDifferencePercentage) < 5)
                return "Excellent";
            else if (dodhi.ReceptionLossPercentage < 5 && Math.Abs(dodhi.PurchaseReceiveDifferencePercentage) < 10)
                return "Good";
            else if (dodhi.ReceptionLossPercentage < 10 && Math.Abs(dodhi.PurchaseReceiveDifferencePercentage) < 15)
                return "Average";
            else
                return "Poor";
        }
    }

    /// <summary>
    /// Dodhi performance metric DTO for quick overview
    /// </summary>
    public class DodhiPerformanceMetricDto
    {
        public int DodhiId { get; set; }
        public string DodhiName { get; set; } = string.Empty;
        public decimal TotalPurchasedLiters { get; set; }
        public decimal TotalReceivedLiters { get; set; }
        public decimal TotalNetLiters { get; set; }
        public decimal ReceptionLossPercentage { get; set; }
        public decimal AveragePurchaseRate { get; set; }
        public string Performance { get; set; } = string.Empty;
    }
}

