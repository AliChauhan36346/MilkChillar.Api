//    using MilkChillar.Application.DTOs.Purchase;
//using MilkChillar.Application.Interfaces;
//using MilkChillar.Application.Parameters;
//using MilkChillar.Application.Responses;
//using MilkChillar.Domain.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Http;
//using MilkChillar.Application;

//namespace MilkChillar.Infrastructure.Services
//{
//    public class PurchaseService : IPurchaseService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IHttpContextAccessor _httpContextAccessor;

//        public PurchaseService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
//        {
//            _context = context;
//            _httpContextAccessor = httpContextAccessor;
//        }

//        public async Task<PaginatedResult<PurchaseDto>> GetPaginatedAsync(PurchaseQueryParameters parameters, int tenantId)
//        {


//            var query = _context.Purchases
//                .Include(p => p.Account)
//                .Include(p => p.ExpenseAccount)
//                .Include(p => p.Dodhi)
//                .Where(p => p.TenantId == tenantId);

//            // Apply filters
//            if (parameters.FromDate.HasValue)
//                query = query.Where(p => p.Date >= parameters.FromDate.Value);

//            if (parameters.ToDate.HasValue)
//                query = query.Where(p => p.Date <= parameters.ToDate.Value);

//            if (!string.IsNullOrWhiteSpace(parameters.TimeOfDay))
//                query = query.Where(p => p.TimeOfDay == parameters.TimeOfDay);

//            if (parameters.AccountId.HasValue)
//                query = query.Where(p => p.AccountId == parameters.AccountId.Value);

//            if (parameters.DodhiId.HasValue)
//                query = query.Where(p => p.DodhiId == parameters.DodhiId.Value);

//            var totalCount = await query.CountAsync();

//            var items = await query
//                .OrderByDescending(p => p.Date)
//                .ThenByDescending(p => p.PurchaseId)
//                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
//                .Take(parameters.PageSize)
//                .Select(p => new PurchaseDto
//                {
//                    PurchaseId = p.PurchaseId,
//                    Date = p.Date,
//                    TimeOfDay = p.TimeOfDay,
//                    AccountId = p.AccountId,
//                    AccountName = p.Account.Name,
//                    AccountCode = p.Account.AccountCode,
//                    ExpenseAccountName = p.ExpenseAccount.Name,
//                    DodhiName = p.Dodhi.FullName,
//                    GrossLiters = p.GrossLiters,
//                    Rate = p.Rate,
//                    TotalAmount = p.GrossLiters * p.Rate,
//                    Balance = p.Balance
//                })
//                .ToListAsync();

//            return new PaginatedResult<PurchaseDto>
//            {
//                Items = items,
//                TotalCount = totalCount,
//                PageNumber = parameters.PageNumber,
//                PageSize = parameters.PageSize
//            };
//        }

//        public async Task<PurchaseDto?> GetByIdAsync(int purchaseId, int tenantId)
//        {
//            var p = await _context.Purchases
//                .Include(p => p.Account)
//                .Include(p => p.ExpenseAccount)
//                .Include(p => p.Dodhi)
//                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.TenantId == tenantId);

//            if (p == null) return null;

//            return new PurchaseDto
//            {
//                PurchaseId = p.PurchaseId,
//                Date = p.Date,
//                TimeOfDay = p.TimeOfDay,
//                AccountId = p.AccountId,
//                AccountCode = p.Account.AccountCode,
//                AccountName = p.Account.Name,
//                ExpenseAccountName = p.ExpenseAccount.Name,
//                DodhiName = p.Dodhi.FullName,
//                GrossLiters = p.GrossLiters,
//                Rate = p.Rate,
//                TotalAmount = p.GrossLiters * p.Rate,
//                Balance = p.Balance
//            };
//        }

