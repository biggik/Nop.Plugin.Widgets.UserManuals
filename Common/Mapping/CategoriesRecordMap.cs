using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Widgets.UserManuals.Domain;

namespace Nop.Plugin.Widgets.UserManuals.Mapping
{
    public partial class CategoriesRecordMap : NopEntityTypeConfiguration<UserManualCategory>
    {
        public static string TableName = "StatusUserManualCategory";
        
        public override void Configure(EntityTypeBuilder<UserManualCategory> builder)
        {
            builder.ToTable(TableName);
            builder.HasKey(x => x.Id);
            base.Configure(builder);
        }
    }
}