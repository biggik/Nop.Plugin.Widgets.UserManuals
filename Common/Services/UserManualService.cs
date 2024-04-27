using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.UserManuals.Domain;
using System.Security.Cryptography;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Core.Domain.Seo;
using NUglify.Helpers;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Services.Caching;

namespace Nop.Plugin.Widgets.UserManuals.Services
{
    public partial class UserManualService : IUserManualService
    {
        #region Constants
        private const string _prefix = "Nop.status.userManual.";
        private readonly static string _allKey = _prefix + "all-{0}-{1}-{2}";
        private readonly static string _uiKey = _prefix + "ui-{0}";
        private readonly static string _usermanualCategoryKey = _prefix + "usermanualcategory.all-{0}-{1}";
        private readonly static string _productsKey = _prefix + "usermanualproducts.all-{0}-{1}-{2}-{3}";
        private readonly static string _categoriesKey = _prefix + "usermanualcategories.all-{0}-{1}-{2}";
        private readonly static string _productKey = _prefix + "usermanualproduct-{0}";
        private readonly static string _categoryKey = _prefix + "usermanualcategory-{0}";

        private readonly CacheKey UserManualsAllKey = new CacheKey(_allKey, _prefix);
        private readonly CacheKey UserManualsUIKey = new CacheKey(_uiKey, _prefix);
        private readonly CacheKey UserManualsCategoryKey = new CacheKey(_usermanualCategoryKey, _prefix);
        private readonly CacheKey CategoriesKey = new CacheKey(_categoriesKey, _prefix);
        private readonly CacheKey UserManualProductsKey = new CacheKey(_productsKey, _prefix);
        private readonly CacheKey ProductKey = new CacheKey(_productKey, _prefix);
        private readonly CacheKey CategoryKey = new CacheKey(_categoryKey, _prefix);
        #endregion

        #region Fields
        private readonly IRepository<UserManual> _userManualRepository;
        private readonly IRepository<UserManualProduct> _userManualProductRepository;
        private readonly IRepository<UserManualCategory> _categoryRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<Nop.Core.Domain.Catalog.Manufacturer> _manufacturerRepository;
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
        {
            return CacheImpl.PrepareKeyForDefaultCache(cacheKey, arguments);
        }

        #region Methods
        public async virtual Task<IPagedList<UserManual>> GetOrderedUserManualsAsync
            (bool showUnpublished, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var key = CreateKey(UserManualsAllKey, showUnpublished, pageIndex, pageSize);
            return await _cacheManager.GetAsync(key, () =>
            {
                var categoryDict = (from category in _categoryRepository.Table
                                    where showUnpublished || category.Published
                                    select category
                                   ).ToDictionary(x => x.Id, x => x);
                var manufacturerDict = (from manufacturer in _manufacturerRepository.Table
                                        where showUnpublished || manufacturer.Published
                                        select manufacturer
                                       ).ToDictionary(x => x.Id, x => x);

                var userManuals = (from userManual in _userManualRepository.Table
                                   where showUnpublished || userManual.Published
                                   select userManual
                                  ).ToList();

                var list = (from userManual in userManuals
                            let mOrder = manufacturerDict.ContainsKey(userManual.ManufacturerId) ? manufacturerDict[userManual.ManufacturerId].DisplayOrder : int.MaxValue
                            let mName = manufacturerDict.ContainsKey(userManual.ManufacturerId) ? manufacturerDict[userManual.ManufacturerId].Name : ""

                            let cOrder = categoryDict.ContainsKey(userManual.CategoryId) ? categoryDict[userManual.CategoryId].DisplayOrder : int.MaxValue
                            let cName = categoryDict.ContainsKey(userManual.CategoryId) ? categoryDict[userManual.CategoryId].Name : ""

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
                    (from m in list select m.userManual).ToList(),
                    pageIndex,
                    pageSize);
            });
        }