//        public async Task<PurchaseDto> CreateAsync(CreatePurchaseDto dto, int tenantId)
//        {
//            var purchase = new Purchase
//            {
//                TenantId = tenantId,
//                Date = dto.Date,
//                TimeOfDay = dto.TimeOfDay,
//                AccountId = dto.AccountId,
//                ExpenseAccountId = dto.ExpenseAccountId,
//                DodhiId = dto.DodhiId,
//                GrossLiters = dto.GrossLiters,
//                Rate = dto.Rate,
//                Balance = dto.Balance
//            };

//            _context.Purchases.Add(purchase);
//            await _context.SaveChangesAsync();

//            var account = await _context.Accounts.FindAsync(dto.AccountId);
//            var expenseAccount = await _context.Accounts.FindAsync(dto.ExpenseAccountId);
//            var dodhi = await _context.Employees.FindAsync(dto.DodhiId);

//            return new PurchaseDto
//            {
//                PurchaseId = purchase.PurchaseId,
//                Date = purchase.Date,
//                TimeOfDay = purchase.TimeOfDay,
//                AccountId = purchase.AccountId,
//                AccountCode = account?.AccountCode ?? "Unknown",
//                AccountName = account?.Name ?? "Unknown",
//                ExpenseAccountName = expenseAccount?.Name ?? "Unknown",
//                DodhiName = dodhi?.FullName ?? "Unknown",
//                GrossLiters = purchase.GrossLiters,
//                Rate = purchase.Rate,
//                TotalAmount = purchase.GrossLiters * purchase.Rate,
//                Balance = purchase.Balance
//            };
//        }

//        public async Task<PurchaseDto?> UpdateAsync(int purchaseId, UpdatePurchaseDto dto, int tenantId)
//        {
//            var purchase = await _context.Purchases
//                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.TenantId == tenantId);

//            if (purchase == null) return null;

//            purchase.Date = dto.Date;
//            purchase.TimeOfDay = dto.TimeOfDay;
//            purchase.AccountId = dto.AccountId;
//            purchase.ExpenseAccountId = dto.ExpenseAccountId;
//            purchase.DodhiId = dto.DodhiId;
//            purchase.GrossLiters = dto.GrossLiters;
//            purchase.Rate = dto.Rate;
//            purchase.Balance = dto.Balance;

//            await _context.SaveChangesAsync();

//            // Refetch purchase with related data after save to avoid null references
//            purchase = await _context.Purchases
//                .Include(p => p.Account)
//                .Include(p => p.ExpenseAccount)
//                .Include(p => p.Dodhi)
//                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.TenantId == tenantId);

//            if (purchase == null) return null;

//            return new PurchaseDto
//            {
//                PurchaseId = purchase.PurchaseId,
//                Date = purchase.Date,
//                TimeOfDay = purchase.TimeOfDay,
//                AccountId = purchase.AccountId,
//                AccountCode = purchase.Account.AccountCode,
//                AccountName = purchase.Account.Name,
//                ExpenseAccountName = purchase.ExpenseAccount.Name,
//                DodhiName = purchase.Dodhi.FullName,
//                GrossLiters = purchase.GrossLiters,
//                Rate = purchase.Rate,
//                TotalAmount = purchase.GrossLiters * purchase.Rate,
//                Balance = purchase.Balance
//            };
//        }

//        public async Task<int?> GetMyDodhiIdAsync(int userId)
//        {
//            var user = await _context.Users
//                .Include(u => u.Employee)
//                .FirstOrDefaultAsync(u => u.UserId == userId);

//            var employee = user?.Employee;

//            if (employee == null || employee.Designation.ToLower() != "dodhi")
//                return null;

//            return employee.EmployeeId;
//        }




//        //public async Task<PurchaseMetadataDto> GetPurchaseMetadataAsync(DateOnly date, string timeOfDay, int tenantId, int dodhiId)
//        //{
//        //    // No need to get dodhiId from userId anymore - it's passed as parameter
//        //    if (dodhiId <= 0)
//        //        return new PurchaseMetadataDto();

