using MilkChillar.Application.DTOs.Sales;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class SalesService : ISaleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SalesService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaginatedResult<SaleDto>> GetPaginatedAsync(SaleQueryParameters parameters)
        {
            var query = _context.Sales
                .Include(s => s.Account)
                .Include(s => s.RevenueAccount)
                .Include(s => s.Chillar)
                .Include(s => s.User)
                .Where(s => s.TenantId == parameters.TenantId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.Date)
                .ThenByDescending(s => s.SaleId)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(s => new SaleDto
                {
                    SaleId = s.SaleId,
                    Date = s.Date,
                    AccountId = s.AccountId,
                    AccountName = s.Account.Name,
                    AccountCode = s.Account.AccountCode, // 👈 ADD THIS
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

                .ToListAsync();

            return new PaginatedResult<SaleDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<SaleDto?> GetByIdAsync(int saleId, int tenantId)
        {
            var s = await _context.Sales
                .Include(s => s.Account)
                .Include(s => s.RevenueAccount)
                .Include(s => s.Chillar)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SaleId == saleId && s.TenantId == tenantId);

            if (s == null) return null;

            return new SaleDto
            {
                SaleId = s.SaleId,
                Date = s.Date,
                AccountId = s.AccountId,
                AccountCode=s.Account.AccountCode,
                AccountName = s.Account.Name,
                RevenueAccountId = s.RevenueAccountId,
                RevenueAccountName = s.RevenueAccount.Name,
                ChillarId = s.ChillarId,
                ChillarName = s.Chillar.Name,
                AddedById = s.AddedBy,
                AddedByName = s.User.Username,
                GrossLiters = s.GrossLiters,
                LR = s.LR,
                Fat = s.Fat,
                NetLiters = s.NetLiters,
                Rate = s.Rate,
                TotalAmount = s.NetLiters * s.Rate,
                AmountReceived = s.AmountReceived,
                Balance = s.Balance
            };
        }

        public async Task<SaleDto> CreateAsync(CreateSaleDto dto, int tenantId, int addedByUserId)
        {
            var sale = new Sales
            {
                TenantId = tenantId,
                Date = dto.Date,
                AccountId = dto.AccountId,
                RevenueAccountId = dto.RevenueAccountId,
                ChillarId = dto.ChillarId,
                AddedBy = addedByUserId,
                GrossLiters = dto.GrossLiters,
                LR = dto.LR,
                Fat = dto.Fat,
                NetLiters = dto.NetLiters,
                Rate = dto.Rate,
                AmountReceived = dto.AmountReceived,
                Balance = (dto.NetLiters * dto.Rate) - dto.AmountReceived
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            var account = await _context.Accounts.FindAsync(dto.AccountId);
            var revenueAccount = await _context.Accounts.FindAsync(dto.RevenueAccountId);
            var chillar = await _context.Chillars.FindAsync(dto.ChillarId);
            var user = await _context.Users.FindAsync(addedByUserId);

            return new SaleDto
            {
                SaleId = sale.SaleId,
                Date = sale.Date,
                AccountId = sale.AccountId,
                AccountCode = account?.AccountCode ?? "Unknown",
                AccountName = account?.Name ?? "Unknown",
                RevenueAccountName = revenueAccount?.Name ?? "Unknown",
                ChillarName = chillar?.Name ?? "Unknown",
                AddedByName = user?.Username ?? "Unknown",
                GrossLiters = sale.GrossLiters,
                LR = sale.LR,
                Fat = sale.Fat,
                NetLiters = sale.NetLiters,
                Rate = sale.Rate,
                TotalAmount = sale.NetLiters * sale.Rate,
                AmountReceived = sale.AmountReceived,
                Balance = sale.Balance
            };
        }

        public async Task<SaleDto?> UpdateAsync(int saleId, UpdateSaleDto dto, int tenantId)
        {
            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.SaleId == saleId && s.TenantId == tenantId);

            if (sale == null) return null;

            sale.Date = dto.Date;
            sale.AccountId = dto.AccountId;
            sale.RevenueAccountId = dto.RevenueAccountId;
            sale.ChillarId = dto.ChillarId;
            sale.GrossLiters = dto.GrossLiters;
            sale.LR = dto.LR;
            sale.Fat = dto.Fat;
            sale.NetLiters = dto.NetLiters;
            sale.Rate = dto.Rate;
            sale.AmountReceived = dto.AmountReceived;
            sale.Balance = (dto.NetLiters * dto.Rate) - dto.AmountReceived;

            await _context.SaveChangesAsync();

            // ✅ Refetch sale with related data after save to avoid null references
            sale = await _context.Sales
                .Include(s => s.Account)
                .Include(s => s.RevenueAccount)
                .Include(s => s.Chillar)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SaleId == saleId && s.TenantId == tenantId);

            if (sale == null) return null;

            return new SaleDto
            {
                SaleId = sale.SaleId,
                Date = sale.Date,
                AccountId = sale.AccountId,
                AccountCode = sale.Account.AccountCode,
                AccountName = sale.Account.Name,
                RevenueAccountName = sale.RevenueAccount.Name,
                ChillarName = sale.Chillar.Name,
                AddedByName = sale.User.Username,
                GrossLiters = sale.GrossLiters,
                LR = sale.LR,
                Fat = sale.Fat,
                NetLiters = sale.NetLiters,
                Rate = sale.Rate,
                TotalAmount = sale.NetLiters * sale.Rate,
                AmountReceived = sale.AmountReceived,
                Balance = sale.Balance
            };
        }


        public async Task<SalesMetadataDto> GetSalesMetadataAsync(DateOnly date, int tenantId, int userId)
        {
            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.TenantId == tenantId);

            var employee = user?.Employee;
            bool isChillarIncharge = employee?.Designation.ToLower() == "chillarincharge";
            int? chillarId = isChillarIncharge ? employee.ChillarId : null;

            // Get all buyers with their accounts
            var allBuyers = await _context.Buyers
                .Include(b => b.Account)
                .Where(b => b.TenantId == tenantId && b.Account.SubAccount.MainAccount.Name.ToLower() == "buyers")
                .ToListAsync();

            var revenueAccounts = await (
    from a in _context.Accounts
    join sa in _context.SubAccounts on a.SubAccountId equals sa.SubAccountId
    join ma in _context.MainAccounts on sa.MainAccountId equals ma.MainAccountId
    where a.TenantId == tenantId && ma.FinancialStatementComponent.ToLower() == "revenue"
    select new RevenueAccountDto
    {
        AccountId = a.AccountId,
        AccountCode = a.AccountCode,
        AccountName = a.Name
    }).ToListAsync();





            // Get added sales for this date
            var addedQuery = _context.Sales
                .Where(s => s.TenantId == tenantId && s.Date == date);

            if (isChillarIncharge && chillarId != null)
                addedQuery = addedQuery.Where(s => s.ChillarId == chillarId);

            var added = await addedQuery
                .Include(s => s.Account)
                .Include(s => s.RevenueAccount)
                .Include(s => s.Chillar)
                .Include(s => s.User)
                .ToListAsync();

            var addedDtos = added.Select(s => new SaleDto
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
            }).ToList();

            var addedAccountIds = added.Select(s => s.AccountId).ToHashSet();

            var remaining = allBuyers
                .Where(b => !addedAccountIds.Contains(b.AccountId))
                .Select(b => new RemainingAccountDto
                {
                    AccountId = b.AccountId,
                    AccountName = b.Account.Name,
                    AccountCode = b.Account.AccountCode,
                    Rate = b.Rate // 👈 Rate coming from Buyer entity
                }).ToList();


            return new SalesMetadataDto
            {
                ChillarId = chillarId,
                AddedSales = addedDtos,
                RemainingAccounts = remaining,
                RevenueAccounts = revenueAccounts
            };

        }

        public async Task<bool> DeleteAsync(int saleId, int tenantId)
        {
            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.SaleId == saleId && s.TenantId == tenantId);

            if (sale == null)
                return false;

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
