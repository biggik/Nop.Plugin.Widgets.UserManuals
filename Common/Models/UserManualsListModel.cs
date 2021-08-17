using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
#if !NOP_4_4
    public partial class 
#else
    public partial record
#endif
    UserManualsListModel : BaseNopEntityModel
    {
        public UserManualsListModel()
        {
            UserManuals = new List<UserManualModel>();
        }

        public CategoryModel Category { get; set; }
        public List<UserManualModel> UserManuals { get; set; }
    }
}