using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Widgets.UserManuals.Components;
using Nop.Plugin.Widgets.UserManuals.Controllers;
using Nop.Plugin.Widgets.UserManuals.Extensions;
using Nop.Plugin.Widgets.UserManuals.Resources;
using Nop.Plugin.Widgets.UserManuals.Security;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using nopLocalizationHelper;

namespace Nop.Plugin.Widgets.UserManuals;

public class UserManualPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
{
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly IPermissionService _permissionService;
    private readonly IStoreContext _storeContext;

    public bool HideInWidgetList => false;

    public UserManualPlugin(
        IWebHelper webHelper,
        ILocalizationService localizationService,
        ILanguageService languageService,
        IPermissionService permissionService,
        IStoreContext storeContext,
        ISettingService settingService)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
        _languageService = languageService;
        _permissionService = permissionService;
        _storeContext = storeContext;
        _settingService = settingService;
    }

#if DEBUG
    private static bool _debugInitialized = false;

    private async Task DebugInitializeAsync()
    {
        if (_debugInitialized)
        {
            return;
        }

        _debugInitialized = true;

        var resourceHelper = await CreateResourceHelperAsync();
        await resourceHelper.CreateLocaleStringsAsync();
        await _permissionService.InstallPermissionsAsync(new UserManualPermissionProvider());
    }
#endif

    private async Task<LocaleStringHelper<LocaleStringResource>> CreateResourceHelperAsync()
    {
        return new LocaleStringHelper<LocaleStringResource>
        (
            pluginAssembly: GetType().Assembly,
            languageCultures: from lang in await _languageService.GetAllLanguagesAsync() select (lang.Id, lang.LanguageCulture),
            getResource: (resourceName, languageId) => _localizationService.GetLocaleStringResourceByNameAsync(resourceName, languageId, false),
            createResource: (languageId, resourceName, resourceValue) => new LocaleStringResource { LanguageId = languageId, ResourceName = resourceName, ResourceValue = resourceValue },
            insertResource: _localizationService.InsertLocaleStringResourceAsync,
            updateResource: (lsr, resourceValue) => { lsr.ResourceValue = resourceValue; return _localizationService.UpdateLocaleStringResourceAsync(lsr); },
            deleteResource: _localizationService.DeleteLocaleStringResourceAsync,
            areResourcesEqual: (lsr, resourceValue) => lsr.ResourceValue == resourceValue
        );
    }

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>Widget zones</returns>
    public async Task<IList<string>> GetWidgetZonesAsync()
    {
#if DEBUG
        await DebugInitializeAsync();
#endif
        if (_widgetZones == null)
        {
            int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<UserManualsWidgetSettings>(storeScope);

            _widgetZones = string.IsNullOrWhiteSpace(settings.WidgetZones)
                ? []
                : await settings.WidgetZones.Split(';').ToListAsync();
        }

        return _widgetZones;
    }

    private List<string> _widgetZones;

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/{UserManualsController.ControllerName}/Configure";
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new UserManualsWidgetSettings
        {
            WidgetZones = "header_menu_after;productdetails_overview_bottom"
        });

        var resourceHelper = await CreateResourceHelperAsync();
        await resourceHelper.CreateLocaleStringsAsync();
        await _permissionService.InstallPermissionsAsync(new UserManualPermissionProvider());

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<UserManualsWidgetSettings>();

        var resourceHelper = await CreateResourceHelperAsync();
        await resourceHelper.DeleteLocaleStringsAsync();

        await _permissionService.UninstallPermissionsAsync(new UserManualPermissionProvider());

        await base.UninstallAsync();
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return widgetZone.StartsWith("productdetails", StringComparison.CurrentCultureIgnoreCase)
            ? typeof(WidgetsProductUserManualsViewComponent)
            : typeof(WidgetsUserManualsViewComponent);
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var contentMenu = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Content Management");
        if (contentMenu == null)
        {
            // Unable to find the "Configure" menu, create our own menu container
            contentMenu = new SiteMapNode
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
            contentMenu.ChildNodes.Add(new SiteMapNode
            {
                SystemName = $"{controller}.{action}",
                Title = caption,
                ControllerName = controller,
                ActionName = action,
                Visible = true,
                IconClass = "fa-dot-circle-o",
                RouteValues = new RouteValueDictionary {
                    ["area"] = "Admin"
                },
            });
        }
    }
}
