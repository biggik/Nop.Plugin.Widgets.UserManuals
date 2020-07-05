using System.Collections.Generic;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public class ManufacturerManualsModel
    {
        public ManufacturerManualsModel(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public List<CategoryUserManualModel> Categories { get; set; } = new List<CategoryUserManualModel>();
    }
}