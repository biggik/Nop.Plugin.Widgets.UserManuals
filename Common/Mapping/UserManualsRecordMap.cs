using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Widgets.UserManuals.Domain;

namespace Nop.Plugin.Widgets.UserManuals.Mapping
{
    public partial class UserManualsRecordMap : NopEntityTypeConfiguration<UserManual>
    {
        public static string TableName = "StatusUserManual";

        public override void Configure(EntityTypeBuilder<UserManual> builder)
        {
            builder.ToTable(TableName);
            builder.HasKey(x => x.Id);
            base.Configure(builder);
        }
    }
}