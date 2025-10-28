namespace MilkChillar.Application.DTOs.Reports
{
    // Buyer-wise sales summary (grouped by buyer)
    public class BuyerWiseSalesReportDto
    {
        public List<BuyerSalesSummaryDto> BuyerSummaries { get; set; } = new();
        public SalesReportSummaryDto OverallSummary { get; set; } = new();
    }

    public class BuyerSalesSummaryDto
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal TotalGrossLiters { get; set; }
        public decimal TotalNetLiters { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountReceived { get; set; }
        public decimal AverageRate { get; set; }
        public decimal AverageLR { get; set; }
        public decimal AverageFat { get; set; }
        public int TransactionCount { get; set; }
        public decimal Balance { get; set; }
    }

    public class SalesReportSummaryDto
    {
        public decimal TotalGrossLiters { get; set; }
        public decimal TotalNetLiters { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountReceived { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal AverageRate { get; set; }
        public decimal AverageLR { get; set; }
        public decimal AverageFat { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalBuyers { get; set; }
    }
}