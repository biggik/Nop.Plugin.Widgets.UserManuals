using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Seo;
using Nop.Data;
using Nop.Plugin.Widgets.UserManuals.Domain;
using Nop.Plugin.Widgets.UserManuals.Models;

namespace Nop.Plugin.Widgets.UserManuals.Services;

public partial class UserManualService : IUserManualService
{
    #region Constants
    private const string _prefix = "Nop.status.userManual.";
    private static readonly string _allKey = _prefix + "all-{0}-{1}-{2}";
    private static readonly string _uiKey = _prefix + "ui-{0}";
    private static readonly string _usermanualCategoryKey = _prefix + "usermanualcategory.all-{0}-{1}";
    private static readonly string _productsKey = _prefix + "usermanualproducts.all-{0}-{1}-{2}-{3}";
    private static readonly string _categoriesKey = _prefix + "usermanualcategories.all-{0}-{1}-{2}";
    private static readonly string _productKey = _prefix + "usermanualproduct-{0}";
    private static readonly string _categoryKey = _prefix + "usermanualcategory-{0}";

    private readonly CacheKey UserManualsAllKey = new(_allKey, _prefix);
    private readonly CacheKey UserManualsUIKey = new(_uiKey, _prefix);
    private readonly CacheKey UserManualsCategoryKey = new(_usermanualCategoryKey, _prefix);
    private readonly CacheKey CategoriesKey = new(_categoriesKey, _prefix);
    private readonly CacheKey UserManualProductsKey = new(_productsKey, _prefix);
    private readonly CacheKey ProductKey = new(_productKey, _prefix);
    private readonly CacheKey CategoryKey = new(_categoryKey, _prefix);
    #endregion

    #region Fields
    private readonly IRepository<UserManual> _userManualRepository;
    private readonly IRepository<UserManualProduct> _userManualProductRepository;
    private readonly IRepository<UserManualCategory> _categoryRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<UrlRecord> _urlRecordRepository;
    private readonly IRepository<Manufacturer> _manufacturerRepository;
    private readonly IStaticCacheManager _cacheManager;
    #endregion

    #region Ctor
    public UserManualService(
        IStaticCacheManager cacheManager,
        IRepository<Manufacturer> manufacturerRepository,
        IRepository<UserManual> userManualRepository,
        IRepository<UserManualProduct> userManualProductRepository,
        IRepository<UserManualCategory> categoryRespository,
        IRepository<Product> productRepository,
        IRepository<UrlRecord> urlRecordRepository)
    {
        _cacheManager = cacheManager;
        _manufacturerRepository = manufacturerRepository;
        _userManualRepository = userManualRepository;
        _userManualProductRepository = userManualProductRepository;
        _categoryRepository = categoryRespository;
        _productRepository = productRepository;
        _urlRecordRepository = urlRecordRepository;
    }
    #endregion

    private IStaticCacheManager CacheImpl => _cacheManager;

    private CacheKey CreateKey(CacheKey cacheKey, params object[] arguments)
        => CacheImpl.PrepareKeyForDefaultCache(cacheKey, arguments);

    #region Methods
    public virtual async Task<IPagedList<UserManual>> GetOrderedUserManualsAsync(
        bool showUnpublished, 
        int pageIndex = 0, 
        int pageSize = int.MaxValue)
    {
        CacheKey key = CreateKey(UserManualsAllKey, showUnpublished, pageIndex, pageSize);
        return await _cacheManager.GetAsync(key, async () =>
        {
            var categoryDict = (from category in _categoryRepository.Table
                                where showUnpublished || category.Published
                                select category
                               ).ToDictionary(x => x.Id, x => x);
            var manufacturerDict = (from manufacturer in _manufacturerRepository.Table
                                    where showUnpublished || manufacturer.Published
                                    select manufacturer
                                   ).ToDictionary(x => x.Id, x => x);

            var userManuals = await (from userManual in _userManualRepository.Table
                                     where showUnpublished || userManual.Published
                                     select userManual
                                    ).ToListAsync();

            var list = (from userManual in userManuals
                        let manufacturer = manufacturerDict.TryGetValue(userManual.ManufacturerId, out var mv) ? mv : null
                        let mOrder = manufacturer?.DisplayOrder ?? int.MaxValue
                        let mName = manufacturer?.Name ?? ""

                        let category = categoryDict.TryGetValue(userManual.CategoryId, out var cv) ? cv : null
                        let cOrder = category?.DisplayOrder ?? int.MaxValue
                        let cName = category?.Name ?? ""

                        select (mId: mOrder, mName, cId: cOrder, cName, userManual)
              )
              .OrderBy(x => x.mId)
              .ThenBy(x => x.mName)
              .ThenBy(x => x.cId)
              .ThenBy(x => x.cName)
              .ThenBy(x => x.userManual.DisplayOrder)
              .ThenBy(x => x.userManual.Description)
              ;

            return new PagedList<UserManual>(
                await (from m in list select m.userManual).ToListAsync(),
                pageIndex,
                pageSize);
        });
    }

