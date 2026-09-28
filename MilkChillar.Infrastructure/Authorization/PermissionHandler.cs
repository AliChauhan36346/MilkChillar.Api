// MilkChillar.Infrastructure/Authorization/PermissionHandler.cs
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MilkChillar.Infrastructure.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var user = context.User;

            // Admin and Manager roles have global authorization
            var roles = user.FindAll(ClaimTypes.Role)
                .Concat(user.FindAll("role"))
                .Select(r => r.Value)
                .ToList();

            if (roles.Any(r => string.Equals(r, "admin", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(r, "manager", StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Case-insensitive permission matching
            if (user.Claims.Any(c =>
                (c.Type == "permissions" || c.Type == "permission") &&
                string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}