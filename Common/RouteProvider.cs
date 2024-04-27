using Microsoft.AspNetCore.Routing;
using Nop.Plugin.Widgets.UserManuals.Controllers;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;

namespace Nop.Plugin.Widgets.UserManuals
{
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

            string action = "";
            string actionSanitized = "";

            void Build(string routeAction = null) =>
                builder.MapControllerRoute(
                        name: $"{namePrefix}{action}",
                        pattern: routeAction ?? $"{routePrefix}/{actionSanitized}",
                        defaults: new { controller = controller, action = action });

            // Index
            action = nameof(UserManualsController.Index);
            actionSanitized = action;
            Build(routePrefix); // Skip action - to get only /UserManuals
        }

        private void RegisterCategoryAdminRoutes(IEndpointRouteBuilder builder)
        {
            string controller = CategoriesController.ControllerName;
            string namePrefix = $"Plugins.{controller}.";
            const string routePrefix = "Admin/UserManualCategories";

            string action = "";
            string actionSanitized = "";

            void Build() =>
                builder.MapAreaControllerRoute(
                                name: $"{namePrefix}{action}",
                                areaName: "Admin",
                                pattern: $"{routePrefix}/{actionSanitized}",
                                defaults: new { controller = controller, action = action, area = "Admin" });

            // Create category
            action = nameof(CategoriesController.Create);
            actionSanitized = action;
            Build();

            // Edit category
            action = nameof(CategoriesController.Edit);
            actionSanitized = action + "/{id}";
            Build();

            // Delete category
            action = nameof(CategoriesController.Delete);
            actionSanitized = action + "/{id}";
            Build();

            // List categories
            action = nameof(CategoriesController.List);
            actionSanitized = action;
            Build();

            // List category data
            action = nameof(CategoriesController.ListData);
            actionSanitized = action;
            Build();
        }

        private void RegisterUserManualAdminRoutes(IEndpointRouteBuilder builder)
        {
            string controller = UserManualsController.ControllerName;
            string namePrefix = $"Plugins.{controller}.";
            const string routePrefix = "Admin/UserManuals";

            string action = "";
            string actionSanitized = "";

            void Build() =>
                builder.MapAreaControllerRoute(
                                name: $"{namePrefix}{action}",
                                areaName: "Admin",
                                pattern: $"{routePrefix}/{actionSanitized}",
                                defaults: new { controller = controller, action = action, area = "Admin" });

            // Configure
            action = nameof(UserManualsController.Configure);
            actionSanitized = action;
            Build();

            // Edit user manual
            action = nameof(UserManualsController.Edit);
            actionSanitized = action + "/{id}";
            Build();

            // Create user manual
            action = nameof(UserManualsController.Create);
            actionSanitized = action;
            Build();

            // Delete user manual
            action = nameof(UserManualsController.Delete);
            actionSanitized = action + "/{id}";
            Build();

            // List user manuals
            action = nameof(UserManualsController.List);
            actionSanitized = action;
            Build();

            // List products for user manuals
            action = nameof(UserManualsController.ListProductData);
            actionSanitized = action;
            Build();

            // Add products to user manuals
            action = nameof(UserManualsController.ProductAddPopup);
            actionSanitized = action;
            Build();

            // Add products to user manuals
            action = nameof(UserManualsController.ProductAddPopupList);
            actionSanitized = action;
            Build();

            // List user manual data
            action = nameof(UserManualsController.ListData);
            actionSanitized = action;
            Build();
        }

        public int Priority => 1000;
    }
}
