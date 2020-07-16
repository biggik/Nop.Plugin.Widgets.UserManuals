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
#if NOP_PRE_4_3
using Remotion.Linq.Clauses;
using Nop.Core.Data;
#else
using Nop.Data;
using Nop.Services.Caching;
#endif

namespace Nop.Plugin.Widgets.UserManuals.Services
{
    public partial class UserManualService : IUserManualService
    {
        #region Constants
        private const string _prefix = "Nop.status.userManual.";
        private readonly static string _allKey = _prefix + "all-{0}-{1}-{2}";
        private readonly static string _uiKey = _prefix + "ui-{0}";
        private readonly static string _categoryKey = _prefix + "usermanualcategory.all-{0}-{1}";
        private readonly static string _productsKey = _prefix + "usermanualproducts.all-{0}-{1}-{2}-{3}";
        private readonly static string _categoriesKey = _prefix + "usermanualcategories.all-{0}-{1}-{2}";
        private readonly static string _productKey = _prefix + "usermanualproduct-{0}";

#if NOP_PRE_4_3
        private readonly static string UserManualsAllKey = _allKey;
        private readonly static string UserManualsUIKey = _uiKey;
        private readonly static string UserManualsCategoryKey = _categoryKey;
        private readonly static string UserManualProductsKey = _productsKey;
        private readonly static string CategoriesKey = _categoriesKey;
        private readonly static string ProductKey = _productKey;
#else
        private readonly CacheKey UserManualsAllKey = new CacheKey(_allKey, _prefix);
        private readonly CacheKey UserManualsUIKey = new CacheKey(_uiKey, _prefix);
        private readonly CacheKey UserManualsCategoryKey = new CacheKey(_categoryKey, _prefix);
        private readonly CacheKey CategoriesKey = new CacheKey(_categoriesKey, _prefix);
        private readonly CacheKey UserManualProductsKey = new CacheKey(_productsKey, _prefix);
        private readonly CacheKey ProductKey = new CacheKey(_productKey, _prefix);
#endif
        #endregion

        #region Fields
        private readonly IRepository<UserManual> _userManualRepository;
        private readonly IRepository<UserManualProduct> _userManualProductRepository;
        private readonly IRepository<UserManualCategory> _categoryRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<Nop.Core.Domain.Catalog.Manufacturer> _manufacturerRepository;
#if NOP_PRE_4_3
        private readonly ICacheManager _cacheManager;
#else
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IStaticCacheManager _cacheManager;
#endif
        #endregion

        #region Ctor
        public UserManualService(
#if NOP_PRE_4_3
            ICacheManager cacheManager,
#else
            ICacheKeyService cacheKeyService,
            IStaticCacheManager cacheManager,
#endif
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<UserManual> userManualRepository,
            IRepository<UserManualProduct> userManualProductRepository,
            IRepository<UserManualCategory> categoryRespository,
            IRepository<Product> productRepository,
            IRepository<UrlRecord> urlRecordRepository)
        {
            _cacheManager = cacheManager;
#if !NOP_PRE_4_3
            _cacheKeyService = cacheKeyService;
#endif
            _manufacturerRepository = manufacturerRepository;
            _userManualRepository = userManualRepository;
            _userManualProductRepository = userManualProductRepository;
            _categoryRepository = categoryRespository;
            _productRepository = productRepository;
            _urlRecordRepository = urlRecordRepository;
        }
        #endregion

#if NOP_PRE_4_3
        private string CreateKey(string template, params object[] arguments)
        {
            return string.Format(template, arguments);
        }
#else
        private CacheKey CreateKey(CacheKey cacheKey, params object[] arguments)
        {
            return _cacheKeyService.PrepareKeyForShortTermCache(cacheKey, arguments);
        }
#endif

