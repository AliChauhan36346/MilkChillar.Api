using MilkChillar.Application.DTOs.CashPayments;
using MilkChillar.Application.Parameters;
using MilkChillar.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    public interface ICashPaymentService
    {
        Task<PaginatedResult<CashPaymentDto>> GetCashPaymentsAsync(CashPaymentQueryParameters query);
        Task<CashPaymentDto?> GetByIdAsync(int id, int tenantId);
        Task<CashPaymentDto> CreateAsync(CreateCashPaymentDto dto, int tenantId, int userId);
        Task<CashPaymentDto?> UpdateAsync(int id, UpdateCashPaymentDto dto, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        Task<int> GetNextVoucherNumberAsync(int tenantId);
    }
}
