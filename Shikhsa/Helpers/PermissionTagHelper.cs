using Microsoft.AspNetCore.Razor.TagHelpers;
using Shikhsa.Helpers;
using Shikhsa.Services;
using Shikhsa.ViewModels;

namespace Shikhsa.Helpers
{
    [HtmlTargetElement("permission")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PermissionService _permissionService;

        public PermissionTagHelper(
            IHttpContextAccessor httpContextAccessor,
            PermissionService permissionService)
        {
            _httpContextAccessor = httpContextAccessor;
            _permissionService = permissionService;
        }

        // Usage:
        // <permission controller="Masters" type="Edit">
        public string Controller { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        //    public override async Task ProcessAsync(
        //        TagHelperContext context,
        //        TagHelperOutput output)
        //    {
        //        var currentUser =
        //            _httpContextAccessor.HttpContext?
        //            .Session
        //            .GetObject<UserSessionVM>("CurrentUser");

        //        if (currentUser == null)
        //        {
        //            output.SuppressOutput();
        //            return;
        //        }

        //        var cache =
        //             _permissionService
        //            .GetPermissions(currentUser.Id);
        //        var debugCount = cache?.Permissions?.Count ?? 0;
        //        if (cache == null)
        //        {
        //            output.SuppressOutput();
        //            return;
        //        }

        //        //var permission = cache.Permissions
        //        //    .FirstOrDefault(x =>
        //        //        x.ControllerName != null &&
        //        //        x.ControllerName.Equals(
        //        //            Controller,
        //        //            StringComparison.OrdinalIgnoreCase));
        //        var permission = cache.Permissions
        //.FirstOrDefault(x =>
        //    string.Equals(x.ControllerName?.Trim(), Controller?.Trim(), StringComparison.OrdinalIgnoreCase)
        //    &&
        //    string.Equals(x.ActionName?.Trim(), Action?.Trim(), StringComparison.OrdinalIgnoreCase)
        //);

        //        if (permission == null)
        //        {
        //            output.SuppressOutput();
        //            return;
        //        }

        //        var type = Type?.Trim().ToLowerInvariant();

        //        bool allowed = type switch
        //        {
        //            "view" => permission.CanView,
        //            "create" => permission.CanCreate,
        //            "edit" => permission.CanUpdate,
        //            "delete" => permission.CanDelete,
        //            _ => false
        //        };

        //        if (!allowed)
        //        {
        //            output.SuppressOutput();
        //        }

        //    }
        public override async Task ProcessAsync(
        TagHelperContext context,
        TagHelperOutput output)
        {
            var currentUser = _httpContextAccessor.HttpContext?
                .Session
                .GetObject<UserSessionVM>("CurrentUser");

            if (currentUser == null)
            {
                output.SuppressOutput();
                return;
            }

            var cache = _permissionService.GetPermissions(currentUser.Id);

            if (cache == null)
            {
                await _permissionService.CacheUserPermissions(currentUser.Id);
                cache = _permissionService.GetPermissions(currentUser.Id);
            }

            if (cache == null)
            {
                output.SuppressOutput();
                return;
            }

            var permission = cache.Permissions.FirstOrDefault(x =>
                string.Equals(x.ControllerName?.Trim(), Controller?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ActionName?.Trim(), Action?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (permission == null)
            {
                output.SuppressOutput();
                return;
            }

            bool allowed = Type?.Trim().ToLowerInvariant() switch
            {
                "view" => permission.CanView,
                "create" => permission.CanCreate,
                "edit" => permission.CanUpdate,
                "delete" => permission.CanDelete,
                _ => false
            };

            if (!allowed)
            {
                output.SuppressOutput();
            }
        }
    }
}