        #region Methods
        public virtual IPagedList<UserManual> GetOrderedUserManuals(bool showUnpublished, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var key = CreateKey(UserManualsAllKey, showUnpublished, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
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

                            select (mId: mOrder, mName, cId:cOrder, cName, userManual)
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
        
        public virtual List<ManufacturerManualsModel> GetOrderedUserManualsWithProducts(bool showUnpublished)
        {
            var key = CreateKey(UserManualsUIKey, showUnpublished);
            return _cacheManager.Get(key, () =>
            {
                var categoryDict = (from category in _categoryRepository.Table
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
            });
        }

        public virtual IPagedList<UserManualCategory> GetOrderedCategories(bool showUnpublished, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var key = CreateKey(CategoriesKey, showUnpublished, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
            {
                return new PagedList<UserManualCategory>(
                    from category in _categoryRepository.Table
                    where category.Published || showUnpublished

                    orderby category.DisplayOrder,
                            category.Name

                    select category,
                    pageIndex,
                    pageSize);
            });
        }

        public virtual UserManual GetById(int id)
        {
            if (id == 0)
                return null;

            return _userManualRepository.GetById(id);
        }

        public virtual IEnumerable<UserManual> GetByProductId(int productId)
        {
            var key = CreateKey(ProductKey, productId);
            return _cacheManager.Get(key, () =>
            {
                return from userManualProduct in _userManualProductRepository.Table
                            where userManualProduct.ProductId == productId
                       join p_userManual in _userManualRepository.Table on 
                            userManualProduct.UserManualId equals p_userManual.Id into pj_userManual
                       from userManual in pj_userManual.DefaultIfEmpty()
                            where userManual.Published
                       select userManual
                       ;
            });
        }

        public virtual void InsertUserManual(UserManual userManual)
        {
            if (userManual == null)
                throw new ArgumentNullException(nameof(userManual));

            _userManualRepository.Insert(userManual);

            _cacheManager.RemoveByPrefix(_prefix);
        }

        public virtual void InsertCategory(UserManualCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            _categoryRepository.Insert(category);

            _cacheManager.RemoveByPrefix(_prefix);
        }

        public virtual void UpdateUserManual(UserManual userManual)
        {
            if (userManual == null)
                throw new ArgumentNullException(nameof(userManual));

            _userManualRepository.Update(userManual);

            _cacheManager.RemoveByPrefix(_prefix);
        }

        public virtual void UpdateCategory(UserManualCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            _categoryRepository.Update(category);

            _cacheManager.RemoveByPrefix(_prefix);
        }

        public virtual void DeleteUserManual(UserManual userManual)
        {
            if (userManual == null)
                throw new ArgumentNullException(nameof(userManual));

            _userManualRepository.Delete(userManual);

            _cacheManager.RemoveByPrefix(_prefix);
        }

        public virtual void DeleteCategory(UserManualCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            _categoryRepository.Delete(category);

            _cacheManager.RemoveByPrefix(_prefix);
        }

        public virtual IEnumerable<UserManual> GetUserManualsByCategoryId(int categoryId, bool showUnpublished = false)
        {
            var key = CreateKey(UserManualsCategoryKey, categoryId, showUnpublished);
            return _cacheManager.Get(key, () =>
            {
                return from userManual in _userManualRepository.Table
                       where userManual.CategoryId == categoryId
                             && (showUnpublished
                                 ||
                                 userManual.Published)

                       orderby userManual.DisplayOrder, userManual.Description

                       select userManual;
            });
        }

        public IPagedList<(UserManualProduct userManualProduct, Product product)> GetProductsForManual(int manualId, bool showUnpublished = false,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var key = CreateKey(UserManualProductsKey, manualId, showUnpublished, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
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

        public void AddProductToManual(int manualId, int productId)
        {
            var manuProducts = from userManualProduct in _userManualProductRepository.Table
                               where userManualProduct.UserManualId == manualId
                                     && userManualProduct.ProductId == productId
                               select userManualProduct;

            if (!manuProducts.Any())
            {
                _userManualProductRepository.Insert(new UserManualProduct
                {
                    UserManualId = manualId,
                    ProductId = productId
                });

                _cacheManager.RemoveByPrefix(_prefix);
            }
        }

        public void RemoveProductFromManual(int manualId, int productId)
        {
            var manuProduct = (from userManualProduct in _userManualProductRepository.Table
                               where userManualProduct.UserManualId == manualId
                                     && userManualProduct.ProductId == productId
                               select userManualProduct
                              ).FirstOrDefault();

            if (manuProduct != null)
            {
                _userManualProductRepository.Delete(manuProduct);

                _cacheManager.RemoveByPrefix(_prefix);
            }
        }

        public virtual UserManualCategory GetCategoryById(int categoryId)
        {
            return (from category in _categoryRepository.Table
                    where category.Id == categoryId
                    select category)
                    .FirstOrDefault();
        }
        #endregion
    }
}
