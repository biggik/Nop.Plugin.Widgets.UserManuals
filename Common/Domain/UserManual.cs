using System;
using System.ComponentModel.DataAnnotations;
using Nop.Core;

namespace Nop.Plugin.Widgets.UserManuals.Domain
{
    public partial class UserManual : BaseEntity
    {
        public string Description { get; set; }

        public int ManufacturerId { get; set; }
        public int CategoryId { get; set; }

        [UIHint("Download")]
        public int DocumentId { get; set; }
        public string OnlineLink { get; set; }

        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
    }
}
