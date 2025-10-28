using MilkChillar.Application.Responses;


namespace MilkChillar.Application.DTOs.Reports
{
    // Detailed purchase report (individual entries)
    public class DetailedPurchaseReportDto
    {
        public List<PurchaseDetailDto> Purchases { get; set; } = new();
        public PurchaseReportSummaryDto Summary { get; set; } = new();
    }

    public class PagedPurchaseReportDto
    {
        public PaginatedResult<PurchaseDetailDto> PaginatedPurchases { get; set; } = new();
        
    }

    public class PurchaseDetailDto
    {
        public int PurchaseId { get; set; }
        public DateTime Date { get; set; }
        public string TimeOfDay { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string ExpenseAccountName { get; set; } = string.Empty;
        public string DodhiName { get; set; } = string.Empty;
        public int DodhiId { get; set; }
        public string ChillarName { get; set; } = string.Empty;
        public decimal GrossLiters { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Balance { get; set; }
    }

    // Summary by supplier (grouped by account)
    public class SupplierWisePurchaseReportDto
    {
        public List<SupplierPurchaseSummaryDto> SupplierSummaries { get; set; } = new();
        public PurchaseReportSummaryDto OverallSummary { get; set; } = new();
    }

    public class SupplierPurchaseSummaryDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal TotalLiters { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageRate { get; set; }
        public int TransactionCount { get; set; }
        public decimal Balance { get; set; }
    }

    // Common summary structure for both reports
    public class PurchaseReportSummaryDto
    {
        public decimal TotalLiters { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageRate { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalSuppliers { get; set; }
    }
}