//        //    // Get suppliers assigned to this dodhi
//        //    var allSuppliers = await _context.Suppliers
//        //        .Include(s => s.Account)
//        //        .Where(s => s.TenantId == tenantId &&
//        //                   s.Account.AccountCode.StartsWith("200") &&
//        //                   s.DodhiId == dodhiId &&
//        //                   s.IsActive) // Add IsActive filter
//        //        .ToListAsync();

//        //    // Get expense accounts by account code starting with "500"
//        //    var expenseAccounts = await _context.Accounts
//        //        .Where(a => a.TenantId == tenantId && a.AccountCode.StartsWith("500"))
//        //        .Select(a => new ExpenseAccountDto
//        //        {
//        //            AccountId = a.AccountId,
//        //            AccountCode = a.AccountCode,
//        //            AccountName = a.Name
//        //        }).ToListAsync();

//        //    // Get added purchases - handle "both" case and filter by dodhi
//        //    var addedQuery = _context.Purchases
//        //        .Where(p => p.TenantId == tenantId &&
//        //                   p.Date == date &&
//        //                   p.DodhiId == dodhiId);

//        //    if (timeOfDay != "both")
//        //    {
//        //        addedQuery = addedQuery.Where(p => p.TimeOfDay == timeOfDay);
//        //    }

//        //    var added = await addedQuery
//        //        .Include(p => p.Account)
//        //        .Include(p => p.ExpenseAccount)
//        //        .Include(p => p.Dodhi)
//        //        .ToListAsync();

//        //    var addedDtos = added.Select(p => new PurchaseDto
//        //    {
//        //        PurchaseId = p.PurchaseId,
//        //        Date = p.Date,
//        //        TimeOfDay = p.TimeOfDay,
//        //        AccountId = p.AccountId,
//        //        AccountCode = p.Account.AccountCode,
//        //        AccountName = p.Account.Name,
//        //        ExpenseAccountName = p.ExpenseAccount.Name,
//        //        DodhiName = p.Dodhi.FullName,
//        //        GrossLiters = p.GrossLiters,
//        //        Rate = p.Rate,
//        //        TotalAmount = p.GrossLiters * p.Rate,
//        //        Balance = p.Balance
//        //    }).ToList();

//        //    // Handle remaining suppliers logic for "both" vs specific time
//        //    List<RemainingSupplierDto> remainingSuppliers;

//        //    if (timeOfDay == "both")
//        //    {
//        //        // For "both", show double entries (morning & evening) for suppliers not added for either time
//        //        var addedAccountTimeMap = added
//        //            .GroupBy(p => p.AccountId)
//        //            .ToDictionary(g => g.Key, g => g.Select(p => p.TimeOfDay).ToHashSet());

//        //        remainingSuppliers = new List<RemainingSupplierDto>();

//        //        foreach (var supplier in allSuppliers)
//        //        {
//        //            var addedTimes = addedAccountTimeMap.GetValueOrDefault(supplier.AccountId, new HashSet<string>());

//        //            // Add morning entry if not added for morning
//        //            if (!addedTimes.Contains("morning"))
//        //            {
//        //                remainingSuppliers.Add(new RemainingSupplierDto
//        //                {
//        //                    AccountId = supplier.AccountId,
//        //                    AccountName = supplier.Account.Name,
//        //                    AccountCode = supplier.Account.AccountCode,
//        //                    Rate = supplier.Rate,
//        //                    TimeOfDay = "morning" // Add this field to track which time
//        //                });
//        //            }

//        //            // Add evening entry if not added for evening
//        //            if (!addedTimes.Contains("evening"))
//        //            {
//        //                remainingSuppliers.Add(new RemainingSupplierDto
//        //                {
//        //                    AccountId = supplier.AccountId,
//        //                    AccountName = supplier.Account.Name,
//        //                    AccountCode = supplier.Account.AccountCode,
//        //                    Rate = supplier.Rate,
//        //                    TimeOfDay = "evening" // Add this field to track which time
//        //                });
//        //            }
//        //        }
//        //    }
//        //    else
//        //    {
//        //        // For specific time, show suppliers not added for that specific time
//        //        var addedAccountIds = added.Select(p => p.AccountId).ToHashSet();

