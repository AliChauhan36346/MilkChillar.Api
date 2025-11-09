using MilkChillar.Application.DTOs.BankTransactions;
using MilkChillar.Application.Interfaces;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using MilkChillar.Application;
using MilkChillar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MilkChillar.Infrastructure.Services
{
    public class BankTransactionService : IBankTransactionService
    {
        private readonly ApplicationDbContext _context;

        public BankTransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Bank Payment Methods

        public async Task<PaginatedResult<BankTransactionDto>> GetBankPaymentsAsync(BankTransactionQueryParameters query)
        {
            var bankPaymentsQuery = _context.BankPayments
                .Include(bp => bp.BankAccount)
                .Include(bp => bp.User)
                .Include(bp => bp.JournalEntry)
                .Include(bp => bp.BankPaymentLines)
                    .ThenInclude(bpl => bpl.Account)
                .Where(bp => bp.TenantId == query.TenantId)
                .AsQueryable();

            // Apply filters
            bankPaymentsQuery = ApplyFilters(bankPaymentsQuery, query);

            var totalCount = await bankPaymentsQuery.CountAsync();
            var skip = (query.PageNumber - 1) * query.PageSize;

            var bankPayments = await bankPaymentsQuery
                .OrderByDescending(bp => bp.PaymentDate)
                .ThenByDescending(bp => bp.VoucherNo)
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            var bankPaymentDtos = bankPayments.Select(bp => MapBankPaymentToDto(bp)).ToList();

            return new PaginatedResult<BankTransactionDto>
            {
                Items = bankPaymentDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<BankTransactionDto?> GetBankPaymentByIdAsync(int id, int tenantId)
        {
            var bp = await _context.BankPayments
                .Include(x => x.BankAccount)
                .Include(x => x.User)
                .Include(x => x.JournalEntry)
                .Include(x => x.BankPaymentLines)
                    .ThenInclude(bpl => bpl.Account)
                .FirstOrDefaultAsync(x => x.BankPaymentId == id && x.TenantId == tenantId);

            return bp == null ? null : MapBankPaymentToDto(bp);
        }

        public async Task<BankTransactionDto> CreateBankPaymentAsync(CreateBankTransactionDto dto, int tenantId, int userId)
        {
            // Validate that payment lines total matches total amount
            var linesTotal = dto.TransactionLines.Sum(l => l.Amount);
            if (Math.Abs(dto.TotalAmount - linesTotal) > 0.01m)
                throw new ArgumentException($"Total amount ({dto.TotalAmount:C}) does not match payment lines total ({linesTotal:C}).");

            // Get next voucher number
            var voucherNo = await GetNextBankPaymentVoucherNumberAsync(tenantId);

            var bankPayment = new BankPayment
            {
                TenantId = tenantId,
                VoucherNo = voucherNo,
                PaymentDate = dto.TransactionDate,
                JobDescription = dto.JobDescription,
                BankAccountId = dto.BankAccountId,
                ChequeNo = dto.InstrumentNo,
                ChequeDate = dto.InstrumentDate,
                TotalAmount = dto.TotalAmount,
                Remarks = dto.Remarks,
                AddedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Add payment lines
            foreach (var lineDto in dto.TransactionLines)
            {
                bankPayment.BankPaymentLines.Add(new BankPaymentLine
                {
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    Amount = lineDto.Amount
                });
            }

            _context.BankPayments.Add(bankPayment);
            await _context.SaveChangesAsync();

            // Load related data for response
            return await GetBankPaymentByIdAsync(bankPayment.BankPaymentId, tenantId) ??
                throw new InvalidOperationException("Failed to retrieve created bank payment.");
        }

        public async Task<BankTransactionDto?> UpdateBankPaymentAsync(int id, UpdateBankTransactionDto dto, int tenantId)
        {
            var bankPayment = await _context.BankPayments
                .Include(bp => bp.BankPaymentLines)
                .FirstOrDefaultAsync(bp => bp.BankPaymentId == id && bp.TenantId == tenantId);

            if (bankPayment == null) return null;

            // Validate that payment lines total matches total amount
            var linesTotal = dto.TransactionLines.Sum(l => l.Amount);
            if (Math.Abs(dto.TotalAmount - linesTotal) > 0.01m)
                throw new ArgumentException($"Total amount ({dto.TotalAmount:C}) does not match payment lines total ({linesTotal:C}).");

            // Update header
            bankPayment.PaymentDate = dto.TransactionDate;
            bankPayment.JobDescription = dto.JobDescription;
            bankPayment.BankAccountId = dto.BankAccountId;
            bankPayment.ChequeNo = dto.InstrumentNo;
            bankPayment.ChequeDate = dto.InstrumentDate;
            bankPayment.TotalAmount = dto.TotalAmount;
            bankPayment.Remarks = dto.Remarks;

            // Update payment lines - remove existing and add new ones
            _context.BankPaymentLines.RemoveRange(bankPayment.BankPaymentLines);

            foreach (var lineDto in dto.TransactionLines)
            {
                bankPayment.BankPaymentLines.Add(new BankPaymentLine
                {
                    BankPaymentId = bankPayment.BankPaymentId,
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    Amount = lineDto.Amount
                });
            }

            await _context.SaveChangesAsync();

            return await GetBankPaymentByIdAsync(bankPayment.BankPaymentId, tenantId);
        }

        public async Task<bool> DeleteBankPaymentAsync(int id, int tenantId)
        {
            var bankPayment = await _context.BankPayments
                .FirstOrDefaultAsync(bp => bp.BankPaymentId == id && bp.TenantId == tenantId);

            if (bankPayment == null) return false;

            _context.BankPayments.Remove(bankPayment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetNextBankPaymentVoucherNumberAsync(int tenantId)
        {
            var maxVoucher = await _context.BankPayments
                .Where(bp => bp.TenantId == tenantId)
                .MaxAsync(bp => (int?)bp.VoucherNo) ?? 0;

            return maxVoucher + 1;
        }

        #endregion

        #region Bank Receipt Methods

        public async Task<PaginatedResult<BankTransactionDto>> GetBankReceiptsAsync(BankTransactionQueryParameters query)
        {
            var bankReceiptsQuery = _context.BankReceipts
                .Include(br => br.BankAccount)
                .Include(br => br.User)
                .Include(br => br.JournalEntry)
                .Include(br => br.BankReceiptLines)
                    .ThenInclude(brl => brl.Account)
                .Where(br => br.TenantId == query.TenantId)
                .AsQueryable();

            // Apply filters
            bankReceiptsQuery = ApplyFiltersForReceipts(bankReceiptsQuery, query);

            var totalCount = await bankReceiptsQuery.CountAsync();
            var skip = (query.PageNumber - 1) * query.PageSize;

            var bankReceipts = await bankReceiptsQuery
                .OrderByDescending(br => br.ReceiptDate)
                .ThenByDescending(br => br.ReceiptNo)
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            var bankReceiptDtos = bankReceipts.Select(br => MapBankReceiptToDto(br)).ToList();

            return new PaginatedResult<BankTransactionDto>
            {
                Items = bankReceiptDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<BankTransactionDto?> GetBankReceiptByIdAsync(int id, int tenantId)
        {
            var br = await _context.BankReceipts
                .Include(x => x.BankAccount)
                .Include(x => x.User)
                .Include(x => x.JournalEntry)
                .Include(x => x.BankReceiptLines)
                    .ThenInclude(brl => brl.Account)
                .FirstOrDefaultAsync(x => x.BankReceiptId == id && x.TenantId == tenantId);

            return br == null ? null : MapBankReceiptToDto(br);
        }

        public async Task<BankTransactionDto> CreateBankReceiptAsync(CreateBankTransactionDto dto, int tenantId, int userId)
        {
            // Validate that receipt lines total matches total amount
            var linesTotal = dto.TransactionLines.Sum(l => l.Amount);
            if (Math.Abs(dto.TotalAmount - linesTotal) > 0.01m)
                throw new ArgumentException($"Total amount ({dto.TotalAmount:C}) does not match receipt lines total ({linesTotal:C}).");

            // Get next receipt number
            var receiptNo = await GetNextBankReceiptNumberAsync(tenantId);

            var bankReceipt = new BankReceipt
            {
                TenantId = tenantId,
                ReceiptNo = receiptNo,
                ReceiptDate = dto.TransactionDate,
                JobDescription = dto.JobDescription,
                BankAccountId = dto.BankAccountId,
                InstrumentNo = dto.InstrumentNo,
                InstrumentDate = dto.InstrumentDate,
                TotalAmount = dto.TotalAmount,
                Remarks = dto.Remarks,
                AddedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Add receipt lines
            foreach (var lineDto in dto.TransactionLines)
            {
                bankReceipt.BankReceiptLines.Add(new BankReceiptLine
                {
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    Amount = lineDto.Amount
                });
            }

            _context.BankReceipts.Add(bankReceipt);
            await _context.SaveChangesAsync();

            // Load related data for response
            return await GetBankReceiptByIdAsync(bankReceipt.BankReceiptId, tenantId) ??
                throw new InvalidOperationException("Failed to retrieve created bank receipt.");
        }

        public async Task<BankTransactionDto?> UpdateBankReceiptAsync(int id, UpdateBankTransactionDto dto, int tenantId)
        {
            var bankReceipt = await _context.BankReceipts
                .Include(br => br.BankReceiptLines)
                .FirstOrDefaultAsync(br => br.BankReceiptId == id && br.TenantId == tenantId);

            if (bankReceipt == null) return null;

            // Validate that receipt lines total matches total amount
            var linesTotal = dto.TransactionLines.Sum(l => l.Amount);
            if (Math.Abs(dto.TotalAmount - linesTotal) > 0.01m)
                throw new ArgumentException($"Total amount ({dto.TotalAmount:C}) does not match receipt lines total ({linesTotal:C}).");

            // Update header
            bankReceipt.ReceiptDate = dto.TransactionDate;
            bankReceipt.JobDescription = dto.JobDescription;
            bankReceipt.BankAccountId = dto.BankAccountId;
            bankReceipt.InstrumentNo = dto.InstrumentNo;
            bankReceipt.InstrumentDate = dto.InstrumentDate;
            bankReceipt.TotalAmount = dto.TotalAmount;
            bankReceipt.Remarks = dto.Remarks;

            // Update receipt lines - remove existing and add new ones
            _context.BankReceiptLines.RemoveRange(bankReceipt.BankReceiptLines);

            foreach (var lineDto in dto.TransactionLines)
            {
                bankReceipt.BankReceiptLines.Add(new BankReceiptLine
                {
                    BankReceiptId = bankReceipt.BankReceiptId,
                    AccountId = lineDto.AccountId,
                    Description = lineDto.Description,
                    Amount = lineDto.Amount
                });
            }

            await _context.SaveChangesAsync();

            return await GetBankReceiptByIdAsync(bankReceipt.BankReceiptId, tenantId);
        }

        public async Task<bool> DeleteBankReceiptAsync(int id, int tenantId)
        {
            var bankReceipt = await _context.BankReceipts
                .FirstOrDefaultAsync(br => br.BankReceiptId == id && br.TenantId == tenantId);

            if (bankReceipt == null) return false;

            _context.BankReceipts.Remove(bankReceipt);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetNextBankReceiptNumberAsync(int tenantId)
        {
            var maxReceipt = await _context.BankReceipts
                .Where(br => br.TenantId == tenantId)
                .MaxAsync(br => (int?)br.ReceiptNo) ?? 0;

            return maxReceipt + 1;
        }

        #endregion

        #region Helper Methods

        private IQueryable<BankPayment> ApplyFilters(IQueryable<BankPayment> query, BankTransactionQueryParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(bp =>
                    (bp.JobDescription != null && bp.JobDescription.Contains(parameters.Search)) ||
                    (bp.Remarks != null && bp.Remarks.Contains(parameters.Search)) ||
                    (bp.ChequeNo != null && bp.ChequeNo.Contains(parameters.Search)) ||
                    (bp.BankAccount != null && bp.BankAccount.Name.ToLower().Contains(parameters.Search.ToLower())) ||
                    (bp.BankAccount != null && bp.BankAccount.AccountCode.Contains(parameters.Search))
                );
            }

            if (parameters.VoucherNo.HasValue)
                query = query.Where(bp => bp.VoucherNo == parameters.VoucherNo);

            if (parameters.FromDate.HasValue)
                query = query.Where(bp => bp.PaymentDate >= parameters.FromDate);

            if (parameters.ToDate.HasValue)
                query = query.Where(bp => bp.PaymentDate <= parameters.ToDate);

            if (parameters.BankAccountId.HasValue)
                query = query.Where(bp => bp.BankAccountId == parameters.BankAccountId);

            if (parameters.MinAmount.HasValue)
                query = query.Where(bp => bp.TotalAmount >= parameters.MinAmount);

            if (parameters.MaxAmount.HasValue)
                query = query.Where(bp => bp.TotalAmount <= parameters.MaxAmount);

            if (parameters.HasJournalEntry.HasValue)
                query = query.Where(bp =>
                    parameters.HasJournalEntry.Value ? bp.JournalEntryId != null : bp.JournalEntryId == null);

            if (!string.IsNullOrWhiteSpace(parameters.InstrumentNo))
                query = query.Where(bp => bp.ChequeNo != null && bp.ChequeNo.Contains(parameters.InstrumentNo));

            return query;
        }

        private IQueryable<BankReceipt> ApplyFiltersForReceipts(IQueryable<BankReceipt> query, BankTransactionQueryParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(br =>
                    (br.JobDescription != null && br.JobDescription.Contains(parameters.Search)) ||
                    (br.Remarks != null && br.Remarks.Contains(parameters.Search)) ||
                    (br.InstrumentNo != null && br.InstrumentNo.Contains(parameters.Search)) ||
                    (br.BankAccount != null && br.BankAccount.Name.ToLower().Contains(parameters.Search.ToLower())) ||
                    (br.BankAccount != null && br.BankAccount.AccountCode.Contains(parameters.Search))
                );
            }

            if (parameters.VoucherNo.HasValue)
                query = query.Where(br => br.ReceiptNo == parameters.VoucherNo);

            if (parameters.FromDate.HasValue)
                query = query.Where(br => br.ReceiptDate >= parameters.FromDate);

            if (parameters.ToDate.HasValue)
                query = query.Where(br => br.ReceiptDate <= parameters.ToDate);

            if (parameters.BankAccountId.HasValue)
                query = query.Where(br => br.BankAccountId == parameters.BankAccountId);

            if (parameters.MinAmount.HasValue)
                query = query.Where(br => br.TotalAmount >= parameters.MinAmount);

            if (parameters.MaxAmount.HasValue)
                query = query.Where(br => br.TotalAmount <= parameters.MaxAmount);

            if (parameters.HasJournalEntry.HasValue)
                query = query.Where(br =>
                    parameters.HasJournalEntry.Value ? br.JournalEntryId != null : br.JournalEntryId == null);

            if (!string.IsNullOrWhiteSpace(parameters.InstrumentNo))
                query = query.Where(br => br.InstrumentNo != null && br.InstrumentNo.Contains(parameters.InstrumentNo));

            return query;
        }

        private BankTransactionDto MapBankPaymentToDto(BankPayment bp)
        {
            return new BankTransactionDto
            {
                TransactionId = bp.BankPaymentId,
                TenantId = bp.TenantId,
                VoucherNo = bp.VoucherNo,
                TransactionDate = bp.PaymentDate,
                JobDescription = bp.JobDescription,
                BankAccountId = bp.BankAccountId,
                InstrumentNo = bp.ChequeNo,
                InstrumentDate = bp.ChequeDate,
                TotalAmount = bp.TotalAmount,
                Remarks = bp.Remarks,
                AddedBy = bp.AddedBy,
                JournalEntryId = bp.JournalEntryId,
                CreatedAt = bp.CreatedAt,
                BankAccountCode = bp.BankAccount?.AccountCode,
                BankAccountName = bp.BankAccount?.Name,
                AddedByUsername = bp.User?.Username,
                JournalDescription = bp.JournalEntry?.Description,
                TransactionLines = bp.BankPaymentLines.Select(bpl => new BankTransactionLineDto
                {
                    TransactionLineId = bpl.BankPaymentLineId,
                    TransactionId = bpl.BankPaymentId,
                    AccountId = bpl.AccountId,
                    Description = bpl.Description,
                    Amount = bpl.Amount,
                    AccountCode = bpl.Account?.AccountCode,
                    AccountName = bpl.Account?.Name,
                    AccountFullCode = bpl.Account?.FullCode
                }).ToList()
            };
        }

        private BankTransactionDto MapBankReceiptToDto(BankReceipt br)
        {
            return new BankTransactionDto
            {
                TransactionId = br.BankReceiptId,
                TenantId = br.TenantId,
                VoucherNo = br.ReceiptNo,
                TransactionDate = br.ReceiptDate,
                JobDescription = br.JobDescription,
                BankAccountId = br.BankAccountId,
                InstrumentNo = br.InstrumentNo,
                InstrumentDate = br.InstrumentDate,
                TotalAmount = br.TotalAmount,
                Remarks = br.Remarks,
                AddedBy = br.AddedBy,
                JournalEntryId = br.JournalEntryId,
                CreatedAt = br.CreatedAt,
                BankAccountCode = br.BankAccount?.AccountCode,
                BankAccountName = br.BankAccount?.Name,
                AddedByUsername = br.User?.Username,
                JournalDescription = br.JournalEntry?.Description,
                TransactionLines = br.BankReceiptLines.Select(brl => new BankTransactionLineDto
                {
                    TransactionLineId = brl.BankReceiptLineId,
                    TransactionId = brl.BankReceiptId,
                    AccountId = brl.AccountId,
                    Description = brl.Description,
                    Amount = brl.Amount,
                    AccountCode = brl.Account?.AccountCode,
                    AccountName = brl.Account?.Name,
                    AccountFullCode = brl.Account?.FullCode
                }).ToList()
            };
        }

        #endregion
    }
}