using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Plugin.Widgets.UserManuals.Resources;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
#if !NOP_4_4
    public partial class 
#else
    public partial record
#endif
    UserManualProductModel : BaseNopEntityModel
    {
        public UserManualProductModel()
        {
        }

        public int UserManualId { get; set; }

        public int ProductId { get; set; }

        public bool Published { get; set; }

        public string ProductName { get; set; }
    }
}