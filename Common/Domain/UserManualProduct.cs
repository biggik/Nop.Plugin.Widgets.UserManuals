using Nop.Core;

namespace Nop.Plugin.Widgets.UserManuals.Domain
{
    public partial class UserManualProduct : BaseEntity
    {
        public int UserManualId { get; set; }
        public int ProductId { get; set; }
    }
}
