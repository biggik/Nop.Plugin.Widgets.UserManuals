namespace Nop.Plugin.Widgets.UserManuals.Models;

public partial record ManufacturerManualsModel
{
    public ManufacturerManualsModel(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public List<CategoryUserManualModel> Categories { get; set; } = [];
}