using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MilkChillar.Application;
using MilkChillar.Application.DTOs.Roznamcha;
using MilkChillar.Application.Interfaces;

namespace MilkChillar.Infrastructure.Services
{
    public class RoznamchaService : IRoznamchaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoznamchaService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetTenantIdFromToken()
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id");
            if (tenantIdClaim == null || !int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                throw new UnauthorizedAccessException("Tenant ID not found in token");
            }
            return tenantId;
        }

        public async Task<RoznamchaResponse> GetRoznamchaAsync(RoznamchaFilterRequest request)
        {
            int tenantId = GetTenantIdFromToken();

            // Collect all transactions
            var allTransactions = new List<RoznamchaEntryDto>();

            // Convert dates to UTC for PostgreSQL
            var startDate = request.StartDate.HasValue
                ? DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc)
                : (DateTime?)null;
            var endDate = request.EndDate.HasValue
                ? DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
                : (DateTime?)null;

            // Fetch Cash Payments
            if (request.ViewType == null || request.ViewType == "ALL" || request.ViewType == "CASH")
            {
                var cashPaymentsQuery = _context.CashPayments
                    .Include(cp => cp.CashAccount)
                    .Include(cp => cp.User)
                    .Include(cp => cp.CashPaymentLines)
                        .ThenInclude(cpl => cpl.Account)
                    .Where(cp => cp.TenantId == tenantId);

                if (startDate.HasValue)
                    cashPaymentsQuery = cashPaymentsQuery.Where(cp => cp.PaymentDate >= startDate.Value);
                if (endDate.HasValue)
                    cashPaymentsQuery = cashPaymentsQuery.Where(cp => cp.PaymentDate <= endDate.Value);
                if (request.CashAccountId.HasValue)
                    cashPaymentsQuery = cashPaymentsQuery.Where(cp => cp.CashAccountId == request.CashAccountId.Value);
                if (!string.IsNullOrEmpty(request.Search))
                    cashPaymentsQuery = cashPaymentsQuery.Where(cp =>
                        cp.JobDescription.Contains(request.Search) ||
                        cp.VoucherNo.ToString().Contains(request.Search));

                if (request.TransactionType == null || request.TransactionType == "ALL" || request.TransactionType == "PAYMENT")
                {
                    var cashPayments = await cashPaymentsQuery.ToListAsync();
                    allTransactions.AddRange(cashPayments.Select(cp => new RoznamchaEntryDto
                    {
                        Id = cp.CashPaymentId,
                        Date = cp.PaymentDate,
                        VoucherType = "CASH_PAYMENT",
                        VoucherNo = $"CP-{cp.VoucherNo}",
                        JobDescription = cp.JobDescription,
                        CashOrBankAccount = cp.CashAccount.Name,
                        CashOrBankAccountId = cp.CashAccountId,
                        PayeeOrRecipient = string.Join(", ", cp.CashPaymentLines.Select(l => l.Account.Name).Distinct()),
                        Amount = cp.TotalAmount,
                        Remarks = cp.Remarks,
                        CreatedBy = cp.User?.Username,
                        CreatedAt = cp.CreatedAt,
                        Lines = cp.CashPaymentLines.Select(l => new TransactionLineDto
                        {
                            AccountCode = l.Account.AccountCode,
                            AccountName = l.Account.Name,
                            Description = l.Description,
                            Amount = l.Amount
                        }).ToList()
                    }));
                }
            }

            // Fetch Cash Receipts
            if (request.ViewType == null || request.ViewType == "ALL" || request.ViewType == "CASH")
            {
                var cashReceiptsQuery = _context.CashReceipts
                    .Include(cr => cr.CashAccount)
                    .Include(cr => cr.User)
                    .Include(cr => cr.CashReceiptLines)
                        .ThenInclude(crl => crl.Account)
                    .Where(cr => cr.TenantId == tenantId);

                if (startDate.HasValue)
                    cashReceiptsQuery = cashReceiptsQuery.Where(cr => cr.ReceiptDate >= startDate.Value);
                if (endDate.HasValue)
                    cashReceiptsQuery = cashReceiptsQuery.Where(cr => cr.ReceiptDate <= endDate.Value);
                if (request.CashAccountId.HasValue)
                    cashReceiptsQuery = cashReceiptsQuery.Where(cr => cr.CashAccountId == request.CashAccountId.Value);
                if (!string.IsNullOrEmpty(request.Search))
                    cashReceiptsQuery = cashReceiptsQuery.Where(cr =>
                        cr.JobDescription.Contains(request.Search) ||
                        cr.ReceiptNo.ToString().Contains(request.Search));

                if (request.TransactionType == null || request.TransactionType == "ALL" || request.TransactionType == "RECEIPT")
                {
                    var cashReceipts = await cashReceiptsQuery.ToListAsync();
                    allTransactions.AddRange(cashReceipts.Select(cr => new RoznamchaEntryDto
                    {
                        Id = cr.CashReceiptId,
                        Date = cr.ReceiptDate,
                        VoucherType = "CASH_RECEIPT",
                        VoucherNo = $"CR-{cr.ReceiptNo}",
                        JobDescription = cr.JobDescription,
                        CashOrBankAccount = cr.CashAccount.Name,
                        CashOrBankAccountId = cr.CashAccountId,
                        PayeeOrRecipient = string.Join(", ", cr.CashReceiptLines.Select(l => l.Account.Name).Distinct()),
                        Amount = cr.TotalAmount,
                        Remarks = cr.Remarks,
                        CreatedBy = cr.User?.Username,
                        CreatedAt = cr.CreatedAt,
                        Lines = cr.CashReceiptLines.Select(l => new TransactionLineDto
                        {
                            AccountCode = l.Account.AccountCode,
                            AccountName = l.Account.Name,
                            Description = l.Description,
                            Amount = l.Amount
                        }).ToList()
                    }));
                }
            }

            // Fetch Bank Payments
            if (request.ViewType == null || request.ViewType == "ALL" || request.ViewType == "BANK")
            {
                var bankPaymentsQuery = _context.BankPayments
                    .Include(bp => bp.BankAccount)
                    .Include(bp => bp.User)
                    .Include(bp => bp.BankPaymentLines)
                        .ThenInclude(bpl => bpl.Account)
                    .Where(bp => bp.TenantId == tenantId);

                if (startDate.HasValue)
                    bankPaymentsQuery = bankPaymentsQuery.Where(bp => bp.PaymentDate >= startDate.Value);
                if (endDate.HasValue)
                    bankPaymentsQuery = bankPaymentsQuery.Where(bp => bp.PaymentDate <= endDate.Value);
                if (request.BankAccountId.HasValue)
                    bankPaymentsQuery = bankPaymentsQuery.Where(bp => bp.BankAccountId == request.BankAccountId.Value);
                if (!string.IsNullOrEmpty(request.Search))
                    bankPaymentsQuery = bankPaymentsQuery.Where(bp =>
                        bp.JobDescription.Contains(request.Search) ||
                        bp.VoucherNo.ToString().Contains(request.Search) ||
                        bp.ChequeNo.Contains(request.Search));

                if (request.TransactionType == null || request.TransactionType == "ALL" || request.TransactionType == "PAYMENT")
                {
                    var bankPayments = await bankPaymentsQuery.ToListAsync();
                    allTransactions.AddRange(bankPayments.Select(bp => new RoznamchaEntryDto
                    {
                        Id = bp.BankPaymentId,
                        Date = bp.PaymentDate,
                        VoucherType = "BANK_PAYMENT",
                        VoucherNo = $"BP-{bp.VoucherNo}",
                        JobDescription = bp.JobDescription,
                        CashOrBankAccount = bp.BankAccount.Name,
                        CashOrBankAccountId = bp.BankAccountId,
                        PayeeOrRecipient = string.Join(", ", bp.BankPaymentLines.Select(l => l.Account.Name).Distinct()),
                        Amount = bp.TotalAmount,
                        ChequeNo = bp.ChequeNo,
                        ChequeDate = bp.ChequeDate,
                        Remarks = bp.Remarks,
                        CreatedBy = bp.User?.Username,
                        CreatedAt = bp.CreatedAt,
                        Lines = bp.BankPaymentLines.Select(l => new TransactionLineDto
                        {
                            AccountCode = l.Account.AccountCode,
                            AccountName = l.Account.Name,
                            Description = l.Description,
                            Amount = l.Amount
                        }).ToList()
                    }));
                }
            }

            // Fetch Bank Receipts
            if (request.ViewType == null || request.ViewType == "ALL" || request.ViewType == "BANK")
            {
                var bankReceiptsQuery = _context.BankReceipts
                    .Include(br => br.BankAccount)
                    .Include(br => br.User)
                    .Include(br => br.BankReceiptLines)
                        .ThenInclude(brl => brl.Account)
                    .Where(br => br.TenantId == tenantId);

                if (startDate.HasValue)
                    bankReceiptsQuery = bankReceiptsQuery.Where(br => br.ReceiptDate >= startDate.Value);
                if (endDate.HasValue)
                    bankReceiptsQuery = bankReceiptsQuery.Where(br => br.ReceiptDate <= endDate.Value);
                if (request.BankAccountId.HasValue)
                    bankReceiptsQuery = bankReceiptsQuery.Where(br => br.BankAccountId == request.BankAccountId.Value);
                if (!string.IsNullOrEmpty(request.Search))
                    bankReceiptsQuery = bankReceiptsQuery.Where(br =>
                        br.JobDescription.Contains(request.Search) ||
                        br.ReceiptNo.ToString().Contains(request.Search) ||
                        br.InstrumentNo.Contains(request.Search));

                if (request.TransactionType == null || request.TransactionType == "ALL" || request.TransactionType == "RECEIPT")
                {
                    var bankReceipts = await bankReceiptsQuery.ToListAsync();
                    allTransactions.AddRange(bankReceipts.Select(br => new RoznamchaEntryDto
                    {
                        Id = br.BankReceiptId,
                        Date = br.ReceiptDate,
                        VoucherType = "BANK_RECEIPT",
                        VoucherNo = $"BR-{br.ReceiptNo}",
                        JobDescription = br.JobDescription,
                        CashOrBankAccount = br.BankAccount.Name,
                        CashOrBankAccountId = br.BankAccountId,
                        PayeeOrRecipient = string.Join(", ", br.BankReceiptLines.Select(l => l.Account.Name).Distinct()),
                        Amount = br.TotalAmount,
                        ChequeNo = br.InstrumentNo,
                        ChequeDate = br.InstrumentDate,
                        Remarks = br.Remarks,
                        CreatedBy = br.User?.Username,
                        CreatedAt = br.CreatedAt,
                        Lines = br.BankReceiptLines.Select(l => new TransactionLineDto
                        {
                            AccountCode = l.Account.AccountCode,
                            AccountName = l.Account.Name,
                            Description = l.Description,
                            Amount = l.Amount
                        }).ToList()
                    }));
                }
            }

            // Sort by date descending
            allTransactions = allTransactions.OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt).ToList();

            // Calculate summary
            var summary = new RoznamchaSummaryDto
            {
                TotalPayments = allTransactions.Where(t => t.VoucherType.Contains("PAYMENT")).Sum(t => t.Amount),
                TotalReceipts = allTransactions.Where(t => t.VoucherType.Contains("RECEIPT")).Sum(t => t.Amount),
                TotalTransactions = allTransactions.Count,
                CashPayments = allTransactions.Where(t => t.VoucherType == "CASH_PAYMENT").Sum(t => t.Amount),
                CashReceipts = allTransactions.Where(t => t.VoucherType == "CASH_RECEIPT").Sum(t => t.Amount),
                BankPayments = allTransactions.Where(t => t.VoucherType == "BANK_PAYMENT").Sum(t => t.Amount),
                BankReceipts = allTransactions.Where(t => t.VoucherType == "BANK_RECEIPT").Sum(t => t.Amount),
            };
            summary.NetAmount = summary.TotalReceipts - summary.TotalPayments;

            // Pagination
            int totalCount = allTransactions.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)request.Limit);
            var pagedTransactions = allTransactions
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .ToList();

            return new RoznamchaResponse
            {
                Entries = pagedTransactions,
                Summary = summary,
                Pagination = new PaginationDto
                {
                    CurrentPage = request.Page,
                    PerPage = request.Limit,
                    TotalPages = totalPages,
                    TotalRecords = totalCount,
                    HasNext = request.Page < totalPages,
                    HasPrevious = request.Page > 1
                }
            };
        }

        public async Task<RoznamchaSummaryResponse> GetRoznamchaSummaryAsync(RoznamchaFilterRequest request)
        {
            int tenantId = GetTenantIdFromToken();

            var fullResponse = await GetRoznamchaAsync(request);

            return new RoznamchaSummaryResponse
            {
                Period = new PeriodDto
                {
                    StartDate = request.StartDate ?? DateTime.MinValue,
                    EndDate = request.EndDate ?? DateTime.MaxValue
                },
                Summary = fullResponse.Summary
            };
        }

        public async Task<RoznamchaResponse> GetRoznamchaByAccountAsync(int accountId, RoznamchaFilterRequest request)
        {
            // Determine if the account is cash or bank by checking account code
            int tenantId = GetTenantIdFromToken();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.TenantId == tenantId);

            if (account != null)
            {
                // Check if it's a cash account (starts with 110)
                if (account.FullCode.StartsWith("110"))
                {
                    request.CashAccountId = accountId;
                    request.ViewType = "CASH";
                }
                // Check if it's a bank account (starts with 120)
                else if (account.FullCode.StartsWith("120"))
                {
                    request.BankAccountId = accountId;
                    request.ViewType = "BANK";
                }
            }

            return await GetRoznamchaAsync(request);
        }

        public async Task<RoznamchaResponse> GetCashBookAsync(RoznamchaFilterRequest request)
        {
            request.ViewType = "CASH";
            return await GetRoznamchaAsync(request);
        }

        public async Task<RoznamchaResponse> GetBankBookAsync(int? bankAccountId, RoznamchaFilterRequest request)
        {
            request.ViewType = "BANK";
            if (bankAccountId.HasValue)
            {
                request.BankAccountId = bankAccountId.Value;
            }
            return await GetRoznamchaAsync(request);
        }

        public async Task<DayBookResponse> GetDayBookAsync(RoznamchaFilterRequest request)
        {
            int tenantId = GetTenantIdFromToken();

            // Get all transactions first
            var allTransactionsRequest = new RoznamchaFilterRequest
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ViewType = request.ViewType,
                CashAccountId = request.CashAccountId,
                BankAccountId = request.BankAccountId,
                TransactionType = request.TransactionType,
                Search = request.Search,
                Page = 1,
                Limit = 10000 // Get all for grouping
            };

            var allTransactions = await GetRoznamchaAsync(allTransactionsRequest);

            // Group by date
            var dayBookEntries = allTransactions.Entries
                .GroupBy(t => t.Date.Date)
                .Select(g => new DayBookEntryDto
                {
                    Date = g.Key,
                    TotalPayments = g.Where(t => t.VoucherType.Contains("PAYMENT")).Sum(t => t.Amount),
                    TotalReceipts = g.Where(t => t.VoucherType.Contains("RECEIPT")).Sum(t => t.Amount),
                    Net = g.Where(t => t.VoucherType.Contains("RECEIPT")).Sum(t => t.Amount) -
                          g.Where(t => t.VoucherType.Contains("PAYMENT")).Sum(t => t.Amount),
                    TransactionCount = g.Count(),
                    CashPayments = g.Where(t => t.VoucherType == "CASH_PAYMENT").Sum(t => t.Amount),
                    CashReceipts = g.Where(t => t.VoucherType == "CASH_RECEIPT").Sum(t => t.Amount),
                    BankPayments = g.Where(t => t.VoucherType == "BANK_PAYMENT").Sum(t => t.Amount),
                    BankReceipts = g.Where(t => t.VoucherType == "BANK_RECEIPT").Sum(t => t.Amount)
                })
                .OrderByDescending(d => d.Date)
                .ToList();

            return new DayBookResponse
            {
                Entries = dayBookEntries
            };
        }

        public async Task<List<AccountOptionDto>> GetCashAccountsAsync()
        {
            int tenantId = GetTenantIdFromToken();

            var cashAccounts = await _context.Accounts
                .Include(a => a.SubAccount)
                    .ThenInclude(sa => sa.MainAccount)
                .Where(a => a.TenantId == tenantId && a.FullCode.StartsWith("100")) // Assuming cash accounts start with 100
                .Select(a => new AccountOptionDto
                {
                    AccountId = a.AccountId,
                    AccountName = a.Name,
                    AccountCode = a.FullCode
                })
                .OrderBy(a => a.AccountName)
                .ToListAsync();

            return cashAccounts;
        }

        

        public async Task<byte[]> ExportRoznamchaAsync(RoznamchaFilterRequest request, string format)
        {
            // TODO: Implement export functionality
            throw new NotImplementedException("Export functionality will be implemented based on your preferred library");
        }
    }
}