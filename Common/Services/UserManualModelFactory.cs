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

        public virtual AddProductToUserManualSearchModel PrepareAddProductToUserManualSearchModel(AddProductToUserManualSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            //prepare available manufacturers
            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            //prepare available product types
            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        public virtual AddProductToUserManualListModel PrepareAddProductToUserManualListModel(AddProductToUserManualSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerId: searchModel.SearchManufacturerId,
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new AddProductToUserManualListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Select(product =>
                {
                    var productModel = product.ToModel<ProductModel>();
                    productModel.SeName = _urlRecordService.GetSeName(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }
    }
}
