using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public class AddProductToUserManualModel : BaseNopModel
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