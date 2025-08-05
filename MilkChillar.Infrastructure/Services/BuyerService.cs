using MilkChillar.Application.DTOs.Buyers;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class BuyerService : IBuyerService
    {
        private readonly ApplicationDbContext _context;

        public BuyerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BuyerDto>> GetAllAsync(int tenantId)
        {
            return await _context.Buyers
                .Include(b => b.Account)
                .Where(b => b.TenantId == tenantId)
                .Select(b => new BuyerDto
                {
                    BuyerId = b.BuyerId,
                    TenantId = b.TenantId,
                    AccountId = b.AccountId,
                    //FullName = b.FullName,
                    Rate = b.Rate,
                    KhataNumber = b.KhataNumber,
                    CreditLimit = b.CreditLimit,
                    Address = b.Address,
                    IsActive = b.IsActive,
                    AccountCode = b.Account.AccountCode,
                    AccountName = b.Account.Name
                })
                .ToListAsync();
        }

        public async Task<BuyerDto?> GetByIdAsync(int id, int tenantId)
        {
            var b = await _context.Buyers
                .Include(b => b.Account)
                .FirstOrDefaultAsync(b => b.BuyerId == id && b.TenantId == tenantId);

            if (b == null) return null;

            return new BuyerDto
            {
                BuyerId = b.BuyerId,
                TenantId = b.TenantId,
                AccountId = b.AccountId,
                //FullName = b.FullName,
                Rate = b.Rate,
                KhataNumber = b.KhataNumber,
                CreditLimit = b.CreditLimit,
                Address = b.Address,
                IsActive = b.IsActive,
                AccountCode = b.Account.AccountCode,
                AccountName = b.Account.Name
            };
        }

        public async Task<BuyerDto> CreateAsync(CreateBuyerDto dto, int tenantId)
        {
            var buyer = new Buyer
            {
                TenantId = tenantId,
                AccountId = dto.AccountId,
                //FullName = dto.FullName,
                Rate = dto.Rate,
                KhataNumber = dto.KhataNumber,
                CreditLimit = dto.CreditLimit,
                Address = dto.Address,
                IsActive = dto.IsActive
            };

            _context.Buyers.Add(buyer);
            await _context.SaveChangesAsync();

            var account = await _context.Accounts.FindAsync(dto.AccountId);

            return new BuyerDto
            {
                BuyerId = buyer.BuyerId,
                TenantId = tenantId,
                AccountId = dto.AccountId,
                //FullName = dto.FullName,
                Rate = dto.Rate,
                KhataNumber = dto.KhataNumber,
                CreditLimit = dto.CreditLimit,
                Address = dto.Address,
                IsActive = dto.IsActive,
                AccountCode = account?.AccountCode,
                AccountName = account?.Name
            };
        }

        public async Task<BuyerDto?> UpdateAsync(int id, UpdateBuyerDto dto, int tenantId)
        {
            var buyer = await _context.Buyers
                .Include(b => b.Account)
                .FirstOrDefaultAsync(b => b.BuyerId == id && b.TenantId == tenantId);

            if (buyer == null) return null;

            //buyer.FullName = dto.FullName;
            buyer.Rate = dto.Rate;
            buyer.KhataNumber = dto.KhataNumber;
            buyer.CreditLimit = dto.CreditLimit;
            buyer.Address = dto.Address;
            buyer.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return new BuyerDto
            {
                BuyerId = buyer.BuyerId,
                TenantId = buyer.TenantId,
                AccountId = buyer.AccountId,
                //FullName = buyer.FullName,
                Rate = buyer.Rate,
                KhataNumber = buyer.KhataNumber,
                CreditLimit = buyer.CreditLimit,
                Address = buyer.Address,
                IsActive = buyer.IsActive,
                AccountCode = buyer.Account.AccountCode,
                AccountName = buyer.Account.Name
            };
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.BuyerId == id && b.TenantId == tenantId);
            if (buyer == null) return false;

            _context.Buyers.Remove(buyer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaginatedResult<BuyerDto>> GetBuyersAsync(BuyerQueryParameters query)
        {
            var buyersQuery = from account in _context.Accounts
                              where account.TenantId == query.TenantId &&
                                    account.AccountCode.StartsWith("100")
                              join buyer in _context.Buyers
                                  on account.AccountId equals buyer.AccountId into buyerJoin
                              from buyer in buyerJoin.DefaultIfEmpty() // LEFT JOIN
                              select new
                              {
                                  account.AccountId,
                                  account.TenantId,
                                  account.AccountCode,
                                  account.FullCode,
                                  account.Name,
                                  BuyerId = (int?)buyer.BuyerId,
                                  Rate = (decimal?)buyer.Rate,
                                  KhataNumber = buyer.KhataNumber,
                                  CreditLimit = (decimal?)buyer.CreditLimit,
                                  Address = buyer.Address,
                                  IsActive = (bool?)buyer.IsActive
                              };

            // 🔍 Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                buyersQuery = buyersQuery.Where(x =>
                    x.Name.Contains(query.Search) ||
                    x.KhataNumber.Contains(query.Search) ||
                    (x.Address != null && x.Address.Contains(query.Search))
                );
            }

            // ✅ Filter by IsActive if specified
            if (query.IsActive.HasValue)
            {
                buyersQuery = buyersQuery.Where(x => x.IsActive == query.IsActive);
            }

            var totalCount = await buyersQuery.CountAsync();
            var skip = (query.PageNumber - 1) * query.PageSize;

            var items = await buyersQuery
                .OrderBy(x => x.FullCode)
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            var resultDtos = items.Select(x => new BuyerDto
            {
                BuyerId = x.BuyerId ?? 0,
                TenantId = x.TenantId,
                AccountId = x.AccountId,
                AccountCode = x.AccountCode,
                AccountName = x.Name,
                Rate = x.Rate ?? 0,
                KhataNumber = x.KhataNumber ?? "",
                CreditLimit = x.CreditLimit ?? 0,
                Address = x.Address,
                IsActive = x.IsActive ?? false
            }).ToList();

            return new PaginatedResult<BuyerDto>
            {
                Items = resultDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

    }
}
