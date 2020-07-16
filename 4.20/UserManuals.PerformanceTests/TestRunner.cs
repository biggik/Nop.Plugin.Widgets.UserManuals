using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.UserManuals.Models;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UserManuals.PerformanceTests
{
    public interface ITestRunner
    {
        (TimeSpan runTime, int manuals, List<ManufacturerManualsModel> model) RunExisting();
        (TimeSpan runTime, int manuals, List<ManufacturerManualsModel> model) RunOptimized();
    }

    public class TestRunner : ITestRunner
    {
        private readonly IUserManualService _userManualService;
        private readonly IManufacturerService _manufacturerService;
        private readonly UrlRecordService _urlRecordService;

        public TestRunner(IUserManualService userManualService,
            IManufacturerService manufacturerService,
            UrlRecordService urlRecordService)
        {
            this._userManualService = userManualService;
            this._manufacturerService = manufacturerService;
            this._urlRecordService = urlRecordService;
        }

        public (TimeSpan runTime, int manuals, List<ManufacturerManualsModel> model) RunExisting()
        {
            int manuals = 0;
            var sw = new Stopwatch();
            sw.Start();
            var model = new List<ManufacturerManualsModel>();

            var manufacturerDict = _manufacturerService.GetAllManufacturers().ToDictionary(x => x.Id, x => x);
            var categoryDict = _userManualService.GetOrderedCategories(showUnpublished: false).ToDictionary(x => x.Id, x => x);

            string lastManufacturer = "";
            string lastCategoryName = "";
            ManufacturerManualsModel manufacturerModel = null;
            CategoryUserManualModel categoryModel = null;

            foreach (var um in _userManualService.GetOrderedUserManuals(showUnpublished: false))
            {
                var manufacturerName = manufacturerDict.ContainsKey(um.ManufacturerId) ? manufacturerDict[um.ManufacturerId].Name : "";
                if (manufacturerName != lastManufacturer)
                {
                    manufacturerModel = new ManufacturerManualsModel(manufacturerName);
                    model.Add(manufacturerModel);
                    categoryModel = null;
                    lastManufacturer = manufacturerName;
                }

                var categoryName = categoryDict.ContainsKey(um.CategoryId) ? categoryDict[um.CategoryId].Name : "";
                if (categoryName != lastCategoryName || categoryModel == null)
                {
                    categoryModel = new CategoryUserManualModel(new CategoryModel { Name = categoryName });
                    manufacturerModel.Categories.Add(categoryModel);
                    lastCategoryName = categoryName;
                }

                var manualProducts = _userManualService.GetProductsForManual(um.Id, showUnpublished: true);
                if (manualProducts.Any())
                {
                    // We repeat the manual for each product
                    foreach (var umProd in manualProducts)
                    {
                        manuals++;
                        var umm = um.ToModel();
                        umm.ProductSlug = _urlRecordService.GetActiveSlug(umProd.userManualProduct.ProductId, nameof(Product), 0); // TODO: use languageId ?

                        if (!string.IsNullOrEmpty(umm.ProductSlug) && umProd.product.Published)
                        {
                            categoryModel.UserManualsForActiveProducts.Add(umm);
                        }
                        else
                        {
                            categoryModel.UserManualsForDiscontinuedProducts.Add(umm);
                        }
                    }
                }
                else
                {
                    manuals++;
                    // No products = discontinued product
                    categoryModel.UserManualsForDiscontinuedProducts.Add(um.ToModel());
                }
            }

            sw.Stop();
            return (sw.Elapsed, manuals, model);
        }

        public (TimeSpan runTime, int manuals, List<ManufacturerManualsModel> model) RunOptimized()
        {
            int manuals = 0;
            var sw = new Stopwatch();
            sw.Start();
            var model = _userManualService.GetOrderedUserManualsWithProducts(showUnpublished: false);
            foreach (var man in model)
            {
                foreach (var cat in man.Categories)
                {
                    manuals += cat.UserManualsForActiveProducts.Count() + cat.UserManualsForDiscontinuedProducts.Count();
                }
            }
            sw.Stop();
            return (sw.Elapsed, manuals, model);
        }
    }
}
