using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.UserManuals.Domain;
#if NOP_45
using Nop.Data.Extensions;
#endif

namespace Nop.Plugin.Widgets.UserManuals.Data
{
#if !NOP_45
    [SkipMigrationOnUpdate]
#endif
    [NopMigration("2020/07/04 09:10:11:6455422", "UserManuals Plugin base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
#if !NOP_45
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }
#endif

        public override void Up()
        {
#if NOP_45
            Create.TableFor<UserManualProduct>();
            Create.TableFor<UserManualCategory>();
            Create.TableFor<UserManual>();
#else
            _migrationManager.BuildTable<UserManualProduct>(Create);
            _migrationManager.BuildTable<UserManualCategory>(Create);
            _migrationManager.BuildTable<UserManual>(Create);
#endif
        }
    }
}
