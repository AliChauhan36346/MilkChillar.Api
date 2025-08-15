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

        public async Task<PaginatedResult<PurchaseDto>> GetPaginatedAsync(PurchaseQueryParameters parameters, int tenantId)
        {


            var query = _context.Purchases
                .Include(p => p.Account)
                .Include(p => p.ExpenseAccount)
                .Include(p => p.Dodhi)
                .Where(p => p.TenantId == tenantId);

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

        public async Task<int?> GetMyDodhiIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var employee = user?.Employee;

            if (employee == null || employee.Designation.ToLower() != "dodhi")
                return null;

            return employee.EmployeeId;
        }

        public async Task<PurchaseMetadataDto> GetPurchaseMetadataAsync(DateOnly date, string timeOfDay, int tenantId, int userId)
        {
            var dodhiId = await GetMyDodhiIdAsync(userId);
            if (dodhiId == null)
                return new PurchaseMetadataDto();

            // Get suppliers assigned to this dodhi
            var allSuppliers = await _context.Suppliers
                .Include(s => s.Account)
                .Where(s => s.TenantId == tenantId &&
                           s.Account.AccountCode.StartsWith("200") &&
                           s.DodhiId == dodhiId &&
                           s.IsActive) // Add IsActive filter
                .ToListAsync();

            // Get expense accounts by account code starting with "500"
            var expenseAccounts = await _context.Accounts
                .Where(a => a.TenantId == tenantId && a.AccountCode.StartsWith("500"))
                .Select(a => new ExpenseAccountDto
                {
                    AccountId = a.AccountId,
                    AccountCode = a.AccountCode,
                    AccountName = a.Name
                }).ToListAsync();

            // Get added purchases - handle "both" case and filter by dodhi
            var addedQuery = _context.Purchases
                .Where(p => p.TenantId == tenantId &&
                           p.Date == date &&
                           p.DodhiId == dodhiId);

            if (timeOfDay != "both")
            {
                addedQuery = addedQuery.Where(p => p.TimeOfDay == timeOfDay);
            }

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

            // Handle remaining suppliers logic for "both" vs specific time
            List<RemainingSupplierDto> remainingSuppliers;

            if (timeOfDay == "both")
            {
                // For "both", show double entries (morning & evening) for suppliers not added for either time
                var addedAccountTimeMap = added
                    .GroupBy(p => p.AccountId)
                    .ToDictionary(g => g.Key, g => g.Select(p => p.TimeOfDay).ToHashSet());

                remainingSuppliers = new List<RemainingSupplierDto>();

                foreach (var supplier in allSuppliers)
                {
                    var addedTimes = addedAccountTimeMap.GetValueOrDefault(supplier.AccountId, new HashSet<string>());

                    // Add morning entry if not added for morning
                    if (!addedTimes.Contains("morning"))
                    {
                        remainingSuppliers.Add(new RemainingSupplierDto
                        {
                            AccountId = supplier.AccountId,
                            AccountName = supplier.Account.Name,
                            AccountCode = supplier.Account.AccountCode,
                            Rate = supplier.Rate,
                            TimeOfDay = "morning" // Add this field to track which time
                        });
                    }

                    // Add evening entry if not added for evening
                    if (!addedTimes.Contains("evening"))
                    {
                        remainingSuppliers.Add(new RemainingSupplierDto
                        {
                            AccountId = supplier.AccountId,
                            AccountName = supplier.Account.Name,
                            AccountCode = supplier.Account.AccountCode,
                            Rate = supplier.Rate,
                            TimeOfDay = "evening" // Add this field to track which time
                        });
                    }
                }
            }
            else
            {
                // For specific time, show suppliers not added for that specific time
                var addedAccountIds = added.Select(p => p.AccountId).ToHashSet();

                remainingSuppliers = allSuppliers
                    .Where(s => !addedAccountIds.Contains(s.AccountId))
                    .Select(s => new RemainingSupplierDto
                    {
                        AccountId = s.AccountId,
                        AccountName = s.Account.Name,
                        AccountCode = s.Account.AccountCode,
                        Rate = s.Rate,
                        TimeOfDay = timeOfDay // Add this field
                    }).ToList();
            }

            return new PurchaseMetadataDto
            {
                DodhiId = dodhiId.Value,
                AddedPurchases = addedDtos,
                RemainingSuppliers = remainingSuppliers,
                ExpenseAccounts = expenseAccounts
            };
        }


    }
}