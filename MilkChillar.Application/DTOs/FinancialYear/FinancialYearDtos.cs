using System;
using System.Collections.Generic;

namespace MilkChillar.Application.DTOs.FinancialYear
{
    public class FinancialYearDto
    {
        public int FinancialYearId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int? ClosedBy { get; set; }
        public string? ClosedByUsername { get; set; }
        public int? ClosingJournalEntryId { get; set; }
        public int? RetainedEarningsAccountId { get; set; }
        public string? RetainedEarningsAccountName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status => IsClosed ? "Closed" : (IsActive ? "Active" : "Draft");
    }

    public class CreateFinancialYearDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool SetAsActive { get; set; } = false;
        public string? Notes { get; set; }
    }

    public class UpdateFinancialYearDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class CloseFinancialYearPreviewDto
    {
        public int FinancialYearId { get; set; }
        public string FinancialYearName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetProfitLoss { get; set; } // Positive = Profit, Negative = Loss
        public int AccountsToRollForwardCount { get; set; }
        public List<RollForwardAccountItemDto> RollForwardAccounts { get; set; } = new();
    }

    public class RollForwardAccountItemDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty; // Asset, Liability, Equity
        public decimal DebitTotal { get; set; }
        public decimal CreditTotal { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal NewDebitOpening { get; set; }
        public decimal NewCreditOpening { get; set; }
    }

    public class CloseFinancialYearDto
    {
        public int RetainedEarningsAccountId { get; set; }
        public int? NextFinancialYearId { get; set; }
        public bool CreateNextYearIfMissing { get; set; } = true;
        public string? NextYearName { get; set; }
        public string? NextYearCode { get; set; }
        public DateTime? NextYearStartDate { get; set; }
        public DateTime? NextYearEndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class FinancialYearCloseResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ClosedFinancialYearId { get; set; }
        public int ActiveFinancialYearId { get; set; }
        public int? ClosingJournalEntryId { get; set; }
        public int RolledForwardBalancesCount { get; set; }
        public decimal NetProfitLoss { get; set; }
    }
}
