namespace MilkChillar.Application.DTOs.Reports
{
    /// <summary>
    /// Dodhi-wise purchase vs receive comparison report
    /// </summary>
    public class DodhiWisePurchaseReceiveReportDto
    {
        public List<DodhiSummaryDto> DodhiSummaries { get; set; } = new();
        public DodhiReportOverallSummaryDto OverallSummary { get; set; } = new();
    }

    public class DodhiSummaryDto
    {
        public int DodhiId { get; set; }
        public string DodhiName { get; set; } = string.Empty;
        public int ChillarId { get; set; }
        public string ChillarName { get; set; } = string.Empty;
        
        /// <summary>
        /// Total liters purchased (from suppliers)
        /// </summary>
        public decimal TotalPurchasedLiters { get; set; }
        
        /// <summary>
        /// Total amount paid for purchases
        /// </summary>
        public decimal TotalPurchaseAmount { get; set; }
        
        /// <summary>
        /// Average rate for purchases
        /// </summary>
        public decimal AveragePurchaseRate { get; set; }
        
        /// <summary>
        /// Number of purchase transactions
        /// </summary>
        public int PurchaseTransactionCount { get; set; }
        
        /// <summary>
        /// Total liters received (to chillar)
        /// </summary>
        public decimal TotalReceivedLiters { get; set; }
        
        /// <summary>
        /// Net liters (after losses)
        /// </summary>
        public decimal TotalNetLiters { get; set; }
        
        /// <summary>
        /// Loss during reception (Gross - Net)
        /// </summary>
        public decimal ReceptionLoss { get; set; }
        
        /// <summary>
        /// Loss percentage during reception
        /// </summary>
        public decimal ReceptionLossPercentage { get; set; }
        
        /// <summary>
        /// Number of receive transactions
        /// </summary>
        public int ReceiveTransactionCount { get; set; }
        
        /// <summary>
        /// Difference between purchased and received (can indicate loss/gain)
        /// </summary>
        public decimal PurchaseReceiveDifference { get; set; }
        
        /// <summary>
        /// Difference percentage
        /// </summary>
        public decimal PurchaseReceiveDifferencePercentage { get; set; }
    }

    public class DodhiReportOverallSummaryDto
    {
        /// <summary>
        /// Total liters purchased across all dodhis
        /// </summary>
        public decimal TotalPurchasedLiters { get; set; }
        
        /// <summary>
        /// Total purchase amount
        /// </summary>
        public decimal TotalPurchaseAmount { get; set; }
        
        /// <summary>
        /// Overall average purchase rate
        /// </summary>
        public decimal AveragePurchaseRate { get; set; }
        
        /// <summary>
        /// Total number of purchase transactions
        /// </summary>
        public int TotalPurchaseTransactions { get; set; }
        
        /// <summary>
        /// Total liters received
        /// </summary>
        public decimal TotalReceivedLiters { get; set; }
        
        /// <summary>
        /// Total net liters
        /// </summary>
        public decimal TotalNetLiters { get; set; }
        
        /// <summary>
        /// Total reception loss
        /// </summary>
        public decimal TotalReceptionLoss { get; set; }
        
        /// <summary>
        /// Overall reception loss percentage
        /// </summary>
        public decimal OverallReceptionLossPercentage { get; set; }
        
        /// <summary>
        /// Total number of receive transactions
        /// </summary>
        public int TotalReceiveTransactions { get; set; }
        
        /// <summary>
        /// Total difference
        /// </summary>
        public decimal TotalPurchaseReceiveDifference { get; set; }
        
        /// <summary>
        /// Number of unique dodhis
        /// </summary>
        public int TotalDodhis { get; set; }
    }

    /// <summary>
    /// Detailed transaction-wise purchase and receive data for a single dodhi
    /// </summary>
    public class DodhiPurchaseReceiveDetailDto
    {
        public int DodhiId { get; set; }
        public string DodhiName { get; set; } = string.Empty;
        public int ChillarId { get; set; }
        public string ChillarName { get; set; } = string.Empty;

        /// <summary>
        /// List of purchase transactions
        /// </summary>
        public List<DodhiPurchaseTransactionDto> PurchaseTransactions { get; set; } = new();

        /// <summary>
        /// List of receive transactions
        /// </summary>
        public List<DodhiReceiveTransactionDto> ReceiveTransactions { get; set; } = new();

        /// <summary>
        /// Overall summary for the period
        /// </summary>
        public DodhiDetailSummaryDto Summary { get; set; } = new();
    }

    public class DodhiPurchaseTransactionDto
    {
        public int PurchaseId { get; set; }
        public DateTime Date { get; set; }
        public string TimeOfDay { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal GrossLiters { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class DodhiReceiveTransactionDto
    {
        public int ReceiveId { get; set; }
        public DateTime Date { get; set; }
        public string TimeOfDay { get; set; } = string.Empty;
        public decimal GrossLiters { get; set; }
        public decimal? LR { get; set; }
        public decimal? Fat { get; set; }
        public decimal NetLiters { get; set; }
    }

    public class DodhiDetailSummaryDto
    {
        public decimal TotalPurchasedLiters { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public decimal AveragePurchaseRate { get; set; }
        public int PurchaseTransactionCount { get; set; }
        
        public decimal TotalReceivedLiters { get; set; }
        public decimal TotalNetLiters { get; set; }
        public decimal TotalReceptionLoss { get; set; }
        public decimal ReceptionLossPercentage { get; set; }
        public int ReceiveTransactionCount { get; set; }
        
        public decimal PurchaseReceiveDifference { get; set; }
        public decimal PurchaseReceiveDifferencePercentage { get; set; }
    }
}

