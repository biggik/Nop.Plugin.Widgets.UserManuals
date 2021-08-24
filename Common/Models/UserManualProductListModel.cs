using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
#if !NOP_ASYNC
    public partial class 
#else
    public partial record
#endif
    UserManualProductListModel : BasePagedListModel<UserManualProductModel>
    {
    }
}
