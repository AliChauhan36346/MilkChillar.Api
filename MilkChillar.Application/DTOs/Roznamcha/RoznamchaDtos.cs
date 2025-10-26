using System;
using System.Collections.Generic;

namespace MilkChillar.Application.DTOs.Roznamcha
{
    // Request DTOs
    public class RoznamchaFilterRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ViewType { get; set; } // ALL, CASH, BANK
        public int? CashAccountId { get; set; }
        public int? BankAccountId { get; set; }
        public string? TransactionType { get; set; } // PAYMENT, RECEIPT, ALL
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 50;
    }

    // Response DTOs
    public class RoznamchaResponse
    {
        public List<RoznamchaEntryDto> Entries { get; set; } = new();
        public RoznamchaSummaryDto Summary { get; set; } = new();
        public PaginationDto Pagination { get; set; } = new();
    }

    public class RoznamchaEntryDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string VoucherType { get; set; } = string.Empty; // CASH_PAYMENT, CASH_RECEIPT, BANK_PAYMENT, BANK_RECEIPT
        public string VoucherNo { get; set; } = string.Empty;
        public string? JobDescription { get; set; }
        public string CashOrBankAccount { get; set; } = string.Empty; // Cash/Bank account name
        public int CashOrBankAccountId { get; set; }
        public string PayeeOrRecipient { get; set; } = string.Empty; // Main account(s) from lines
        public decimal Amount { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TransactionLineDto> Lines { get; set; } = new();
    }

    public class TransactionLineDto
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class RoznamchaSummaryDto
    {
        public decimal TotalPayments { get; set; }
        public decimal TotalReceipts { get; set; }
        public decimal NetAmount { get; set; }
        public int TotalTransactions { get; set; }
        public decimal CashPayments { get; set; }
        public decimal CashReceipts { get; set; }
        public decimal BankPayments { get; set; }
        public decimal BankReceipts { get; set; }
    }

    public class PaginationDto
    {
        public int CurrentPage { get; set; }
        public int PerPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }

    public class AccountOptionDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
    }

    public class RoznamchaSummaryResponse
    {
        public PeriodDto Period { get; set; } = new();
        public RoznamchaSummaryDto Summary { get; set; } = new();
    }

    public class PeriodDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class DayBookResponse
    {
        public List<DayBookEntryDto> Entries { get; set; } = new();
    }

    public class DayBookEntryDto
    {
        public DateTime Date { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalReceipts { get; set; }
        public decimal Net { get; set; }
        public int TransactionCount { get; set; }
        public decimal CashPayments { get; set; }
        public decimal CashReceipts { get; set; }
        public decimal BankPayments { get; set; }
        public decimal BankReceipts { get; set; }
    }
}