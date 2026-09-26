using System;

namespace MilkChillar.Application.DTOs.Maintenance
{
    public class DataCleanupPreviewDto
    {
        public int PurchasesCount { get; set; }
        public int SalesCount { get; set; }
        public int ChillarReceivesCount { get; set; }
        public int StockEntriesCount { get; set; }
        public int CashPaymentsCount { get; set; }
        public int CashReceiptsCount { get; set; }
        public int BankPaymentsCount { get; set; }
        public int BankReceiptsCount { get; set; }
        public int JournalEntriesCount { get; set; }
        public int SuppliersCount { get; set; }
        public int BuyersCount { get; set; }
        public int EmployeesCount { get; set; }
        public int ChillarsCount { get; set; }
        public int AccountsCount { get; set; }
        public int TotalTransactionalRecords { get; set; }
        public int TotalMasterRecords { get; set; }
    }

    public class DataCleanupRequestDto
    {
        public bool PreserveAccounts { get; set; } = true;
        public bool ClearPurchases { get; set; } = true;
        public bool ClearSales { get; set; } = true;
        public bool ClearChillarReceives { get; set; } = true;
        public bool ClearStockEntries { get; set; } = true;
        public bool ClearPaymentsAndReceipts { get; set; } = true;
        public bool ClearJournalEntries { get; set; } = true;
        public string? ConfirmationText { get; set; }
        public string? Password { get; set; }
    }

    public class DataCleanupResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = default!;
        public int DeletedPurchases { get; set; }
        public int DeletedSales { get; set; }
        public int DeletedChillarReceives { get; set; }
        public int DeletedStockEntries { get; set; }
        public int DeletedPaymentsAndReceipts { get; set; }
        public int DeletedJournalEntries { get; set; }
        public int DeletedMasterEntities { get; set; }
        public int DeletedAccounts { get; set; }
        public int TotalDeleted { get; set; }
    }
}
