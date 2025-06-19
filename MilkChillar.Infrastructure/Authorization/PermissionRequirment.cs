// MilkChillar.Infrastructure/Authorization/PermissionRequirement.cs

using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;

namespace MilkChillar.Infrastructure.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