        public virtual async Task<List<ManufacturerManualsModel>> GetOrderedUserManualsWithProductsAsync
            (bool showUnpublished)
        {
            var key = CreateKey(UserManualsUIKey, showUnpublished);
            return await _cacheManager.GetAsync(key, () =>
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
                                     let pinfo = productInfoDict.ContainsKey(manualProduct.ProductId)
                                                 ? productInfoDict[manualProduct.ProductId]
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
                var userManuals = (from userManual in _userManualRepository.Table
                                   where showUnpublished || userManual.Published
                                   select userManual
                                  );
                foreach (var userManual in userManuals)
                {
                    var products = manualProductDict.ContainsKey(userManual.Id) ? manualProductDict[userManual.Id] : null;
                    if (products == null)
                    {
                        yield return (userManual.ToModel(), false);
                    }
                    else
                    {
                        foreach (var product in products)
                        {
                            var umm = userManual.ToModel();
                            umm.ProductSlug = product.Slug;
                            yield return (umm, product.PublishedProduct);
                        }
                    }
                }
            }

            var manualList
                      = (from data in CrossJoinWithProducts()
                         let mOrder = manufacturerDict.ContainsKey(data.userManual.ManufacturerId)
                                          ? manufacturerDict[data.userManual.ManufacturerId].DisplayOrder
                                          : int.MaxValue
                         let mName = manufacturerDict.ContainsKey(data.userManual.ManufacturerId)
                                         ? manufacturerDict[data.userManual.ManufacturerId].Name
                                         : ""

                         let cOrder = categoryDict.ContainsKey(data.userManual.CategoryId)
                                          ? categoryDict[data.userManual.CategoryId].DisplayOrder
                                          : int.MaxValue
                         let cName = categoryDict.ContainsKey(data.userManual.CategoryId)
                                         ? categoryDict[data.userManual.CategoryId].Name
                                         : ""

                         select (mId: mOrder, mName, cId: cOrder, cName, data)
                       )
                       .OrderBy(x => x.mId)
                       .ThenBy(x => x.mName)
                       .ThenBy(x => x.cId)
                       .ThenBy(x => x.cName)
                       .ThenBy(x => x.data.userManual.DisplayOrder)
                       .ThenBy(x => x.data.userManual.Description)
                       ;

            var list = new List<ManufacturerManualsModel>();
            string lastManufacturer = "";
            string lastCategoryName = "";
            ManufacturerManualsModel manufacturerModel = null;
            CategoryUserManualModel categoryModel = null;

            foreach (var data in manualList.Select(x => x.data))
            {
                var manufacturerName = manufacturerDict.ContainsKey(data.userManual.ManufacturerId)
                                       ? manufacturerDict[data.userManual.ManufacturerId].Name
                                       : "";
                if (manufacturerName != lastManufacturer)
                {
                    manufacturerModel = new ManufacturerManualsModel(manufacturerName);
                    list.Add(manufacturerModel);
                    categoryModel = null;
                    lastManufacturer = manufacturerName;
                }

                var categoryName = categoryDict.ContainsKey(data.userManual.CategoryId)
                                   ? categoryDict[data.userManual.CategoryId].Name
                                   : "";
                if (categoryName != lastCategoryName || categoryModel == null)
                {
                    categoryModel = new CategoryUserManualModel(new CategoryModel { Name = categoryName });
                    manufacturerModel.Categories.Add(categoryModel);
                    lastCategoryName = categoryName;
                }

                if (!string.IsNullOrEmpty(data.userManual.ProductSlug) && data.publishedProduct)
                {
                    categoryModel.UserManualsForActiveProducts.Add(data.userManual);
                }
                else
                {
                    categoryModel.UserManualsForDiscontinuedProducts.Add(data.userManual);
                }
            }

            return list;
        }

        public async virtual Task<IPagedList<UserManualCategory>> GetOrderedCategoriesAsync
            (bool showUnpublished, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var key = CreateKey(CategoriesKey, showUnpublished, pageIndex, pageSize);
            return await _cacheManager.GetAsync(key, () =>
            {
                var list = _categoryRepository.Table
                        .Where(c => c.Published || showUnpublished)
                        .OrderBy(c => c.DisplayOrder)
                        .ThenBy(c => c.Name);

                return new PagedList<UserManualCategory>(
                    list.ToList(),
                    pageIndex,
                    pageSize);
            });
        }

        public async virtual Task<UserManual> GetByIdAsync(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return await _userManualRepository.GetByIdAsync(id);
        }

        public async virtual Task<IList<UserManual>> GetByProductIdAsync(int productId)
        {
            var key = CreateKey(ProductKey, productId);
            return await _cacheManager.GetAsync(key, () =>
            {
                var m = from userManualProduct in _userManualProductRepository.Table
                        where userManualProduct.ProductId == productId
                        join p_userManual in _userManualRepository.Table on
                             userManualProduct.UserManualId equals p_userManual.Id into pj_userManual
                        from userManual in pj_userManual.DefaultIfEmpty()
                        where userManual.Published
                        select userManual
                       ;
                return m.ToList();
            });
        }