//        //        remainingSuppliers = allSuppliers
//        //            .Where(s => !addedAccountIds.Contains(s.AccountId))
//        //            .Select(s => new RemainingSupplierDto
//        //            {
//        //                AccountId = s.AccountId,
//        //                AccountName = s.Account.Name,
//        //                AccountCode = s.Account.AccountCode,
//        //                Rate = s.Rate,
//        //                TimeOfDay = timeOfDay // Add this field
//        //            }).ToList();
//        //    }

//        //    return new PurchaseMetadataDto
//        //    {
//        //        DodhiId = dodhiId,
//        //        AddedPurchases = addedDtos,
//        //        RemainingSuppliers = remainingSuppliers,
//        //        ExpenseAccounts = expenseAccounts
//        //    };
//        //}

//        public async Task<PaginatedResult<RemainingSupplierDto>> GetRemainingSuppliers(
//        DateOnly date,
//        string timeOfDay,
//        int dodhiId,
//        string? searchCode = null,
//        int page = 1,
//        int pageSize = 20)
//        {
//            try
//            {
//                if (dodhiId <= 0)
//                    return new PaginatedResult<RemainingSupplierDto> { Items = new List<RemainingSupplierDto>() };

//                // Validate timeOfDay
//                var validTimes = new[] { "morning", "evening", "both" };
//                if (!validTimes.Contains(timeOfDay?.ToLower()))
//                    timeOfDay = "both";

//                // Get all suppliers assigned to this dodhi
//                var allSuppliers = await _context.Suppliers
//                    .Include(s => s.Account)
//                    .Where(s => s.DodhiId == dodhiId &&
//                               s.Account.AccountCode.StartsWith("200") &&
//                               s.IsActive)
//                    .ToListAsync();

//                if (allSuppliers.Count == 0)
//                    return new PaginatedResult<RemainingSupplierDto> { Items = new List<RemainingSupplierDto>() };

//                // Get added purchases for this date and dodhi
//                var addedPurchases = await _context.Purchases
//                    .Where(p => p.Date == date && p.DodhiId == dodhiId)
//                    .ToListAsync();

//                // Build map of which suppliers have purchases at which times
//                var addedMap = addedPurchases
//                    .GroupBy(p => p.AccountId)
//                    .ToDictionary(g => g.Key, g => g.Select(p => p.TimeOfDay).ToHashSet());

//                // Generate remaining suppliers list
//                var remainingList = new List<RemainingSupplierDto>();

//                foreach (var supplier in allSuppliers)
//                {
//                    var addedTimes = addedMap.GetValueOrDefault(supplier.AccountId, new HashSet<string>());

//                    // Add morning entry if not added for morning
//                    if ((timeOfDay.ToLower() == "morning" || timeOfDay.ToLower() == "both") &&
//                        !addedTimes.Contains("morning"))
//                    {
//                        remainingList.Add(new RemainingSupplierDto
//                        {
//                            AccountId = supplier.AccountId,
//                            AccountName = supplier.Account.Name,
//                            AccountCode = supplier.Account.AccountCode,
//                            Rate = supplier.Rate,
//                            TimeOfDay = "morning"
//                        });
//                    }

//                    // Add evening entry if not added for evening
//                    if ((timeOfDay.ToLower() == "evening" || timeOfDay.ToLower() == "both") &&
//                        !addedTimes.Contains("evening"))
//                    {
//                        remainingList.Add(new RemainingSupplierDto
//                        {
//                            AccountId = supplier.AccountId,
//                            AccountName = supplier.Account.Name,
//                            AccountCode = supplier.Account.AccountCode,
//                            Rate = supplier.Rate,
//                            TimeOfDay = "evening"
//                        });
//                    }
//                }

