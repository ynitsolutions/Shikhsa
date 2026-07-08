using Shikhsa.Attributes;
using Shikhsa.Services;
using System.Security.Claims;

namespace Shikhsa.Middlewares
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;

        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            PermissionService permissionService)
        {
            // =========================
            // LOGIN CHECK
            // =========================
            if (context.User.Identity != null &&
                context.User.Identity.IsAuthenticated)
            {
                var userId = context.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                var controller = context
                    .Request
                    .RouteValues["controller"]
                    ?.ToString();

                var action = context
                    .Request
                    .RouteValues["action"]
                    ?.ToString();

                // =========================
                // IGNORE ACCOUNT PAGES
                // =========================
                var ignoredControllers = new[]
                {
                    "Account",
                    "Home"
                };

                if (!string.IsNullOrWhiteSpace(controller)
                    && !ignoredControllers.Contains(controller))
                {
                    bool hasPermission =await permissionService.HasPermission(userId!,controller!,action!, permissionAttribute.PermissionType);

                    if (!hasPermission)
                    {
                        context.Response.Redirect(
                            "/Account/AccessDenied");

                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}