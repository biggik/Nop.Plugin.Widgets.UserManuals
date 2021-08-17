using System.Threading.Tasks;
using Nop.Plugin.Widgets.UserManuals.Models;

namespace Nop.Plugin.Widgets.UserManuals.Services
{
    public partial interface IUserManualModelFactory
    {
#if NOP_4_4
        Task<AddProductToUserManualSearchModel> PrepareAddProductToUserManualSearchModelAsync(AddProductToUserManualSearchModel searchModel);
        Task<AddProductToUserManualListModel> PrepareAddProductToUserManualListModelAsync(AddProductToUserManualSearchModel searchModel);
#else
        AddProductToUserManualSearchModel PrepareAddProductToUserManualSearchModel(AddProductToUserManualSearchModel searchModel);
        AddProductToUserManualListModel PrepareAddProductToUserManualListModel(AddProductToUserManualSearchModel searchModel);
#endif
    }
}