//                // Apply search filter by code if provided
//                if (!string.IsNullOrWhiteSpace(searchCode))
//                {
//                    searchCode = searchCode.Trim().ToLower();
//                    remainingList = remainingList
//                        .Where(s => s.AccountCode.ToLower().Contains(searchCode) ||
//                                    s.AccountName.ToLower().Contains(searchCode))
//                        .ToList();
//                }

//                // Calculate pagination
//                var totalCount = remainingList.Count;
//                var skip = (page - 1) * pageSize;
//                var paginatedItems = remainingList
//                    .Skip(skip)
//                    .Take(pageSize)
//                    .ToList();

//                return new PaginatedResult<RemainingSupplierDto>
//                {
//                    Items = paginatedItems,
//                    TotalCount = totalCount,
//                    PageNumber = page,
//                    PageSize = pageSize
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting remaining suppliers for dodhi {DodhiId}", dodhiId);
//                return new PaginatedResult<RemainingSupplierDto>
//                {
//                    Items = new List<RemainingSupplierDto>(),
//                    PageNumber = page,
//                    PageSize = pageSize,
//                    TotalCount = 0
//                };
//            }
//        }

//        // ============================================
//        // 2. GET DAILY PURCHASES (Paginated)
//        // ============================================
//        public async Task<PaginatedResult<PurchaseDto>> GetDailyPurchases(
//            DateOnly date,
//            string timeOfDay,
//            int dodhiId,
//            string? searchCode = null,
//            int page = 1,
//            int pageSize = 20)
//        {
//            try
//            {
//                if (dodhiId <= 0)
//                    return new PaginatedResult<PurchaseDto> { Items = new List<PurchaseDto>() };

//                // Validate timeOfDay
//                var validTimes = new[] { "morning", "evening", "both" };
//                if (!validTimes.Contains(timeOfDay?.ToLower()))
//                    timeOfDay = "both";

//                // Build query for purchases
//                var query = _context.Purchases
//                    .Include(p => p.Account)
//                    .Include(p => p.ExpenseAccount)
//                    .Include(p => p.Dodhi)
//                    .Where(p => p.Date == date && p.DodhiId == dodhiId);

//                // Filter by timeOfDay if not "both"
//                if (timeOfDay.ToLower() != "both")
//                {
//                    query = query.Where(p => p.TimeOfDay == timeOfDay.ToLower());
//                }

//                // Apply search filter by code if provided
//                if (!string.IsNullOrWhiteSpace(searchCode))
//                {
//                    searchCode = searchCode.Trim().ToLower();
//                    query = query.Where(p => p.Account.AccountCode.ToLower().Contains(searchCode) ||
//                                             p.Account.Name.ToLower().Contains(searchCode));
//                }

//                // Get total count before pagination
//                var totalCount = await query.CountAsync();

//                // Apply pagination
//                var purchases = await query
//                    .OrderByDescending(p => p.PurchaseId) // Latest first
//                    .Skip((page - 1) * pageSize)
//                    .Take(pageSize)
//                    .ToListAsync();

//                // Map to DTOs
//                var purchaseDtos = purchases.Select(p => new PurchaseDto
//                {
//                    PurchaseId = p.PurchaseId,
//                    Date = p.Date,
//                    TimeOfDay = p.TimeOfDay,
//                    AccountId = p.AccountId,
//                    AccountCode = p.Account.AccountCode,
//                    AccountName = p.Account.Name,
//                    ExpenseAccountName = p.ExpenseAccount.Name,
//                    DodhiName = p.Dodhi.FullName,
//                    GrossLiters = p.GrossLiters,
//                    Rate = p.Rate,
//                    TotalAmount = p.GrossLiters * p.Rate,
//                    Balance = p.Balance
//                }).ToList();

