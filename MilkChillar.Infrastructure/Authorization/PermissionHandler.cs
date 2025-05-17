// MilkChillar.Infrastructure/Authorization/PermissionHandler.cs
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace MilkChillar.Infrastructure.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.HasClaim(c =>
                c.Type == "permissions" &&
                c.Value == requirement.Permission))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}