using nopLocalizationHelper;

namespace Nop.Plugin.Widgets.UserManuals.Resources
{
    internal static class Cultures
    {
        public const string EN = "en-US";
        public const string IS = "is-IS";
    }

    [LocaleStringProvider]
    public static class UserManualResources
    {
        [LocaleString(Cultures.EN, "User manual")]
        [LocaleString(Cultures.IS, "Leiðarvísir")]
        public const string UserManual = "Status.UserManualWidget.UserManual.UserManual";

        [LocaleString(Cultures.EN, "Manufacturer", "The manufacturer of the product the user manual refers to")]
        [LocaleString(Cultures.IS, "Framleiðandi", "Framleiðandi vörunnar sem leiðarvísirinn á við")]
        public const string Manufacturer = "Status.UserManualWidget.UserManual.Manufacturer";

        [LocaleString(Cultures.EN, "Category", "The category the user manual belongs to")]
        [LocaleString(Cultures.IS, "Flokkun", "Flokkur sem leiðarvísirinn á heima í")]
        public const string Category = "Status.UserManualWidget.UserManual.Category";

        [LocaleString(Cultures.EN, "Product", "The product that the user manual refers to")]
        [LocaleString(Cultures.IS, "Vara", "Varan sem leiðarvísirinn á við")]
        public const string Product = "Status.UserManualWidget.UserManual.Product";

        [LocaleString(Cultures.EN, "Products", "Here you can add products that the manual refers to")]
        [LocaleString(Cultures.IS, "Vörur", "Hér er hægt að vísa í vörur sem leiðarvísirinn á við")]
        public const string Products = "Status.UserManualWidget.UserManual.Products";

        [LocaleString(Cultures.EN, "You need to save the user manual before adding products")]
        [LocaleString(Cultures.IS, "Þú verður að vista áður en hægt er að bæta við vörum sem leiðarvísirinn á við")]
        public const string SaveBeforeAddingProducts = "Status.UserManualWidget.UserManual.SaveBeforeAddingProducts";

        [LocaleString(Cultures.EN, "Description", "The description of the user manual")]
        [LocaleString(Cultures.IS, "Lýsing", "Lýsing á leiðarvísinum")]
        public const string Description = "Status.UserManualWidget.UserManual.Description";

        [LocaleString(Cultures.EN, "User manual", "The document containing the user manual")]
        [LocaleString(Cultures.IS, "Leiðarvísir", "Skjalið sem inniheldur leiðarvísinn")]
        public const string Document = "Status.UserManualWidget.UserManual.Document";

        [LocaleString(Cultures.EN, "Online link", "A link to an online reference containing a representation of the user manual (e.g. yumpu.com)")]
        [LocaleString(Cultures.IS, "Hlekkur", "Hlekkur á leiðarvísinn þar sem hægt er að skoða hann (t.d. á yumpu.com)")]
        public const string OnlineLink = "Status.UserManualWidget.UserManual.OnlineLink";

        [LocaleString(Cultures.EN, "User Manuals")]
        [LocaleString(Cultures.IS, "Leiðarvísar")]
        public const string ListCaption = "Status.UserManualWidget.UserManual.Caption.UserManuals";

        [LocaleString(Cultures.EN, "Goto product")]
        [LocaleString(Cultures.IS, "Fara í vöru")]
        public const string ProductLink = "Status.UserManualWidget.UserManual.ProductLink";

        [LocaleString(Cultures.EN, "Download the user manual document")]
        [LocaleString(Cultures.IS, "Hlaða niður leiðarvísinum")]
        public const string DownloadText = "Status.UserManualWidget.UserManual.DownloadText";

        [LocaleString(Cultures.EN, "View the user manual online")]
        [LocaleString(Cultures.IS, "Skoða leiðarvísinn á netinu")]
        public const string OnlineLinkText = "Status.UserManualWidget.UserManual.OnlineLinkText";
    }

    [LocaleStringProvider]
    public static class UserManualsActionResources
    {
        [LocaleString(Cultures.EN, "User Manuals")]
        [LocaleString(Cultures.IS, "Leiðarvísar")]
        public const string UserManualListButton = "Status.UserManualWidget.Action.UserManuals.List";

        [LocaleString(Cultures.EN, "Back to User Manual list")]
        [LocaleString(Cultures.IS, "Tilbaka í leiðarvísalista")]
        public const string BackToList = "Status.UserManualWidget.Action.UserManual.BackToList";
    }

