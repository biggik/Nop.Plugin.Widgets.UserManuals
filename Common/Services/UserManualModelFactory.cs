using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Web.Areas.Admin.Factories;
using Nop.Services.Catalog;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Services.Seo;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.UserManuals.Services
{
    public partial class UserManualModelFactory : IUserManualModelFactory
    {
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        public UserManualModelFactory(
            IProductService productService,
            IUrlRecordService urlRecordService,
            IBaseAdminModelFactory baseAdminModelFactory
        )
        {
            _productService = productService;
            _urlRecordService = urlRecordService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

#if NOP_4_4
        public async virtual Task<AddProductToUserManualSearchModel> PrepareAddProductToUserManualSearchModelAsync(AddProductToUserManualSearchModel searchModel)
#else
        public virtual AddProductToUserManualSearchModel PrepareAddProductToUserManualSearchModel(AddProductToUserManualSearchModel searchModel)
#endif
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
#if NOP_4_4
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);
#else
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);
#endif

            //prepare available manufacturers
#if NOP_4_4
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);
#else
            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);
#endif

            //prepare available stores
#if NOP_4_4
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
#else
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);
#endif

            //prepare available vendors
#if NOP_4_4
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);
#else
            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);
#endif

            //prepare available product types
#if NOP_4_4
            await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);
#else
            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);
#endif

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

#if NOP_4_4
        public async virtual Task<AddProductToUserManualListModel> PrepareAddProductToUserManualListModelAsync
#else
        public virtual AddProductToUserManualListModel PrepareAddProductToUserManualListModel
#endif
            (AddProductToUserManualSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
#if NOP_4_4
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
#else
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerId: searchModel.SearchManufacturerId,
#endif
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
#if NOP_4_4
            var model = await new AddProductToUserManualListModel()
                .PrepareToGridAsync(searchModel, products, () =>
                {
                    return products.SelectAwait(async product =>
#else
            var model = new AddProductToUserManualListModel()
                .PrepareToGrid(searchModel, products, () =>
                {
                    return products.Select(product =>
#endif
                    {
                        var productModel = product.ToModel<ProductModel>();
#if NOP_4_4
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
#else
                    productModel.SeName = _urlRecordService.GetSeName(product, 0, true, false);
#endif

                    return productModel;
                });
            });

            return model;
        }
    }
}
