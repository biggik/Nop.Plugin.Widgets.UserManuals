using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
#if !NOP_ASYNC
    public partial class 
#else
    public partial record
#endif
    CategoryUserManualModel
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