    [LocaleStringProvider]
    public static class AdminResources
    {
        [LocaleString(Cultures.EN, "User Manuals")]
        [LocaleString(Cultures.IS, "Leiðarvísar")]
        public const string UserManualListCaption = "Status.UserManualWidget.Admin.UserManual.Caption.UserManuals";

        [LocaleString(Cultures.EN, "Add a new user manual")]
        [LocaleString(Cultures.IS, "Skrá nýjan leiðarvísi")]
        public const string AddUserManualCaption = "Status.UserManualWidget.Admin.UserManual.Caption.Add";

        [LocaleString(Cultures.EN, "Add new")]
        [LocaleString(Cultures.IS, "Skrá leiðarvísi")]
        public const string AddUserManualButton = "Status.UserManualWidget.Admin.UserManual.Button.Add";

        [LocaleString(Cultures.EN, "See published")]
        [LocaleString(Cultures.IS, "Sjá virka")]
        public const string SeePublished = "Status.UserManualWidget.Admin.UserManual.Button.SeePublished";

        [LocaleString(Cultures.EN, "See all")]
        [LocaleString(Cultures.IS, "Sjá alla")]
        public const string SeeAllButton = "Status.UserManualWidget.Admin.UserManual.Button.SeeAll";

        [LocaleString(Cultures.EN, "Edit user manual")]
        [LocaleString(Cultures.IS, "Breyta leiðarvísi")]
        public const string EditUserManualCaption = "Status.UserManualWidget.Admin.UserManual.Caption.Edit";

        [LocaleString(Cultures.EN, "Add new")]
        [LocaleString(Cultures.IS, "Skrá flokk")]
        public const string AddCategoryCaption = "Status.UserManualWidget.Admin.Category.Caption.Add";

        [LocaleString(Cultures.EN, "Add new")]
        [LocaleString(Cultures.IS, "Skrá flokk")]
        public const string AddCategoryButton = "Status.UserManualWidget.Admin.Category.Button.Add";

        [LocaleString(Cultures.EN, "Edit category")]
        [LocaleString(Cultures.IS, "Breyta flokki")]
        public const string EditCategoryCaption = "Status.UserManualWidget.Admin.Category.Caption.Edit";

        [LocaleString(Cultures.EN, "User manual category list")]
        [LocaleString(Cultures.IS, "Flokkalisti leiðarvísa")]
        public const string CategoryListCaption = "Status.UserManualWidget.Admin.Category.Caption.Categories";
    }

    [LocaleStringProvider]
    public static class CategoryResources
    {
        [LocaleString(Cultures.EN, "Name", "The category name")]
        [LocaleString(Cultures.IS, "Nafn", "Nafn flokksins")]
        public const string Name = "Status.UserManualWidget.Category.Name";
    }

    [LocaleStringProvider]
    public static class CategoryActionResources
    {
        [LocaleString(Cultures.EN, "Back to Category list")]
        [LocaleString(Cultures.IS, "Tilbaka í flokkalista")]
        public const string BackToList = "Status.UserManualWidget.Action.Category.BackToList";

        [LocaleString(Cultures.EN, "Categories")]
        [LocaleString(Cultures.IS, "Flokkar")]
        public const string CategoriesListButton = "Status.UserManualWidget.Action.Category.List";
    }

    [LocaleStringProvider]
    public static class ConfigurationResources
    {
        [LocaleString(Cultures.EN, "Widget zones", "In which zones should the widget be displayed")]
        [LocaleString(Cultures.IS, "Birta í", "Hvar á síðunni á að birta íhlutinn")]
        public const string WidgetZones = "Status.UserManualWidget.Configuration.WidgetZones";
    }

    [LocaleStringProvider]
    public static class GenericResources
    {
        [LocaleString(Cultures.EN, "Display order", "The display order for the record")]
        [LocaleString(Cultures.IS, "Birtingarröð", "Birtingarröð færslunnar")]
        public const string DisplayOrder = "Status.UserManualWidget.Generic.DisplayOrder";

        [LocaleString(Cultures.EN, "Published", "Should the record be displayed?")]
        [LocaleString(Cultures.IS, "Birt", "Á að birta færsluna?")]
        public const string Published = "Status.UserManualWidget.Generic.Published";

        [LocaleString(Cultures.EN, "None")]
        [LocaleString(Cultures.IS, "Enginn")]
        public const string None = "Status.UserManualWidget.Generic.None";
    }
}
