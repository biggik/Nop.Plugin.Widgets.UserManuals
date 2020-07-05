using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Widgets.UserManuals.Domain;

namespace Nop.Plugin.Widgets.UserManuals.Mapping
{
    public partial class UserManualProductsRecordMap : NopEntityTypeConfiguration<UserManualProduct>
    {
        public static string TableName = "StatusUserManualProduct";

        public override void Configure(EntityTypeBuilder<UserManualProduct> builder)
        {
            builder.ToTable(TableName);
            builder.HasKey(x => x.Id);
            builder.HasAlternateKey(x => x.UserManualId);
            base.Configure(builder);
        }
    }
}