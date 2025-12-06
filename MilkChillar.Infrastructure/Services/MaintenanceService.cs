using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MilkChillar.Application.Interfaces;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;
using MilkChillar.Application.DTOs.Maintenance;

namespace MilkChillar.Infrastructure.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> MassUpdateSupplierRatesByDodhi(int dodhiId, decimal newRate, int tenantId)
        {
            var suppliers = await _context.Suppliers
                .Where(s => s.DodhiId == dodhiId && s.TenantId == tenantId)
                .ToListAsync();
            foreach (var supplier in suppliers)
                supplier.Rate = newRate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RateSummaryDto> GetSupplierRateSummaryForPeriod(int accountId, DateOnly startDate, DateOnly endDate, int tenantId)
        {
            var purchases = await _context.Purchases
                .Where(p => p.AccountId == accountId && p.TenantId == tenantId && p.Date >= startDate && p.Date <= endDate)
                .ToListAsync();

            //var previousRate = purchases.FirstOrDefault()?.Rate ?? 0;
            var totalLiters = purchases.Sum(p => p.GrossLiters);
            var totalAmount = purchases.Sum(p => p.TotalAmount);

            double previousRate;

            if (totalLiters > 0)
            {
                previousRate = (double)(totalAmount / totalLiters);   // Safe division
            }
            else
            {
                previousRate = (double)(purchases.FirstOrDefault()?.Rate ?? 0);
            }

            return new RateSummaryDto
            {
                PreviousRate = (decimal)previousRate,
                TotalLiters = totalLiters,
                TotalAmount = totalAmount
            };
        }

        public async Task<RateSummaryDto> GetBuyerRateSummaryForPeriod(int accountId, DateOnly startDate, DateOnly endDate, int tenantId)
        {
            var sales = await _context.Sales
                .Where(s => s.AccountId == accountId && s.TenantId == tenantId && s.Date >= startDate && s.Date <= endDate)
                .ToListAsync();

            //var previousRate = sales.FirstOrDefault()?.Rate ?? 0;
            var totalLiters = sales.Sum(s => s.NetLiters);
            var totalAmount = sales.Sum(s => s.TotalAmount);

            double previousRate;

            if (totalLiters > 0)
            {
                previousRate = (double)(totalAmount / totalLiters);   // Safe division
            }
            else
            {
                previousRate = (double)(sales.FirstOrDefault()?.Rate ?? 0);
            }

            return new RateSummaryDto
            {
                PreviousRate = (decimal)previousRate,
                TotalLiters = totalLiters,
                TotalAmount = totalAmount
            };
        }

        public async Task<bool> UpdateSupplierRateForPeriod(int accountId, decimal newRate, DateOnly startDate, DateOnly endDate, int tenantId)
        {
            var purchases = await _context.Purchases
                .Where(p => p.AccountId == accountId && p.TenantId == tenantId && p.Date >= startDate && p.Date <= endDate)
                .ToListAsync();
            foreach (var purchase in purchases)
                purchase.Rate = newRate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBuyerRateForPeriod(int accountId, decimal newRate, DateOnly startDate, DateOnly endDate, int tenantId)
        {
            var sales = await _context.Sales
                .Where(s => s.AccountId == accountId && s.TenantId == tenantId && s.Date >= startDate && s.Date <= endDate)
                .ToListAsync();
            foreach (var sale in sales)
                sale.Rate = newRate;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> MassUpdateSupplierRates(decimal newRate, int tenantId)
        {
            var suppliers = await _context.Suppliers.Where(s => s.TenantId == tenantId).ToListAsync();
            foreach (var supplier in suppliers)
                supplier.Rate = newRate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MassUpdateBuyerRates(decimal newRate, int tenantId)
        {
            var buyers = await _context.Buyers.Where(b => b.TenantId == tenantId).ToListAsync();
            foreach (var buyer in buyers)
                buyer.Rate = newRate;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> MassUpdateSupplierDodhi(int dodhiId, int[] supplierIds, int tenantId)
        {
            var suppliers = await _context.Suppliers
                .Where(s => supplierIds.Contains(s.SupplierId) && s.TenantId == tenantId)
                .ToListAsync();
            foreach (var supplier in suppliers)
                supplier.DodhiId = dodhiId;
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
