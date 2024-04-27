using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public partial record UserManualProductModel : BaseNopEntityModel
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