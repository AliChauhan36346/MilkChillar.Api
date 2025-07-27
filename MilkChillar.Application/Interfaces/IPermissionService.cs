using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MilkChillar.Application.DTOs.Permissions;

namespace MilkChillar.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<List<PermissionDto>> GetAllAsync();
    }
}

