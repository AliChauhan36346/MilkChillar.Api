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
    public class CashReceiptService : ICashReceiptService
    {
        private readonly ApplicationDbContext _context;

        public CashReceiptService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<CashPaymentDto>> GetCashReceiptsAsync(CashPaymentQueryParameters query)
        {
            var cashReceiptsQuery = _context.CashReceipts
                .Include(cr => cr.CashAccount)
                .Include(cr => cr.User)
                .Include(cr => cr.JournalEntry)
                .Include(cr => cr.CashReceiptLines)
                    .ThenInclude(crl => crl.Account)
                .Where(cr => cr.TenantId == query.TenantId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                cashReceiptsQuery = cashReceiptsQuery.Where(cr =>
                    (cr.JobDescription != null && cr.JobDescription.Contains(query.Search)) ||
                    (cr.Remarks != null && cr.Remarks.Contains(query.Search)) ||
                    (cr.CashAccount != null && cr.CashAccount.Name.ToLower().Contains(query.Search.ToLower())) ||
                    (cr.CashAccount != null && cr.CashAccount.AccountCode.Contains(query.Search))
                );
            }

            if (query.VoucherNo.HasValue)
                cashReceiptsQuery = cashReceiptsQuery.Where(cr => cr.ReceiptNo == query.VoucherNo);

            if (query.FromDate.HasValue)
                cashReceiptsQuery = cashReceiptsQuery.Where(cr => cr.ReceiptDate >= query.FromDate);

            if (query.ToDate.HasValue)
                cashReceiptsQuery = cashReceiptsQuery.Where(cr => cr.ReceiptDate <= query.ToDate);

            if (query.CashAccountId.HasValue)
                cashReceiptsQuery = cashReceiptsQuery.Where(cr => cr.CashAccountId == query.CashAccountId);

            if (query.MinAmount.HasValue)
                cashReceiptsQuery = cashReceiptsQuery.Where(cr => cr.TotalAmount >= query.MinAmount);

            if (query.MaxAmount.HasValue)
                cashReceiptsQuery = cashReceiptsQuery.Where(cr => cr.TotalAmount <= query.MaxAmount);

            if (query.HasJournalEntry.HasValue)
                cashReceiptsQuery = cashReceiptsQuery.Where(cr =>
                    query.HasJournalEntry.Value ? cr.JournalEntryId != null : cr.JournalEntryId == null);

            var totalCount = await cashReceiptsQuery.CountAsync();
            var skip = (query.PageNumber - 1) * query.PageSize;

            var cashReceipts = await cashReceiptsQuery
                .OrderByDescending(cr => cr.ReceiptDate)
                .ThenByDescending(cr => cr.ReceiptNo)
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            var cashReceiptDtos = cashReceipts.Select(cr => new CashPaymentDto
            {
                CashPaymentId = cr.CashReceiptId,
                TenantId = cr.TenantId,
                VoucherNo = cr.ReceiptNo,
                PaymentDate = cr.ReceiptDate,
                JobDescription = cr.JobDescription,
                CashAccountId = cr.CashAccountId,
                TotalAmount = cr.TotalAmount,
                Remarks = cr.Remarks,
                AddedBy = cr.AddedBy,
                JournalEntryId = cr.JournalEntryId,
                CreatedAt = cr.CreatedAt,
                CashAccountCode = cr.CashAccount?.AccountCode,
                CashAccountName = cr.CashAccount?.Name,
                AddedByUsername = cr.User?.Username,
                JournalDescription = cr.JournalEntry?.Description,
                PaymentLines = cr.CashReceiptLines.Select(crl => new CashPaymentLineDto
                {
                    CashPaymentLineId = crl.CashReceiptLineId,
                    CashPaymentId = crl.CashReceiptId,
                    AccountId = crl.AccountId,
                    Description = crl.Description,
                    Amount = crl.Amount,
                    AccountCode = crl.Account?.AccountCode,
                    AccountName = crl.Account?.Name,
                    AccountFullCode = crl.Account?.FullCode
                }).ToList()
            }).ToList();

            return new PaginatedResult<CashPaymentDto>
            {
                Items = cashReceiptDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<CashPaymentDto?> GetByIdAsync(int id, int tenantId)
        {
            var cr = await _context.CashReceipts
                .Include(x => x.CashAccount)
                .Include(x => x.User)
                .Include(x => x.JournalEntry)
                .Include(x => x.CashReceiptLines)
                    .ThenInclude(crl => crl.Account)
                .FirstOrDefaultAsync(x => x.CashReceiptId == id && x.TenantId == tenantId);

            if (cr == null) return null;

            return new CashPaymentDto
            {
                CashPaymentId = cr.CashReceiptId,
                TenantId = cr.TenantId,
                VoucherNo = cr.ReceiptNo,
                PaymentDate = cr.ReceiptDate,
                JobDescription = cr.JobDescription,
                CashAccountId = cr.CashAccountId,
                TotalAmount = cr.TotalAmount,
                Remarks = cr.Remarks,
                AddedBy = cr.AddedBy,
                JournalEntryId = cr.JournalEntryId,
                CreatedAt = cr.CreatedAt,
                CashAccountCode = cr.CashAccount?.AccountCode,
                CashAccountName = cr.CashAccount?.Name,
                AddedByUsername = cr.User?.Username,
                JournalDescription = cr.JournalEntry?.Description,
                PaymentLines = cr.CashReceiptLines.Select(crl => new CashPaymentLineDto
                {
                    CashPaymentLineId = crl.CashReceiptLineId,
                    CashPaymentId = crl.CashReceiptId,
                    AccountId = crl.AccountId,
                    Description = crl.Description,
                    Amount = crl.Amount,
                    AccountCode = crl.Account?.AccountCode,
                    AccountName = crl.Account?.Name,
                    AccountFullCode = crl.Account?.FullCode
                }).ToList()
            };
        }

        public async Task<CashPaymentDto> CreateAsync(CreateCashPaymentDto dto, int tenantId, int userId)
        {
            // Validate that receipt lines total matches total amount
            var linesTotal = dto.PaymentLines.Sum(l => l.Amount);
            if (Math.Abs(dto.TotalAmount - linesTotal) > 0.01m)
                throw new ArgumentException($"Total amount ({dto.TotalAmount:C}) does not match receipt lines total ({linesTotal:C}).");

            // Get next receipt number
            var receiptNo = await GetNextReceiptNumberAsync(tenantId);

            var cashReceipt = new CashReceipt
            {
                TenantId = tenantId,
                ReceiptNo = receiptNo,
                ReceiptDate = dto.PaymentDate,
                JobDescription = dto.JobDescription,
                CashAccountId = dto.CashAccountId,
                TotalAmount = dto.TotalAmount,
                Remarks = dto.Remarks,
                AddedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Add receipt lines
            foreach (var lineDto in dto.PaymentLines)
            {
                cashReceipt.CashReceiptLines.Add(new CashReceiptLine
                {
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    Amount = lineDto.Amount
                });
            }

            _context.CashReceipts.Add(cashReceipt);
            await _context.SaveChangesAsync();

            // Load related data for response
            return await GetByIdAsync(cashReceipt.CashReceiptId, tenantId) ??
                throw new InvalidOperationException("Failed to retrieve created cash receipt.");
        }

        public async Task<CashPaymentDto?> UpdateAsync(int id, UpdateCashPaymentDto dto, int tenantId)
        {
            var cashReceipt = await _context.CashReceipts
                .Include(cr => cr.CashReceiptLines)
                .FirstOrDefaultAsync(cr => cr.CashReceiptId == id && cr.TenantId == tenantId);

            if (cashReceipt == null) return null;

            // Validate that receipt lines total matches total amount
            var linesTotal = dto.PaymentLines.Sum(l => l.Amount);
            if (Math.Abs(dto.TotalAmount - linesTotal) > 0.01m)
                throw new ArgumentException($"Total amount ({dto.TotalAmount:C}) does not match receipt lines total ({linesTotal:C}).");

            // Update header
            cashReceipt.ReceiptDate = dto.PaymentDate;
            cashReceipt.JobDescription = dto.JobDescription;
            cashReceipt.CashAccountId = dto.CashAccountId;
            cashReceipt.TotalAmount = dto.TotalAmount;
            cashReceipt.Remarks = dto.Remarks;

            // Update receipt lines - remove existing and add new ones
            _context.CashReceiptLines.RemoveRange(cashReceipt.CashReceiptLines);

            foreach (var lineDto in dto.PaymentLines)
            {
                cashReceipt.CashReceiptLines.Add(new CashReceiptLine
                {
                    CashReceiptId = cashReceipt.CashReceiptId,
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    Amount = lineDto.Amount
                });
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(cashReceipt.CashReceiptId, tenantId);
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var cashReceipt = await _context.CashReceipts
                .FirstOrDefaultAsync(cr => cr.CashReceiptId == id && cr.TenantId == tenantId);

            if (cashReceipt == null) return false;

            _context.CashReceipts.Remove(cashReceipt);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetNextReceiptNumberAsync(int tenantId)
        {
            var maxReceipt = await _context.CashReceipts
                .Where(cr => cr.TenantId == tenantId)
                .MaxAsync(cr => (int?)cr.ReceiptNo) ?? 0;

            return maxReceipt + 1;
        }
    }
}