        public async virtual Task InsertUserManualAsync(UserManual userManual)
        {
            if (userManual == null)
            {
                throw new ArgumentNullException(nameof(userManual));
            }

            await _userManualRepository.InsertAsync(userManual);

            await _cacheManager.RemoveByPrefixAsync(_prefix);
        }

        public async virtual Task InsertCategoryAsync(UserManualCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }
            await _categoryRepository.InsertAsync(category);

            await _cacheManager.RemoveByPrefixAsync(_prefix);
        }

        public async virtual Task UpdateUserManualAsync(UserManual userManual)
        {
            if (userManual == null)
            {
                throw new ArgumentNullException(nameof(userManual));
            }

            await _userManualRepository.UpdateAsync(userManual);

            await _cacheManager.RemoveByPrefixAsync(_prefix);
        }

        public async virtual Task UpdateCategoryAsync(UserManualCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            await _categoryRepository.UpdateAsync(category);

            await _cacheManager.RemoveByPrefixAsync(_prefix);
        }

        public async virtual Task DeleteUserManualAsync(UserManual userManual)
        {
            if (userManual == null)
            {
                throw new ArgumentNullException(nameof(userManual));
            }
            await _userManualRepository.DeleteAsync(userManual);

            await _cacheManager.RemoveByPrefixAsync(_prefix);
        }

        public async virtual Task DeleteCategoryAsync(UserManualCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }
            await _categoryRepository.DeleteAsync(category);

            await _cacheManager.RemoveByPrefixAsync(_prefix);
        }

        public async virtual Task<IList<UserManual>> GetUserManualsByCategoryIdAsync(int categoryId, bool showUnpublished = false)
        {
            var key = CreateKey(UserManualsCategoryKey, categoryId, showUnpublished);
            return await _cacheManager.GetAsync(key, () =>
            {
                var m = from userManual in _userManualRepository.Table
                        where userManual.CategoryId == categoryId
                              && (showUnpublished
                                  ||
                                  userManual.Published)

                        orderby userManual.DisplayOrder, userManual.Description

                        select userManual;

                return m.ToList();
            });
        }

        public async Task<IPagedList<(UserManualProduct userManualProduct, Product product)>> GetProductsForManualAsync
        (int manualId, bool showUnpublished = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var key = CreateKey(UserManualProductsKey, manualId, showUnpublished, pageIndex, pageSize);
            return await _cacheManager.GetAsync(key, () =>
            {
                var manuProducts = (from userManualProduct in _userManualProductRepository.Table
                                    where userManualProduct.UserManualId == manualId
                                    select userManualProduct)
                                    .ToList();

                var productIds = (from p in manuProducts select p.ProductId).Distinct().ToDictionary(x => x, x => x);

                var visibleProducts = (from product in _productRepository.Table
                                       where productIds.ContainsKey(product.Id)
                                       && showUnpublished
                                       ||
                                       (product.Published && !product.Deleted)
                                       select product).ToDictionary(x => x.Id, x => x);

                return new PagedList<(UserManualProduct userManualProduct, Product product)>(
                       (from manuProduct in manuProducts
                        where visibleProducts.ContainsKey(manuProduct.ProductId)
                        select (manuProduct, visibleProducts[manuProduct.ProductId])).ToList(),
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

            if (!manuProducts.Any())
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
            var manuProduct = (from userManualProduct in _userManualProductRepository.Table
                               where userManualProduct.UserManualId == manualId
                                     && userManualProduct.ProductId == productId
                               select userManualProduct
                              ).FirstOrDefault();

            if (manuProduct != null)
            {
                await _userManualProductRepository.DeleteAsync(manuProduct);

                await _cacheManager.RemoveByPrefixAsync(_prefix);
            }
        }

        public async virtual Task<UserManualCategory> GetCategoryByIdAsync(int categoryId)
        {
            var key = CreateKey(CategoryKey, categoryId);
            return await _cacheManager.GetAsync(key, () =>
            {
                return (from category in _categoryRepository.Table
                        where category.Id == categoryId
                        select category)
                    .FirstOrDefault();
            });
        }
        #endregion
    }
}
