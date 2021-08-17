using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
#if !NOP_4_4
    public partial class 
#else
    public partial record
#endif
    UserManualProductListModel : BasePagedListModel<UserManualProductModel>
    {
    }
}
