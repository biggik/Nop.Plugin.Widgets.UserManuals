using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.Widgets.UserManuals.Controllers;
using Nop.Plugin.Widgets.UserManuals.Extensions;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Widgets.UserManuals;

public partial class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder builder)
    {
        RegisterPublicRoutes(builder);
        RegisterCategoryAdminRoutes(builder);
        RegisterUserManualAdminRoutes(builder);
    }

    private void RegisterPublicRoutes(IEndpointRouteBuilder builder)
    {
        string controller = UserManualsController.ControllerName;
        string namePrefix = $"Plugins.{controller}.";
        const string routePrefix = "UserManuals";

        void Build(string action, string actionSanitized, string routeAction = null)
        {
            builder.MapControllerRoute(
                    name: $"{namePrefix}{action}",
                    pattern: routeAction ?? $"{routePrefix}/{actionSanitized ?? action}",
                    defaults: new { controller, action });
        }

        // Index
        Build(nameof(UserManualsController.IndexAsync).NoAsync(), null, routePrefix); // Skip action - to get only /UserManuals
    }

    private void RegisterCategoryAdminRoutes(IEndpointRouteBuilder builder)
    {
        string controller = CategoriesController.ControllerName;
        string namePrefix = $"Plugins.{controller}.";
        const string routePrefix = "Admin/UserManualCategories";

        void Build(string action, Func<string, string> completeAction = null)
        {
            builder.MapAreaControllerRoute(
                            name: $"{namePrefix}{action}",
                            areaName: "Admin",
                            pattern: $"{routePrefix}/{completeAction?.Invoke(action) ?? action}",
                            defaults: new { controller, action, area = "Admin" });
        }

        // Create category
        Build(nameof(CategoriesController.CreateAsync).NoAsync());

        // Edit category
        Build(nameof(CategoriesController.EditAsync).NoAsync(), action => action + "/{id}");

        // Delete category
        Build(nameof(CategoriesController.DeleteAsync).NoAsync(), action => action + "/{id}");

        // List categories
        Build(nameof(CategoriesController.ListAsync).NoAsync());

        // List category data
        Build(nameof(CategoriesController.ListDataAsync).NoAsync());
    }

    private void RegisterUserManualAdminRoutes(IEndpointRouteBuilder builder)
    {
        string controller = UserManualsController.ControllerName;
        string namePrefix = $"Plugins.{controller}.";
        const string routePrefix = "Admin/UserManuals";

        void Build(string action, Func<string, string> completeAction = null)
        {
            builder.MapAreaControllerRoute(
                            name: $"{namePrefix}{action}",
                            areaName: "Admin",
                            pattern: $"{routePrefix}/{completeAction?.Invoke(action) ?? action}",
                            defaults: new { controller, action, area = "Admin" });
        }

        // Configure
        Build(nameof(UserManualsController.ConfigureAsync).NoAsync());

        // Edit user manual
        Build(nameof(UserManualsController.EditAsync).NoAsync(), action => action + "/{id}");

        // Create user manual
        Build(nameof(UserManualsController.CreateAsync).NoAsync());

        // Delete user manual
        Build(nameof(UserManualsController.DeleteAsync).NoAsync(), action => action + "/{id}");

        // List user manuals
        Build(nameof(UserManualsController.ListAsync).NoAsync());

        // List products for user manuals
        Build(nameof(UserManualsController.ListProductDataAsync).NoAsync());

        // Add products to user manuals
        Build(nameof(UserManualsController.ProductAddPopupAsync).NoAsync());

        // Add products to user manuals
        Build(nameof(UserManualsController.ProductAddPopupListAsync).NoAsync());

        // List user manual data
        Build(nameof(UserManualsController.ListDataAsync).NoAsync());
    }

    public int Priority => 1000;
}
