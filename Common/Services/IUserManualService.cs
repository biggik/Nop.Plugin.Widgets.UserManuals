using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.UserManuals.Domain;
using Nop.Plugin.Widgets.UserManuals.Models;

namespace Nop.Plugin.Widgets.UserManuals.Services
{
    public partial interface IUserManualService
    {
        Task<IPagedList<UserManual>> GetOrderedUserManualsAsync(bool showUnpublished, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<List<ManufacturerManualsModel>> GetOrderedUserManualsWithProductsAsync(bool showUnpublished);

        Task<IPagedList<UserManualCategory>> GetOrderedCategoriesAsync(bool showUnpublished, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<UserManual> GetByIdAsync(int id);
        Task<IList<UserManual>> GetByProductIdAsync(int productId);

        Task InsertUserManualAsync(UserManual userManual);
        Task InsertCategoryAsync(UserManualCategory category);

        Task UpdateUserManualAsync(UserManual userManual);
        Task UpdateCategoryAsync(UserManualCategory category);

        Task DeleteUserManualAsync(UserManual userManual);
        Task DeleteCategoryAsync(UserManualCategory category);

        Task<IList<UserManual>> GetUserManualsByCategoryIdAsync(int categoryId, bool showUnpublished = false);

        Task<IPagedList<(UserManualProduct userManualProduct, Product product)>> GetProductsForManualAsync(int manualId, bool showUnpublished = false,
            int pageIndex = 0, int pageSize = int.MaxValue);

        Task AddProductToManualAsync(int manualId, int productId);
        Task RemoveProductFromManualAsync(int manualId, int productId);

        Task<UserManualCategory> GetCategoryByIdAsync(int categoryId);
    }
}
