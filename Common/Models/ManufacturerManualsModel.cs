using System.Collections.Generic;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
#if !NOP_4_4
    public partial class 
#else
    public partial record
#endif
    ManufacturerManualsModel
    {
        public ManufacturerManualsModel(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public List<CategoryUserManualModel> Categories { get; set; } = new List<CategoryUserManualModel>();
    }
}