    public virtual Task<List<ManufacturerManualsModel>> GetOrderedUserManualsWithProductsAsync(bool showUnpublished)
    {
        var key = CreateKey(UserManualsUIKey, showUnpublished);
        return _cacheManager.GetAsync(key, () =>
        {
            return OrderedUserManualsWithProductsCommon(_categoryRepository.Table, showUnpublished);
        });
    }

    private List<ManufacturerManualsModel> OrderedUserManualsWithProductsCommon(IQueryable<UserManualCategory> query, bool showUnpublished)
    {
        var categoryDict = (from category in query
                            where showUnpublished || category.Published
                            select category
                           ).ToDictionary(x => x.Id, x => x);
        var manufacturerDict = (from manufacturer in _manufacturerRepository.Table
                                where showUnpublished || manufacturer.Published
                                select manufacturer
                               ).ToDictionary(x => x.Id, x => x);
        var manualProducts = (from manualProduct in _userManualProductRepository.Table
                              select manualProduct
                             ).ToList();
        var productIds = manualProducts.Select(x => x.ProductId).Distinct().ToDictionary(x => x, x => x);

        var productInfoDict = (from product in _productRepository.Table
                               where productIds.ContainsKey(product.Id)
                               join urlRecord in _urlRecordRepository.Table on
                                      new { EntityId = product.Id, EntityName = "Product", IsActive = true }
                                         equals
                                      new { urlRecord.EntityId, urlRecord.EntityName, urlRecord.IsActive }
                               select new
                               {
                                   ProductId = product.Id,
                                   ProductPublished = product.Published,
                                   ProductDeleted = product.Deleted,
                                   Slug = urlRecord.Slug
                               }
                              )
                              .ToDictionary(x => x.ProductId, x => x);
        var manualProductDict = (from manualProduct in manualProducts
                                 let pinfo = productInfoDict.TryGetValue(manualProduct.ProductId, out var value) 
                                    ? value 
                                    : null
                                 select new
                                 {
                                     UserManualId = manualProduct.UserManualId,
                                     ProductId = pinfo?.ProductId ?? 0,
                                     PublishedProduct = pinfo?.ProductPublished ?? false,
                                     ProductDeleted = pinfo?.ProductDeleted ?? false,
                                     Slug = pinfo?.Slug ?? null
                                 }
                                 )
                                 .Where(x => !x.ProductDeleted)
                                 .GroupBy(x => x.UserManualId)
                                 .ToDictionary(
                                    g => g.Key,
                                    g => (from x in g select (x.ProductId, x.PublishedProduct, x.Slug)).ToList()
                                );

        IEnumerable<(UserManualModel userManual, bool publishedProduct)> CrossJoinWithProducts()
        {
            var userManuals = from userManual in _userManualRepository.Table
                              where showUnpublished || userManual.Published
                              select userManual
                              ;
            foreach (UserManual userManual in userManuals)
            {
                var products = manualProductDict.TryGetValue(userManual.Id, out var value) ? value : null;
                if (products == null)
                {
                    yield return (userManual.ToModel(), false);
                }
                else
                {
                    foreach ((int ProductId, bool PublishedProduct, string Slug) in products)
                    {
                        var umm = userManual.ToModel();
                        umm.ProductSlug = Slug;
                        yield return (umm, PublishedProduct);
                    }
                }
            }
        }

        var manualList = (from data in CrossJoinWithProducts()
                          let manufacturer = manufacturerDict.TryGetValue(data.userManual.ManufacturerId, out var mv) ? mv : null
                          let mOrder = manufacturer?.DisplayOrder ?? int.MaxValue
                          let mName = manufacturer?.Name ?? ""
                          
                          let category = categoryDict.TryGetValue(data.userManual.CategoryId, out var cv) ? cv : null
                          let cOrder = category?.DisplayOrder ?? int.MaxValue
                          let cName = category?.Name ?? ""
                          
                          select (mId: mOrder, mName, cId: cOrder, cName, data)
                        )
                   .OrderBy(x => x.mId)
                   .ThenBy(x => x.mName)
                   .ThenBy(x => x.cId)
                   .ThenBy(x => x.cName)
                   .ThenBy(x => x.data.userManual.DisplayOrder)
                   .ThenBy(x => x.data.userManual.Description)
                   ;

        List<ManufacturerManualsModel> list = [];
        string lastManufacturer = "";
        string lastCategoryName = "";
        ManufacturerManualsModel manufacturerModel = null;
        CategoryUserManualModel categoryModel = null;

        foreach ((UserManualModel userManual, bool publishedProduct) in manualList.Select(x => x.data))
        {
            string manufacturerName = manufacturerDict.TryGetValue(userManual.ManufacturerId, out var mv) 
                                        ? mv.Name
                                        : "";
            if (manufacturerName != lastManufacturer)
            {
                manufacturerModel = new ManufacturerManualsModel(manufacturerName);
                list.Add(manufacturerModel);
                categoryModel = null;
                lastManufacturer = manufacturerName;
            }

            string categoryName = categoryDict.TryGetValue(userManual.CategoryId, out var cv) 
                                    ? cv.Name
                                    : "";
            if (categoryName != lastCategoryName || categoryModel == null)
            {
                categoryModel = new CategoryUserManualModel(new CategoryModel { Name = categoryName });
                manufacturerModel.Categories.Add(categoryModel);
                lastCategoryName = categoryName;
            }

            if (!string.IsNullOrEmpty(userManual.ProductSlug) && publishedProduct)
            {
                categoryModel.UserManualsForActiveProducts.Add(userManual);
            }
            else
            {
                categoryModel.UserManualsForDiscontinuedProducts.Add(userManual);
            }
        }

        return list;
    }

