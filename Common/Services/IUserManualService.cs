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
#if NOP_ASYNC
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
#else
        IPagedList<UserManual> GetOrderedUserManuals(bool showUnpublished, int pageIndex = 0, int pageSize = int.MaxValue);
        List<ManufacturerManualsModel> GetOrderedUserManualsWithProducts(bool showUnpublished);

        IPagedList<UserManualCategory> GetOrderedCategories(bool showUnpublished, int pageIndex = 0, int pageSize = int.MaxValue);

        UserManual GetById(int id);
        IList<UserManual> GetByProductId(int productId);

        void InsertUserManual(UserManual userManual);
        void InsertCategory(UserManualCategory category);

        void UpdateUserManual(UserManual userManual);
        void UpdateCategory(UserManualCategory category);

        void DeleteUserManual(UserManual userManual);
        void DeleteCategory(UserManualCategory category);

        IList<UserManual> GetUserManualsByCategoryId(int categoryId, bool showUnpublished = false);

        IPagedList<(UserManualProduct userManualProduct, Product product)> GetProductsForManual(int manualId, bool showUnpublished = false, 
            int pageIndex = 0, int pageSize = int.MaxValue);

        void AddProductToManual(int manualId, int productId);
        void RemoveProductFromManual(int manualId, int productId);

        UserManualCategory GetCategoryById(int categoryId);
#endif
    }
}
