using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using Nop.Plugin.Widgets.UserManuals.Resources;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
#if !NOP_ASYNC
    public partial class 
#else
    public partial record
#endif
     CategoryModel : BaseNopEntityModel
    {
        [NopResourceDisplayName(CategoryResources.Name)]
        public string Name { get; set; }

        [NopResourceDisplayName(GenericResources.DisplayOrder)]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName(GenericResources.Published)]
        public bool Published { get; set; } = true;
    }
}