//                return new PaginatedResult<PurchaseDto>
//                {
//                    Items = purchaseDtos,
//                    TotalCount = totalCount,
//                    PageNumber = page,
//                    PageSize = pageSize
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting daily purchases for dodhi {DodhiId} on {Date}", dodhiId, date);
//                return new PaginatedResult<PurchaseDto>
//                {
//                    Items = new List<PurchaseDto>(),
//                    PageNumber = page,
//                    PageSize = pageSize,
//                    TotalCount = 0
//                };
//            }
//        }

//        // ============================================
//        // 3. GET PURCHASE SUMMARY (Totals Only)
//        // ============================================
//        public async Task<PurchaseSummaryDto> GetPurchaseSummary(
//            DateOnly date,
//            int dodhiId,
//            string? timeOfDay = null)
//        {
//            try
//            {
//                if (dodhiId <= 0)
//                    return new PurchaseSummaryDto
//                    {
//                        Date = date,
//                        DodhiId = dodhiId,
//                        Morning = new PurchaseSummaryDto.TimeSummary(),
//                        Evening = new PurchaseSummaryDto.TimeSummary()
//                    };

//                // Get all purchases for the date and dodhi
//                var purchases = await _context.Purchases
//                    .Where(p => p.Date == date && p.DodhiId == dodhiId)
//                    .ToListAsync();

//                // Calculate morning summary
//                var morningPurchases = purchases.Where(p => p.TimeOfDay == "morning").ToList();
//                var morningSummary = new PurchaseSummaryDto.TimeSummary
//                {
//                    TotalLiters = morningPurchases.Sum(p => p.GrossLiters),
//                    TotalAmount = morningPurchases.Sum(p => p.GrossLiters * p.Rate),
//                    Count = morningPurchases.Count
//                };

//                // Calculate evening summary
//                var eveningPurchases = purchases.Where(p => p.TimeOfDay == "evening").ToList();
//                var eveningSummary = new PurchaseSummaryDto.TimeSummary
//                {
//                    TotalLiters = eveningPurchases.Sum(p => p.GrossLiters),
//                    TotalAmount = eveningPurchases.Sum(p => p.GrossLiters * p.Rate),
//                    Count = eveningPurchases.Count
//                };

//                return new PurchaseSummaryDto
//                {
//                    Date = date,
//                    DodhiId = dodhiId,
//                    Morning = morningSummary,
//                    Evening = eveningSummary
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting purchase summary for dodhi {DodhiId} on {Date}", dodhiId, date);
//                return new PurchaseSummaryDto
//                {
//                    Date = date,
//                    DodhiId = dodhiId,
//                    Morning = new PurchaseSummaryDto.TimeSummary(),
//                    Evening = new PurchaseSummaryDto.TimeSummary()
//                };
//            }
//        }


//    }
//}

