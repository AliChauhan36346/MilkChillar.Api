using MilkChillar.Application.DTOs.Parchi;
using MilkChillar.Application.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Application.Interfaces
{
    public interface IParchiService
    {
        Task<ParchiResultDto> GetSupplierParchiAsync(ParchiQueryParameters query);
        Task<ParchiDto?> GetSingleSupplierParchiAsync(int supplierId, DateTime startDate, DateTime endDate, int tenantId);
    }
}
