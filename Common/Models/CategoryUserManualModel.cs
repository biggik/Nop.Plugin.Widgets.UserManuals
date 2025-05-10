using System.Collections.Generic;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public partial record CategoryUserManualModel
    {
        public CategoryUserManualModel(CategoryModel category)
        {
            Category = category;
        }

        public CategoryModel Category { get; }
     
        public List<UserManualModel> UserManualsForActiveProducts { get; } = [];
        public List<UserManualModel> UserManualsForDiscontinuedProducts { get; } = [];
    }
}