using MilkChillar.Application.DTOs.Purchase;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(ApplicationDbContext context,IHttpContextAccessor httpContextAccessor,ILogger<PurchaseService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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
                ExpenseAccountId= p.ExpenseAccountId,
                ExpenseAccountName = p.ExpenseAccount.Name,
                DodhiId= p.DodhiId,
                DodhiName = p.Dodhi.FullName,
                GrossLiters = p.GrossLiters,
                Rate = p.Rate,
                TotalAmount = p.GrossLiters * p.Rate,
                Balance = p.Balance
            };
        }

        public async Task<PurchaseDto> CreateAsync(CreatePurchaseDto dto, int tenantId)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.AccountId == dto.AccountId && s.TenantId == tenantId);
            if (supplier != null && !supplier.IsActive)
            {
                throw new InvalidOperationException("Cannot add purchase for an inactive supplier.");
            }

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

        
        // 1. GET REMAINING SUPPLIERS (Paginated + Search)
        public async Task<PaginatedResult<RemainingSupplierDto>> GetRemainingSuppliers(
            DateOnly date,
            string timeOfDay,
            int dodhiId,
            string searchCode,
            int page,
            int pageSize)
        {
            try
            {
                if (dodhiId <= 0)
                    return new PaginatedResult<RemainingSupplierDto>
                    {
                        Items = new List<RemainingSupplierDto>(),
                        PageNumber = page,
                        PageSize = pageSize,
                        TotalCount = 0
                    };

                // Validate timeOfDay
                var validTimes = new[] { "morning", "evening", "both" };
                if (!validTimes.Contains(timeOfDay?.ToLower()))
                    timeOfDay = "both";

                // Get all suppliers assigned to this dodhi
                var allSuppliers = await _context.Suppliers
                    .Include(s => s.Account)
                    .Where(s => s.DodhiId == dodhiId &&
                               s.Account.AccountCode.StartsWith("200") &&
                               s.IsActive)
                    .ToListAsync();

                if (allSuppliers.Count == 0)
                    return new PaginatedResult<RemainingSupplierDto>
                    {
                        Items = new List<RemainingSupplierDto>(),
                        PageNumber = page,
                        PageSize = pageSize,
                        TotalCount = 0
                    };

                // Get added purchases for this date and dodhi
                var addedPurchases = await _context.Purchases
                    .Where(p => p.Date == date && p.DodhiId == dodhiId)
                    .ToListAsync();

                // Build map of which suppliers have purchases at which times
                var addedMap = addedPurchases
                    .GroupBy(p => p.AccountId)
                    .ToDictionary(g => g.Key, g => g.Select(p => p.TimeOfDay).ToHashSet());

                // Generate remaining suppliers list
                var remainingList = new List<RemainingSupplierDto>();

                foreach (var supplier in allSuppliers)
                {
                    var addedTimes = addedMap.GetValueOrDefault(supplier.AccountId, new HashSet<string>());

                    // Add morning entry if not added for morning
                    if ((timeOfDay.ToLower() == "morning" || timeOfDay.ToLower() == "both") &&
                        !addedTimes.Contains("morning"))
                    {
                        remainingList.Add(new RemainingSupplierDto
                        {
                            AccountId = supplier.AccountId,
                            AccountName = supplier.Account.Name,
                            AccountCode = supplier.Account.AccountCode,
                            Rate = supplier.Rate,
                            TimeOfDay = "morning"
                        });
                    }

                    // Add evening entry if not added for evening
                    if ((timeOfDay.ToLower() == "evening" || timeOfDay.ToLower() == "both") &&
                        !addedTimes.Contains("evening"))
                    {
                        remainingList.Add(new RemainingSupplierDto
                        {
                            AccountId = supplier.AccountId,
                            AccountName = supplier.Account.Name,
                            AccountCode = supplier.Account.AccountCode,
                            Rate = supplier.Rate,
                            TimeOfDay = "evening"
                        });
                    }
                }

                // Apply search filter by code if provided
                if (!string.IsNullOrWhiteSpace(searchCode))
                {
                    searchCode = searchCode.Trim().ToLower();
                    remainingList = remainingList
                        .Where(s => s.AccountCode.ToLower().Contains(searchCode) ||
                                    s.AccountName.ToLower().Contains(searchCode))
                        .ToList();
                }

                // Calculate pagination
                var totalCount = remainingList.Count;
                var skip = (page - 1) * pageSize;
                var paginatedItems = remainingList
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                return new PaginatedResult<RemainingSupplierDto>
                {
                    Items = paginatedItems,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remaining suppliers for dodhi {DodhiId}", dodhiId);
                return new PaginatedResult<RemainingSupplierDto>
                {
                    Items = new List<RemainingSupplierDto>(),
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }
        }

        
        public async Task<PaginatedResult<PurchaseDto>> GetDailyPurchases(
            DateOnly date,
            string timeOfDay,
            int dodhiId,
            string searchCode,
            int page,
            int pageSize)
        {
            try
            {
                if (dodhiId <= 0)
                    return new PaginatedResult<PurchaseDto>
                    {
                        Items = new List<PurchaseDto>(),
                        PageNumber = page,
                        PageSize = pageSize,
                        TotalCount = 0
                    };

                // Validate timeOfDay
                var validTimes = new[] { "morning", "evening", "both" };
                if (!validTimes.Contains(timeOfDay?.ToLower()))
                    timeOfDay = "both";

                // Build query for purchases
                var query = _context.Purchases
                    .Include(p => p.Account)
                    .Include(p => p.ExpenseAccount)
                    .Include(p => p.Dodhi)
                    .Where(p => p.Date == date && p.DodhiId == dodhiId);

                // Filter by timeOfDay if not "both"
                if (timeOfDay.ToLower() != "both")
                {
                    query = query.Where(p => p.TimeOfDay == timeOfDay.ToLower());
                }

                // Apply search filter by code if provided
                if (!string.IsNullOrWhiteSpace(searchCode))
                {
                    searchCode = searchCode.Trim().ToLower();
                    query = query.Where(p => p.Account.AccountCode.ToLower().Contains(searchCode) ||
                                             p.Account.Name.ToLower().Contains(searchCode));
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var purchases = await query
                    .OrderByDescending(p => p.PurchaseId) // Latest first
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to DTOs
                var purchaseDtos = purchases.Select(p => new PurchaseDto
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

                return new PaginatedResult<PurchaseDto>
                {
                    Items = purchaseDtos,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily purchases for dodhi {DodhiId} on {Date}", dodhiId, date);
                return new PaginatedResult<PurchaseDto>
                {
                    Items = new List<PurchaseDto>(),
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }
        }

        // 3. GET PURCHASE SUMMARY (Totals Only)
        
        public async Task<PurchaseSummaryDto> GetPurchaseSummary(
            DateOnly date,
            int dodhiId,
            string? timeOfDay = null)
        {
            try
            {
                if (dodhiId <= 0)
                    return new PurchaseSummaryDto
                    {
                        Date = date,
                        DodhiId = dodhiId,
                        Morning = new PurchaseSummaryDto.TimeSummary(),
                        Evening = new PurchaseSummaryDto.TimeSummary()
                    };

                // Get all purchases for the date and dodhi
                var purchases = await _context.Purchases
                    .Where(p => p.Date == date && p.DodhiId == dodhiId)
                    .ToListAsync();

                // Calculate morning summary
                var morningPurchases = purchases.Where(p => p.TimeOfDay == "morning").ToList();
                var morningSummary = new PurchaseSummaryDto.TimeSummary
                {
                    TotalLiters = morningPurchases.Sum(p => p.GrossLiters),
                    TotalAmount = morningPurchases.Sum(p => p.GrossLiters * p.Rate),
                    Count = morningPurchases.Count
                };

                // Calculate evening summary
                var eveningPurchases = purchases.Where(p => p.TimeOfDay == "evening").ToList();
                var eveningSummary = new PurchaseSummaryDto.TimeSummary
                {
                    TotalLiters = eveningPurchases.Sum(p => p.GrossLiters),
                    TotalAmount = eveningPurchases.Sum(p => p.GrossLiters * p.Rate),
                    Count = eveningPurchases.Count
                };

                return new PurchaseSummaryDto
                {
                    Date = date,
                    DodhiId = dodhiId,
                    Morning = morningSummary,
                    Evening = eveningSummary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase summary for dodhi {DodhiId} on {Date}", dodhiId, date);
                return new PurchaseSummaryDto
                {
                    Date = date,
                    DodhiId = dodhiId,
                    Morning = new PurchaseSummaryDto.TimeSummary(),
                    Evening = new PurchaseSummaryDto.TimeSummary()
                };
            }
        }


        public async Task<bool> DeleteAsync(int purchaseId, int tenantId)
        {
            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.TenantId == tenantId);

            if (purchase == null)
                return false;

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
            return true;
        }



    }
}