using MilkChillar.Application.DTOs.CashPayments;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Application;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Infrastructure.Services
{
    public class CashPaymentService : ICashPaymentService
    {
        private readonly ApplicationDbContext _context;

        public CashPaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<CashPaymentDto>> GetCashPaymentsAsync(CashPaymentQueryParameters query)
        {
            var cashPaymentsQuery = _context.CashPayments
                .Include(cp => cp.CashAccount)
                .Include(cp => cp.User)
                .Include(cp => cp.JournalEntry)
                .Include(cp => cp.CashPaymentLines)
                    .ThenInclude(cpl => cpl.Account)
                .Where(cp => cp.TenantId == query.TenantId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                cashPaymentsQuery = cashPaymentsQuery.Where(cp =>
                    (cp.JobDescription != null && cp.JobDescription.Contains(query.Search)) ||
                    (cp.Remarks != null && cp.Remarks.Contains(query.Search)) ||
                    (cp.CashAccount != null && cp.CashAccount.Name.ToLower().Contains(query.Search.ToLower())) ||
                    (cp.CashAccount != null && cp.CashAccount.AccountCode.Contains(query.Search))
                );
            }

            if (query.VoucherNo.HasValue)
                cashPaymentsQuery = cashPaymentsQuery.Where(cp => cp.VoucherNo == query.VoucherNo);

            if (query.FromDate.HasValue)
                cashPaymentsQuery = cashPaymentsQuery.Where(cp => cp.PaymentDate >= query.FromDate);

            if (query.ToDate.HasValue)
                cashPaymentsQuery = cashPaymentsQuery.Where(cp => cp.PaymentDate <= query.ToDate);

            if (query.CashAccountId.HasValue)
                cashPaymentsQuery = cashPaymentsQuery.Where(cp => cp.CashAccountId == query.CashAccountId);

            if (query.MinAmount.HasValue)
                cashPaymentsQuery = cashPaymentsQuery.Where(cp => cp.TotalAmount >= query.MinAmount);

            if (query.MaxAmount.HasValue)
                cashPaymentsQuery = cashPaymentsQuery.Where(cp => cp.TotalAmount <= query.MaxAmount);

            if (query.HasJournalEntry.HasValue)
                cashPaymentsQuery = cashPaymentsQuery.Where(cp =>
                    query.HasJournalEntry.Value ? cp.JournalEntryId != null : cp.JournalEntryId == null);

            var totalCount = await cashPaymentsQuery.CountAsync();
            var skip = (query.PageNumber - 1) * query.PageSize;

            var cashPayments = await cashPaymentsQuery
                .OrderByDescending(cp => cp.PaymentDate)
                .ThenByDescending(cp => cp.VoucherNo)
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            var cashPaymentDtos = cashPayments.Select(cp => new CashPaymentDto
            {
                CashPaymentId = cp.CashPaymentId,
                TenantId = cp.TenantId,
                VoucherNo = cp.VoucherNo,
                PaymentDate = cp.PaymentDate,
                JobDescription = cp.JobDescription,
                CashAccountId = cp.CashAccountId,
                TotalAmount = cp.TotalAmount,
                Remarks = cp.Remarks,
                AddedBy = cp.AddedBy,
                JournalEntryId = cp.JournalEntryId,
                CreatedAt = cp.CreatedAt,
                CashAccountCode = cp.CashAccount?.AccountCode,
                CashAccountName = cp.CashAccount?.Name,
                AddedByUsername = cp.User?.Username,
                JournalDescription = cp.JournalEntry?.Description,
                PaymentLines = cp.CashPaymentLines.Select(cpl => new CashPaymentLineDto
                {
                    CashPaymentLineId = cpl.CashPaymentLineId,
                    CashPaymentId = cpl.CashPaymentId,
                    AccountId = cpl.AccountId,
                    Description = cpl.Description,
                    Amount = cpl.Amount,
                    AccountCode = cpl.Account?.AccountCode,
                    AccountName = cpl.Account?.Name,
                    AccountFullCode = cpl.Account?.FullCode
                }).ToList()
            }).ToList();

            return new PaginatedResult<CashPaymentDto>
            {
                Items = cashPaymentDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<CashPaymentDto?> GetByIdAsync(int id, int tenantId)
        {
            var cp = await _context.CashPayments
                .Include(x => x.CashAccount)
                .Include(x => x.User)
                .Include(x => x.JournalEntry)
                .Include(x => x.CashPaymentLines)
                    .ThenInclude(cpl => cpl.Account)
                .FirstOrDefaultAsync(x => x.CashPaymentId == id && x.TenantId == tenantId);

            if (cp == null) return null;

            return new CashPaymentDto
            {
                CashPaymentId = cp.CashPaymentId,
                TenantId = cp.TenantId,
                VoucherNo = cp.VoucherNo,
                PaymentDate = cp.PaymentDate,
                JobDescription = cp.JobDescription,
                CashAccountId = cp.CashAccountId,
                TotalAmount = cp.TotalAmount,
                Remarks = cp.Remarks,
                AddedBy = cp.AddedBy,
                JournalEntryId = cp.JournalEntryId,
                CreatedAt = cp.CreatedAt,
                CashAccountCode = cp.CashAccount?.AccountCode,
                CashAccountName = cp.CashAccount?.Name,
                AddedByUsername = cp.User?.Username,
                JournalDescription = cp.JournalEntry?.Description,
                PaymentLines = cp.CashPaymentLines.Select(cpl => new CashPaymentLineDto
                {
                    CashPaymentLineId = cpl.CashPaymentLineId,
                    CashPaymentId = cpl.CashPaymentId,
                    AccountId = cpl.AccountId,
                    Description = cpl.Description,
                    Amount = cpl.Amount,
                    AccountCode = cpl.Account?.AccountCode,
                    AccountName = cpl.Account?.Name,
                    AccountFullCode = cpl.Account?.FullCode
                }).ToList()
            };
        }

        public async Task<CashPaymentDto> CreateAsync(CreateCashPaymentDto dto, int tenantId, int userId)
        {
            // Validate that payment lines total matches total amount
            var linesTotal = dto.PaymentLines.Sum(l => l.Amount);
            if (Math.Abs(dto.TotalAmount - linesTotal) > 0.01m)
                throw new ArgumentException($"Total amount ({dto.TotalAmount:C}) does not match payment lines total ({linesTotal:C}).");

            // Get next voucher number
            var voucherNo = await GetNextVoucherNumberAsync(tenantId);

            var cashPayment = new CashPayment
            {
                TenantId = tenantId,
                VoucherNo = voucherNo,
                PaymentDate = dto.PaymentDate,
                JobDescription = dto.JobDescription,
                CashAccountId = dto.CashAccountId,
                TotalAmount = dto.TotalAmount,
                Remarks = dto.Remarks,
                AddedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Add payment lines
            foreach (var lineDto in dto.PaymentLines)
            {
                cashPayment.CashPaymentLines.Add(new CashPaymentLine
                {
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    Amount = lineDto.Amount
                });
            }

            _context.CashPayments.Add(cashPayment);
            await _context.SaveChangesAsync();

            // Load related data for response
            return await GetByIdAsync(cashPayment.CashPaymentId, tenantId) ??
                throw new InvalidOperationException("Failed to retrieve created cash payment.");
        }

        public async Task<CashPaymentDto?> UpdateAsync(int id, UpdateCashPaymentDto dto, int tenantId)
        {
            var cashPayment = await _context.CashPayments
                .Include(cp => cp.CashPaymentLines)
                .FirstOrDefaultAsync(cp => cp.CashPaymentId == id && cp.TenantId == tenantId);

            if (cashPayment == null) return null;

            // Validate that payment lines total matches total amount
            var linesTotal = dto.PaymentLines.Sum(l => l.Amount);
            if (Math.Abs(dto.TotalAmount - linesTotal) > 0.01m)
                throw new ArgumentException($"Total amount ({dto.TotalAmount:C}) does not match payment lines total ({linesTotal:C}).");

            // Update header
            cashPayment.PaymentDate = dto.PaymentDate;
            cashPayment.JobDescription = dto.JobDescription;
            cashPayment.CashAccountId = dto.CashAccountId;
            cashPayment.TotalAmount = dto.TotalAmount;
            cashPayment.Remarks = dto.Remarks;

            // Update payment lines - remove existing and add new ones
            _context.CashPaymentLines.RemoveRange(cashPayment.CashPaymentLines);

            foreach (var lineDto in dto.PaymentLines)
            {
                cashPayment.CashPaymentLines.Add(new CashPaymentLine
                {
                    CashPaymentId = cashPayment.CashPaymentId,
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    Amount = lineDto.Amount
                });
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(cashPayment.CashPaymentId, tenantId);
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var cashPayment = await _context.CashPayments
                .FirstOrDefaultAsync(cp => cp.CashPaymentId == id && cp.TenantId == tenantId);

            if (cashPayment == null) return false;

            _context.CashPayments.Remove(cashPayment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetNextVoucherNumberAsync(int tenantId)
        {
            var maxVoucher = await _context.CashPayments
                .Where(cp => cp.TenantId == tenantId)
                .MaxAsync(cp => (int?)cp.VoucherNo) ?? 0;

            return maxVoucher + 1;
        }
    }
}