    public virtual async Task<IPagedList<UserManualCategory>> GetOrderedCategoriesAsync(
        bool showUnpublished, 
        int pageIndex = 0, 
        int pageSize = int.MaxValue)
    {
        var key = CreateKey(CategoriesKey, showUnpublished, pageIndex, pageSize);
        return await _cacheManager.GetAsync(key, async () =>
        {
            var list = _categoryRepository.Table
                    .Where(c => c.Published || showUnpublished)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name);

            return new PagedList<UserManualCategory>(
                await list.ToListAsync(),
                pageIndex,
                pageSize);
        });
    }

    public virtual async Task<UserManual> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _userManualRepository.GetByIdAsync(id);
    }

    public virtual async Task<IList<UserManual>> GetByProductIdAsync(int productId)
    {
        var  key = CreateKey(ProductKey, productId);
        return await _cacheManager.GetAsync(key, async () =>
        {
            var m = from userManualProduct in _userManualProductRepository.Table
                    where userManualProduct.ProductId == productId
                    join p_userManual in _userManualRepository.Table on
                         userManualProduct.UserManualId equals p_userManual.Id into pj_userManual
                    from userManual in pj_userManual.DefaultIfEmpty()
                    where userManual.Published
                    select userManual
                   ;
            return await m.ToListAsync();
        });
    }

    public virtual async Task InsertUserManualAsync(UserManual userManual)
    {
        ArgumentNullException.ThrowIfNull(userManual);

        await _userManualRepository.InsertAsync(userManual);
        await _cacheManager.RemoveByPrefixAsync(_prefix);
    }

    public virtual async Task InsertCategoryAsync(UserManualCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        await _categoryRepository.InsertAsync(category);
        await _cacheManager.RemoveByPrefixAsync(_prefix);
    }

    public virtual async Task UpdateUserManualAsync(UserManual userManual)
    {
        ArgumentNullException.ThrowIfNull(userManual);

        await _userManualRepository.UpdateAsync(userManual);
        await _cacheManager.RemoveByPrefixAsync(_prefix);
    }

    public virtual async Task UpdateCategoryAsync(UserManualCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        await _categoryRepository.UpdateAsync(category);
        await _cacheManager.RemoveByPrefixAsync(_prefix);
    }

    public virtual async Task DeleteUserManualAsync(UserManual userManual)
    {
        ArgumentNullException.ThrowIfNull(userManual);

        await _userManualRepository.DeleteAsync(userManual);
        await _cacheManager.RemoveByPrefixAsync(_prefix);
    }

    public virtual async Task DeleteCategoryAsync(UserManualCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        await _categoryRepository.DeleteAsync(category);
        await _cacheManager.RemoveByPrefixAsync(_prefix);
    }

    public virtual async Task<IList<UserManual>> GetUserManualsByCategoryIdAsync(int categoryId, bool showUnpublished = false)
    {
        var key = CreateKey(UserManualsCategoryKey, categoryId, showUnpublished);
        return await _cacheManager.GetAsync(key, async () =>
        {
            var m = from userManual in _userManualRepository.Table
                    where userManual.CategoryId == categoryId
                          && (showUnpublished
                              ||
                              userManual.Published)
                    orderby userManual.DisplayOrder, userManual.Description
                    select userManual;

            return await m.ToListAsync();
        });
    }

    public async Task<IPagedList<(UserManualProduct userManualProduct, Product product)>> GetProductsForManualAsync(
        int manualId, 
        bool showUnpublished = false, 
        int pageIndex = 0, 
        int pageSize = int.MaxValue)
    {
        var key = CreateKey(UserManualProductsKey, manualId, showUnpublished, pageIndex, pageSize);
        return await _cacheManager.GetAsync(key, async () =>
        {
            var manuProducts = await (from userManualProduct in _userManualProductRepository.Table
                                      where userManualProduct.UserManualId == manualId
                                      select userManualProduct)
                                .ToListAsync();

            var productIds = (from p in manuProducts select p.ProductId).Distinct().ToDictionary(x => x, x => x);

            var visibleProducts = (from product in _productRepository.Table
                                   where (productIds.ContainsKey(product.Id)
                                   && showUnpublished)
                                   ||
                                   (product.Published && !product.Deleted)
                                   select product).ToDictionary(x => x.Id, x => x);

            return new PagedList<(UserManualProduct userManualProduct, Product product)>(
                   await (from manuProduct in manuProducts
                          where visibleProducts.ContainsKey(manuProduct.ProductId)
                          select (manuProduct, visibleProducts[manuProduct.ProductId])).ToListAsync(),
                   pageIndex,
                   pageSize);
        });
    }

    public async Task AddProductToManualAsync(int manualId, int productId)
    {
        var manuProducts = from userManualProduct in _userManualProductRepository.Table
                           where userManualProduct.UserManualId == manualId
                                 && userManualProduct.ProductId == productId
                           select userManualProduct;

        if (!await manuProducts.AnyAsync())
        {
            await _userManualProductRepository.InsertAsync(
                new UserManualProduct
                {
                    UserManualId = manualId,
                    ProductId = productId
                });

            await _cacheManager.RemoveByPrefixAsync(_prefix);
        }
    }

    public async Task RemoveProductFromManualAsync(int manualId, int productId)
    {
        var manuProduct = await (from userManualProduct in _userManualProductRepository.Table
                                 where userManualProduct.UserManualId == manualId
                                       && userManualProduct.ProductId == productId
                                 select userManualProduct
                          ).FirstOrDefaultAsync();

        if (manuProduct != null)
        {
            await _userManualProductRepository.DeleteAsync(manuProduct);

            await _cacheManager.RemoveByPrefixAsync(_prefix);
        }
    }

    public virtual Task<UserManualCategory> GetCategoryByIdAsync(int categoryId)
    {
        var key = CreateKey(CategoryKey, categoryId);
        return _cacheManager.GetAsync(key, async () =>
        {
            return await (from category in _categoryRepository.Table
                          where category.Id == categoryId
                          select category)
                .FirstOrDefaultAsync();
        });
    }
    #endregion
}
