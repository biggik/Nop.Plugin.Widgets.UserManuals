using Nop.Plugin.Widgets.UserManuals.Models;

namespace Nop.Plugin.Widgets.UserManuals.Services;

public partial interface IUserManualModelFactory
{
    Task<AddProductToUserManualSearchModel> PrepareAddProductToUserManualSearchModelAsync(AddProductToUserManualSearchModel searchModel);
    Task<AddProductToUserManualListModel> PrepareAddProductToUserManualListModelAsync(AddProductToUserManualSearchModel searchModel);
}
