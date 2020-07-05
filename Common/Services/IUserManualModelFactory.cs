using Nop.Plugin.Widgets.UserManuals.Models;

namespace Nop.Plugin.Widgets.UserManuals.Services
{
    public partial interface IUserManualModelFactory
    {
        AddProductToUserManualSearchModel PrepareAddProductToUserManualSearchModel(AddProductToUserManualSearchModel searchModel);
        AddProductToUserManualListModel PrepareAddProductToUserManualListModel(AddProductToUserManualSearchModel searchModel);
    }
}
