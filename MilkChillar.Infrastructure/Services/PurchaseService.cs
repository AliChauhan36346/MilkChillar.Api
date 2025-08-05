using MilkChillar.Application.DTOs.Purchase;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PurchaseService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaginatedResult<PurchaseDto>> GetPaginatedAsync(PurchaseQueryParameters parameters)
        {
            var query = _context.Purchases
                .Include(p => p.Account)
                .Include(p => p.ExpenseAccount)
                .Include(p => p.Dodhi)
                .Where(p => p.TenantId == parameters.TenantId);

            // Apply filters
            if (parameters.FromDate.HasValue)
                query = query.Where(p => p.Date >= parameters.FromDate.Value);

            if (parameters.ToDate.HasValue)
                query = query.Where(p => p.Date <= parameters.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(parameters.TimeOfDay))
                query = query.Where(p => p.TimeOfDay == parameters.TimeOfDay);

            if (parameters.AccountId.HasValue)
                query = query.Where(p => p.AccountId == parameters.AccountId.Value);

            if (parameters.DodhiId.HasValue)
                query = query.Where(p => p.DodhiId == parameters.DodhiId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.Date)
                .ThenByDescending(p => p.PurchaseId)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
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

            return new PaginatedResult<PurchaseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<PurchaseDto?> GetByIdAsync(int purchaseId, int tenantId)
        {
            var p = await _context.Purchases
                .Include(p => p.Account)
                .Include(p => p.ExpenseAccount)
                .Include(p => p.Dodhi)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.TenantId == tenantId);

            if (p == null) return null;

            return new PurchaseDto
            {
                PurchaseId = p.PurchaseId,
                Date = p.Date,
                TimeOfDay = p.TimeOfDay,
                AccountId = p.AccountId,
                AccountCode = p.Account.AccountCode,
                AccountName = p.Account.Name,
                ExpenseAccountName = p.ExpenseAccount.Name,
                DodhiName = p.Dodhi.FullName,
                GrossLiters = p.GrossLiters,
                Rate = p.Rate,
                TotalAmount = p.GrossLiters * p.Rate,
                Balance = p.Balance
            };
        }

        public async Task<PurchaseDto> CreateAsync(CreatePurchaseDto dto, int tenantId)
        {
            var purchase = new Purchase
            {
                TenantId = tenantId,
                Date = dto.Date,
                TimeOfDay = dto.TimeOfDay,
                AccountId = dto.AccountId,
                ExpenseAccountId = dto.ExpenseAccountId,
                DodhiId = dto.DodhiId,
                GrossLiters = dto.GrossLiters,
                Rate = dto.Rate,
                Balance = dto.Balance
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            var account = await _context.Accounts.FindAsync(dto.AccountId);
            var expenseAccount = await _context.Accounts.FindAsync(dto.ExpenseAccountId);
            var dodhi = await _context.Employees.FindAsync(dto.DodhiId);

            return new PurchaseDto
            {
                PurchaseId = purchase.PurchaseId,
                Date = purchase.Date,
                TimeOfDay = purchase.TimeOfDay,
                AccountId = purchase.AccountId,
                AccountCode = account?.AccountCode ?? "Unknown",
                AccountName = account?.Name ?? "Unknown",
                ExpenseAccountName = expenseAccount?.Name ?? "Unknown",
                DodhiName = dodhi?.FullName ?? "Unknown",
                GrossLiters = purchase.GrossLiters,
                Rate = purchase.Rate,
                TotalAmount = purchase.GrossLiters * purchase.Rate,
                Balance = purchase.Balance
            };
        }

        public async Task<PurchaseDto?> UpdateAsync(int purchaseId, UpdatePurchaseDto dto, int tenantId)
        {
            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.TenantId == tenantId);

            if (purchase == null) return null;

            purchase.Date = dto.Date;
            purchase.TimeOfDay = dto.TimeOfDay;
            purchase.AccountId = dto.AccountId;
            purchase.ExpenseAccountId = dto.ExpenseAccountId;
            purchase.DodhiId = dto.DodhiId;
            purchase.GrossLiters = dto.GrossLiters;
            purchase.Rate = dto.Rate;
            purchase.Balance = dto.Balance;

            await _context.SaveChangesAsync();

            // Refetch purchase with related data after save to avoid null references
            purchase = await _context.Purchases
                .Include(p => p.Account)
                .Include(p => p.ExpenseAccount)
                .Include(p => p.Dodhi)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.TenantId == tenantId);

            if (purchase == null) return null;

            return new PurchaseDto
            {
                PurchaseId = purchase.PurchaseId,
                Date = purchase.Date,
                TimeOfDay = purchase.TimeOfDay,
                AccountId = purchase.AccountId,
                AccountCode = purchase.Account.AccountCode,
                AccountName = purchase.Account.Name,
                ExpenseAccountName = purchase.ExpenseAccount.Name,
                DodhiName = purchase.Dodhi.FullName,
                GrossLiters = purchase.GrossLiters,
                Rate = purchase.Rate,
                TotalAmount = purchase.GrossLiters * purchase.Rate,
                Balance = purchase.Balance
            };
        }

        public async Task<PurchaseMetadataDto> GetPurchaseMetadataAsync(DateOnly date, string timeOfDay, int tenantId)
        {
            // Get all suppliers with their accounts
            var allSuppliers = await _context.Suppliers
                .Include(s => s.Account)
                .Where(s => s.TenantId == tenantId && s.Account.SubAccount.MainAccount.Name.ToLower() == "suppliers")
                .ToListAsync();

            // Get expense accounts
            var expenseAccounts = await (
                from a in _context.Accounts
                join sa in _context.SubAccounts on a.SubAccountId equals sa.SubAccountId
                join ma in _context.MainAccounts on sa.MainAccountId equals ma.MainAccountId
                where a.TenantId == tenantId && ma.FinancialStatementComponent.ToLower() == "expense"
                select new ExpenseAccountDto
                {
                    AccountId = a.AccountId,
                    AccountCode = a.AccountCode,
                    AccountName = a.Name
                }).ToListAsync();

            // Get all dodhis (employees with Dodhi designation)
            var dodhis = await _context.Employees
                .Where(e => e.TenantId == tenantId && e.Designation.ToLower() == "dodhi")
                .Select(e => new DodhiDto
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FullName
                }).ToListAsync();

            // Get added purchases for this date and time
            var addedQuery = _context.Purchases
                .Where(p => p.TenantId == tenantId && p.Date == date && p.TimeOfDay == timeOfDay);

            var added = await addedQuery
                .Include(p => p.Account)
                .Include(p => p.ExpenseAccount)
                .Include(p => p.Dodhi)
                .ToListAsync();

            var addedDtos = added.Select(p => new PurchaseDto
            {
                PurchaseId = p.PurchaseId,
                Date = p.Date,
                TimeOfDay = p.TimeOfDay,
                AccountId = p.AccountId,
                AccountCode = p.Account.AccountCode,
                AccountName = p.Account.Name,
                ExpenseAccountName = p.ExpenseAccount.Name,
                DodhiName = p.Dodhi.FullName,
                GrossLiters = p.GrossLiters,
                Rate = p.Rate,
                TotalAmount = p.GrossLiters * p.Rate,
                Balance = p.Balance
            }).ToList();

            var addedAccountIds = added.Select(p => p.AccountId).ToHashSet();

            var remainingSuppliers = allSuppliers
                .Where(s => !addedAccountIds.Contains(s.AccountId))
                .Select(s => new RemainingSupplierDto
                {
                    AccountId = s.AccountId,
                    AccountName = s.Account.Name,
                    AccountCode = s.Account.AccountCode,
                    Rate = s.Rate,
                    DodhiId = s.DodhiId
                }).ToList();

            return new PurchaseMetadataDto
            {
                AddedPurchases = addedDtos,
                RemainingSuppliers = remainingSuppliers,
                ExpenseAccounts = expenseAccounts,
                Dodhis = dodhis
            };
        }
    }
}