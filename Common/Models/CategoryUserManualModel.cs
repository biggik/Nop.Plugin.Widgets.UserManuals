using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public class CategoryUserManualModel
    {
        public CategoryUserManualModel(CategoryModel category)
        {
            Category = category;
        }

        public CategoryModel Category { get; }
     
        public List<UserManualModel> UserManualsForActiveProducts { get; } = new List<UserManualModel>();
        public List<UserManualModel> UserManualsForDiscontinuedProducts { get; } = new List<UserManualModel>();
    }
}