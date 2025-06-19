using MilkChillar.Application.DTOs.Suppliers;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;

namespace MilkChillar.Infrastructure.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;

        public SupplierService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SupplierDto>> GetAllAsync(int tenantId)
        {
            return await _context.Suppliers
                .Include(s => s.Dodhi)
                .Include(s => s.Account)
                .Where(s => s.TenantId == tenantId)
                .Select(s => new SupplierDto
                {
                    SupplierId = s.SupplierId,
                    AccountId = s.AccountId,
                    FullName = s.FullName,
                    Rate = s.Rate,
                    KhataNumber = s.KhataNumber,
                    CreditLimit = s.CreditLimit,
                    DodhiId = s.DodhiId,
                    Address = s.Address,
                    GiveCreditOnParchi = s.GiveCreditOnParchi,
                    IsActive = s.IsActive,
                    DodhiName = s.Dodhi != null ? s.Dodhi.FullName : null,
                    AccountCode = s.Account != null ? s.Account.AccountCode : null,
                    AccountName = s.Account != null ? s.Account.Name : null
                }).ToListAsync();
        }

        public async Task<PaginatedResult<SupplierDto>> GetSuppliersAsync(SupplierQueryParameters query)
        {
            var suppliersQuery = _context.Suppliers
                .Include(s => s.Dodhi)
                .Include(s => s.Account)
                .Where(s => s.TenantId == query.TenantId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                suppliersQuery = suppliersQuery.Where(s =>
                    s.FullName.Contains(query.Search) ||
                    s.KhataNumber.Contains(query.Search) ||
                    (s.Address != null && s.Address.Contains(query.Search))
                );
            }

            if (query.IsActive.HasValue)
                suppliersQuery = suppliersQuery.Where(s => s.IsActive == query.IsActive);

            if (query.GiveCreditOnParchi.HasValue)
                suppliersQuery = suppliersQuery.Where(s => s.GiveCreditOnParchi == query.GiveCreditOnParchi);

            if (query.DodhiId.HasValue)
                suppliersQuery = suppliersQuery.Where(s => s.DodhiId == query.DodhiId);

            var totalCount = await suppliersQuery.CountAsync();
            var skip = (query.PageNumber - 1) * query.PageSize;

            var suppliers = await suppliersQuery
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            var supplierDtos = suppliers.Select(s => new SupplierDto
            {
                SupplierId = s.SupplierId,
                AccountId = s.AccountId,
                FullName = s.FullName,
                Rate = s.Rate,
                KhataNumber = s.KhataNumber,
                CreditLimit = s.CreditLimit,
                DodhiId = s.DodhiId,
                Address = s.Address,
                GiveCreditOnParchi = s.GiveCreditOnParchi,
                IsActive = s.IsActive,
                DodhiName = s.Dodhi?.FullName,
                AccountCode = s.Account?.FullCode,
                AccountName = s.Account?.Name
            }).ToList();

            return new PaginatedResult<SupplierDto>
            {
                Items = supplierDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<SupplierDto?> GetByIdAsync(int id, int tenantId)
        {
            var s = await _context.Suppliers
                .Include(x => x.Dodhi)
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.SupplierId == id && x.TenantId == tenantId);

            if (s == null) return null;

            return new SupplierDto
            {
                SupplierId = s.SupplierId,
                AccountId = s.AccountId,
                FullName = s.FullName,
                Rate = s.Rate,
                KhataNumber = s.KhataNumber,
                CreditLimit = s.CreditLimit,
                DodhiId = s.DodhiId,
                Address = s.Address,
                GiveCreditOnParchi = s.GiveCreditOnParchi,
                IsActive = s.IsActive,
                DodhiName = s.Dodhi?.FullName,
                AccountCode = s.Account?.AccountCode,
                AccountName = s.Account?.Name
            };
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto, int tenantId)
        {
            var supplier = new Supplier
            {
                TenantId = tenantId,
                AccountId = dto.AccountId,
                FullName = dto.FullName,
                Rate = dto.Rate,
                KhataNumber = dto.KhataNumber,
                CreditLimit = dto.CreditLimit,
                DodhiId = dto.DodhiId,
                Address = dto.Address,
                GiveCreditOnParchi = dto.GiveCreditOnParchi,
                IsActive = dto.IsActive
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            var account = await _context.Accounts.FindAsync(supplier.AccountId);
            var dodhi = supplier.DodhiId.HasValue
                ? await _context.Employees.FindAsync(supplier.DodhiId)
                : null;

            return new SupplierDto
            {
                SupplierId = supplier.SupplierId,
                AccountId = supplier.AccountId,
                FullName = supplier.FullName,
                Rate = supplier.Rate,
                KhataNumber = supplier.KhataNumber,
                CreditLimit = supplier.CreditLimit,
                DodhiId = supplier.DodhiId,
                Address = supplier.Address,
                GiveCreditOnParchi = supplier.GiveCreditOnParchi,
                IsActive = supplier.IsActive,
                AccountCode = account?.AccountCode,
                AccountName = account?.Name,
                DodhiName = dodhi?.FullName
            };
        }

        public async Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto, int tenantId)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierId == id && s.TenantId == tenantId);
            if (supplier == null) return null;

            supplier.AccountId = dto.AccountId;
            supplier.FullName = dto.FullName;
            supplier.Rate = dto.Rate;
            supplier.KhataNumber = dto.KhataNumber;
            supplier.CreditLimit = dto.CreditLimit;
            supplier.DodhiId = dto.DodhiId;
            supplier.Address = dto.Address;
            supplier.GiveCreditOnParchi = dto.GiveCreditOnParchi;
            supplier.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            var account = await _context.Accounts.FindAsync(supplier.AccountId);
            var dodhi = supplier.DodhiId.HasValue
                ? await _context.Employees.FindAsync(supplier.DodhiId)
                : null;

            return new SupplierDto
            {
                SupplierId = supplier.SupplierId,
                AccountId = supplier.AccountId,
                FullName = supplier.FullName,
                Rate = supplier.Rate,
                KhataNumber = supplier.KhataNumber,
                CreditLimit = supplier.CreditLimit,
                DodhiId = supplier.DodhiId,
                Address = supplier.Address,
                GiveCreditOnParchi = supplier.GiveCreditOnParchi,
                IsActive = supplier.IsActive,
                AccountCode = account?.AccountCode,
                AccountName = account?.Name,
                DodhiName = dodhi?.FullName
            };
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierId == id && s.TenantId == tenantId);
            if (supplier == null) return false;

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
