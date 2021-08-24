using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.UserManuals.Models
{

#if !NOP_ASYNC
    public partial class 
#else
    public partial record
#endif
    AddProductToUserManualModel : BaseNopModel
    {
        #region Ctor

        public AddProductToUserManualModel()
        {
        }
        #endregion

        #region Properties

        public int UserManualId { get; set; }

        public IList<int> SelectedProductIds { get; set; } = new List<int>();

        #endregion
    }
}