using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.UserManuals.Domain;
using Nop.Plugin.Widgets.UserManuals.Models;

namespace Nop.Plugin.Widgets.UserManuals.Services;

public partial interface IUserManualService
{
    /// <summary>
    /// Get user manuals in the configured order, optionally paged
    /// </summary>
    /// <param name="showUnpublished">True if unpublished manuals should be fetched, false otherwise</param>
    /// <param name="pageIndex">The page index, optional</param>
    /// <param name="pageSize">The page size, optional</param>
    /// <returns></returns>
    Task<IPagedList<UserManual>> GetOrderedUserManualsAsync(
        bool showUnpublished, 
        int pageIndex = 0, 
        int pageSize = int.MaxValue);

    /// <summary>
    /// Gets user manuals by manufacturer, in the configured order
    /// </summary>
    /// <param name="showUnpublished">True if unpublished manuals should be fetched, false otherwise</param>
    /// <returns></returns>
    Task<List<ManufacturerManualsModel>> GetOrderedUserManualsWithProductsAsync(bool showUnpublished);

    /// <summary>
    /// Get user manual categories in the configured order, optionally paged
    /// </summary>
    /// <param name="showUnpublished">True if unpublished categories should be fetched, false otherwise</param>
    /// <param name="pageIndex">The page index, optional</param>
    /// <param name="pageSize">The page size, optional</param>
    /// <returns></returns>
    Task<IPagedList<UserManualCategory>> GetOrderedCategoriesAsync(
        bool showUnpublished, 
        int pageIndex = 0, 
        int pageSize = int.MaxValue);

    /// <summary>
    /// Get the manual specified by <paramref name="id"/>
    /// </summary>
    /// <param name="id">The id of the user manual to fetch</param>
    /// <returns></returns>
    Task<UserManual> GetByIdAsync(int id);

    /// <summary>
    /// Get user manuals that match <paramref name="productId"/>
    /// </summary>
    /// <param name="productId">Product it to fetch manuals for</param>
    /// <returns></returns>
    Task<IList<UserManual>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Create a new user manual
    /// </summary>
    /// <param name="userManual">User manual to create</param>
    /// <returns></returns>
    Task InsertUserManualAsync(UserManual userManual);

    /// <summary>
    /// Create a new user manual category
    /// </summary>
    /// <param name="category">user manual category to create</param>
    /// <returns></returns>
    Task InsertCategoryAsync(UserManualCategory category);

    /// <summary>
    /// Update the user manual
    /// </summary>
    /// <param name="userManual">The user manual to update</param>
    /// <returns></returns>
    Task UpdateUserManualAsync(UserManual userManual);

    /// <summary>
    /// Update the user manual category
    /// </summary>
    /// <param name="category">User manual category to update</param>
    /// <returns></returns>
    Task UpdateCategoryAsync(UserManualCategory category);

    /// <summary>
    /// Delete a user manual
    /// </summary>
    /// <param name="userManual">The user manual to delete</param>
    /// <returns></returns>
    Task DeleteUserManualAsync(UserManual userManual);

    /// <summary>
    /// Delete a user manual category
    /// </summary>
    /// <param name="category">The user manual category to delete</param>
    /// <returns></returns>
    Task DeleteCategoryAsync(UserManualCategory category);

    /// <summary>
    /// Get all user manuals in the specified category <paramref name="categoryId"/>
    /// </summary>
    /// <param name="categoryId">Category id to fetch manuals for</param>
    /// <param name="showUnpublished">True if unpublished categories should be fetched, false otherwise</param>
    /// <returns></returns>
    Task<IList<UserManual>> GetUserManualsByCategoryIdAsync(int categoryId, bool showUnpublished = false);

    /// <summary>
    /// Fetch all products that have been assigned the user manual <paramref name="manualId"/>
    /// </summary>
    /// <param name="manualId">The id of the user  manual</param>
    /// <param name="showUnpublished">True if unpublished categories should be fetched, false otherwise</param>
    /// <param name="pageIndex">The page index, optional</param>
    /// <param name="pageSize">The page size, optional</param>
    /// <returns></returns>
    Task<IPagedList<(UserManualProduct userManualProduct, Product product)>> GetProductsForManualAsync(
        int manualId, 
        bool showUnpublished = false,
        int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Associate the specified product <paramref name="productId"/> with the specified <paramref name="manualId"/>
    /// </summary>
    /// <param name="manualId">Manual id</param>
    /// <param name="productId">Product id</param>
    /// <returns></returns>
    Task AddProductToManualAsync(int manualId, int productId);

    /// <summary>
    /// Dissociate the specified product <paramref name="productId"/> froom the specified <paramref name="manualId"/>
    /// </summary>
    /// <param name="manualId">Manual id</param>
    /// <param name="productId">Product id</param>
    /// <returns></returns>
    Task RemoveProductFromManualAsync(int manualId, int productId);

    /// <summary>
    /// Get the specified user manual category
    /// </summary>
    /// <param name="categoryId">Category to fetch</param>
    /// <returns></returns>
    Task<UserManualCategory> GetCategoryByIdAsync(int categoryId);
}
