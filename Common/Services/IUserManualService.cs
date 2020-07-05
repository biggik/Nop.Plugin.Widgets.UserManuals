using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.UserManuals.Domain;

namespace Nop.Plugin.Widgets.UserManuals.Services
{
    public partial interface IUserManualService
    {
        IPagedList<UserManual> GetOrderedUserManuals(bool showUnpublished, int pageIndex = 0, int pageSize = int.MaxValue);
        IPagedList<UserManualCategory> GetOrderedCategories(bool showUnpublished, int pageIndex = 0, int pageSize = int.MaxValue);

        UserManual GetById(int id);
        IEnumerable<UserManual> GetByProductId(int productId);

        void InsertUserManual(UserManual userManual);
        void InsertCategory(UserManualCategory category);

        void UpdateUserManual(UserManual userManual);
        void UpdateCategory(UserManualCategory category);

        void DeleteUserManual(UserManual userManual);
        void DeleteCategory(UserManualCategory category);

        IEnumerable<UserManual> GetUserManualsByCategoryId(int categoryId, bool showUnpublished = false);

        IPagedList<(UserManualProduct userManualProduct, Product product)> GetProductsForManual(int manualId, bool showUnpublished = false, 
            int pageIndex = 0, int pageSize = int.MaxValue);

        void AddProductToManual(int manualId, int productId);
        void RemoveProductFromManual(int manualId, int productId);

        UserManualCategory GetCategoryById(int categoryId);
    }
}
