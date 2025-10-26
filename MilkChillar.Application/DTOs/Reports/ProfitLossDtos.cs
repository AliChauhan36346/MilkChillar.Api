using System;
using System.Collections.Generic;

namespace MilkChillar.Application.DTOs.Reports
{
    // ========================================
    // REQUEST DTOs
    // ========================================

    public class ProfitLossFilterRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; } = "json"; // json, pdf, excel
    }

    public class ComparativeProfitLossRequest
    {
        public DateTime Period1StartDate { get; set; }
        public DateTime Period1EndDate { get; set; }
        public DateTime Period2StartDate { get; set; }
        public DateTime Period2EndDate { get; set; }
    }

    // ========================================
    // MAIN RESPONSE DTOs
    // ========================================

    public class ProfitLossResponse
    {
        public PeriodDto Period { get; set; } = new();
        public IncomeDto Income { get; set; } = new();
        public CogsDto Cogs { get; set; } = new();
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public OperatingExpensesDto OperatingExpenses { get; set; } = new();
        public FinancialExpensesDto FinancialExpenses { get; set; } = new();
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal NetProfitMargin { get; set; }
    }

    public class PeriodDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    // ========================================
    // INCOME DTOs
    // ========================================

    public class IncomeDto
    {
        public decimal SalesRevenue { get; set; }
        public decimal SalesReturns { get; set; }
        public decimal NetSales { get; set; }
        public decimal OtherIncome { get; set; }
        public decimal TotalIncome { get; set; }
        public List<AccountBreakdownDto> Details { get; set; } = new();
    }

    // ========================================
    // COST OF GOODS SOLD DTOs
    // ========================================

    public class CogsDto
    {
        public decimal OpeningStock { get; set; }
        public decimal Purchases { get; set; }
        public decimal PurchaseReturns { get; set; }
        public decimal DirectExpenses { get; set; }
        public decimal ClosingStock { get; set; }
        public decimal TotalCogs { get; set; }
        public List<AccountBreakdownDto> Details { get; set; } = new();
    }

    // ========================================
    // OPERATING EXPENSES DTOs
    // ========================================

    public class OperatingExpensesDto
    {
        public decimal Salaries { get; set; }
        public decimal Rent { get; set; }
        public decimal Utilities { get; set; }
        public decimal Transportation { get; set; }
        public decimal Marketing { get; set; }
        public decimal OfficeExpenses { get; set; }
        public decimal Depreciation { get; set; }
        public decimal OtherExpenses { get; set; }
        public decimal TotalOperatingExpenses { get; set; }
        public List<AccountBreakdownDto> Details { get; set; } = new();
    }

    // ========================================
    // FINANCIAL EXPENSES DTOs
    // ========================================

    public class FinancialExpensesDto
    {
        public decimal InterestExpense { get; set; }
        public decimal BankCharges { get; set; }
        public decimal OtherFinancialExpenses { get; set; }
        public decimal TotalFinancialExpenses { get; set; }
        public List<AccountBreakdownDto> Details { get; set; } = new();
    }

    // ========================================
    // ACCOUNT BREAKDOWN DTO
    // ========================================

    public class AccountBreakdownDto
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }

    // ========================================
    // COMPARATIVE REPORT DTOs
    // ========================================

    public class ProfitLossComparativeResponse
    {
        public ProfitLossResponse Period1 { get; set; } = new();
        public ProfitLossResponse Period2 { get; set; } = new();
        public ComparisonDto Comparison { get; set; } = new();
    }

    public class ComparisonDto
    {
        public decimal RevenueChange { get; set; }
        public decimal RevenueChangePercentage { get; set; }
        public decimal ExpenseChange { get; set; }
        public decimal ExpenseChangePercentage { get; set; }
        public decimal NetProfitChange { get; set; }
        public decimal NetProfitChangePercentage { get; set; }
    }

    // ========================================
    // EXPENSE BREAKDOWN DTOs
    // ========================================

    public class ExpenseBreakdownResponse
    {
        public PeriodDto Period { get; set; } = new();
        public List<ExpenseCategoryDto> Categories { get; set; } = new();
        public decimal TotalExpenses { get; set; }
    }

    public class ExpenseCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public List<AccountBreakdownDto> Accounts { get; set; } = new();
    }

    // ========================================
    // INCOME BREAKDOWN DTOs
    // ========================================

    public class IncomeBreakdownResponse
    {
        public PeriodDto Period { get; set; } = new();
        public List<IncomeCategoryDto> Categories { get; set; } = new();
        public decimal TotalIncome { get; set; }
    }

    public class IncomeCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public List<AccountBreakdownDto> Accounts { get; set; } = new();
    }
}