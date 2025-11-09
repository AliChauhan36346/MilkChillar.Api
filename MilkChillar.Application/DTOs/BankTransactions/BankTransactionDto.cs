using System;
using System.Collections.Generic;

namespace MilkChillar.Application.DTOs.BankTransactions
{
    public class BankTransactionDto
    {
        public int TransactionId { get; set; }
        public int TenantId { get; set; }
        public int VoucherNo { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? JobDescription { get; set; }
        public int BankAccountId { get; set; }
        public string? InstrumentNo { get; set; }
        public DateTime? InstrumentDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remarks { get; set; }
        public int AddedBy { get; set; }
        public int? JournalEntryId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public string? BankAccountCode { get; set; }
        public string? BankAccountName { get; set; }
        public string? AddedByUsername { get; set; }
        public string? JournalDescription { get; set; }
        public List<BankTransactionLineDto> TransactionLines { get; set; } = new List<BankTransactionLineDto>();
    }

    public class BankTransactionLineDto
    {
        public int TransactionLineId { get; set; }
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }

        // Navigation properties
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? AccountFullCode { get; set; }
    }

    public class CreateBankTransactionDto
    {
        public DateTime TransactionDate { get; set; }
        public string? JobDescription { get; set; }
        public int BankAccountId { get; set; }
        public string? InstrumentNo { get; set; }
        public DateTime? InstrumentDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remarks { get; set; }
        public List<CreateBankTransactionLineDto> TransactionLines { get; set; } = new List<CreateBankTransactionLineDto>();
    }

    public class CreateBankTransactionLineDto
    {
        public int AccountId { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class UpdateBankTransactionDto
    {
        public DateTime TransactionDate { get; set; }
        public string? JobDescription { get; set; }
        public int BankAccountId { get; set; }
        public string? InstrumentNo { get; set; }
        public DateTime? InstrumentDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remarks { get; set; }
        public List<UpdateBankTransactionLineDto> TransactionLines { get; set; } = new List<UpdateBankTransactionLineDto>();
    }

    public class UpdateBankTransactionLineDto
    {
        public int AccountId { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
    }
}