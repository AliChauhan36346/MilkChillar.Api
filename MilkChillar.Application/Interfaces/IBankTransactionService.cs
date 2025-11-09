using MilkChillar.Application.DTOs.BankTransactions;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    public interface IBankTransactionService
    {
        // Bank Payment Methods
        Task<PaginatedResult<BankTransactionDto>> GetBankPaymentsAsync(BankTransactionQueryParameters query);
        Task<BankTransactionDto?> GetBankPaymentByIdAsync(int id, int tenantId);
        Task<BankTransactionDto> CreateBankPaymentAsync(CreateBankTransactionDto dto, int tenantId, int userId);
        Task<BankTransactionDto?> UpdateBankPaymentAsync(int id, UpdateBankTransactionDto dto, int tenantId);
        Task<bool> DeleteBankPaymentAsync(int id, int tenantId);
        Task<int> GetNextBankPaymentVoucherNumberAsync(int tenantId);

        // Bank Receipt Methods
        Task<PaginatedResult<BankTransactionDto>> GetBankReceiptsAsync(BankTransactionQueryParameters query);
        Task<BankTransactionDto?> GetBankReceiptByIdAsync(int id, int tenantId);
        Task<BankTransactionDto> CreateBankReceiptAsync(CreateBankTransactionDto dto, int tenantId, int userId);
        Task<BankTransactionDto?> UpdateBankReceiptAsync(int id, UpdateBankTransactionDto dto, int tenantId);
        Task<bool> DeleteBankReceiptAsync(int id, int tenantId);
        Task<int> GetNextBankReceiptNumberAsync(int tenantId);
    }
}