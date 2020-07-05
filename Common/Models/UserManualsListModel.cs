using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public class UserManualsListModel : BaseNopEntityModel
    {
        public UserManualsListModel()
        {
            UserManuals = new List<UserManualModel>();
        }

        public CategoryModel Category { get; set; }
        public List<UserManualModel> UserManuals { get; set; }
    }
}