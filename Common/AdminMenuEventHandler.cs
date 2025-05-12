using Nop.Plugin.Widgets.UserManuals.Controllers;
using Nop.Plugin.Widgets.UserManuals.Extensions;
using Nop.Plugin.Widgets.UserManuals.Resources;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Widgets.UserManuals;

public class AdminMenuEventHandler : IConsumer<AdminMenuCreatedEvent>
{
    private readonly ILocalizationService _localizationService;

    public AdminMenuEventHandler(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var rootNode = eventMessage.RootMenuItem;

        var contentMenu = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Content Management");
        if (contentMenu == null)
        {
            // Unable to find the "Configure" menu, create our own menu container
            contentMenu = new AdminMenuItem
            {
                SystemName = "User Manuals",
                Title = UserManualResources.ListCaption,
                Visible = true,
                IconClass = "fa-cubes"
            };
            rootNode.ChildNodes.Add(contentMenu);
        }

        async Task<string> TAsync(string format)
        {
            return await _localizationService.GetResourceAsync(format) ?? format;
        }

        foreach ((string caption, string controller, string action) in new List<(string caption, string controller, string action)>
        {
            (await TAsync(UserManualResources.ListCaption), UserManualsController.ControllerName, nameof(UserManualsController.ListAsync).NoAsync()),
            (await TAsync(AdminResources.CategoryListCaption), CategoriesController.ControllerName, nameof(CategoriesController.ListAsync).NoAsync()),
        })
        {
            contentMenu.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = $"{controller}.{action}",
                Title = caption,
                Url = $"/Admin/{controller}/{action}",
                Visible = true,
                IconClass = "fa-dot-circle-o"
            });
        }
    }
}
