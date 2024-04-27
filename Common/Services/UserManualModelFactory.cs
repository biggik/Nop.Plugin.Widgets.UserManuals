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

        public async virtual Task<AddProductToUserManualSearchModel> PrepareAddProductToUserManualSearchModelAsync(AddProductToUserManualSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available product types
            await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        public async virtual Task<AddProductToUserManualListModel> PrepareAddProductToUserManualListModelAsync
            (AddProductToUserManualSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new AddProductToUserManualListModel()
                .PrepareToGridAsync(searchModel, products, () =>
                {
                    return products.SelectAwait(async product =>
                    {
                        var productModel = product.ToModel<ProductModel>();
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }
    }
}
