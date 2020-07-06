using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.UserManuals.Domain;

namespace Nop.Plugin.Widgets.UserManuals.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2020/07/04 09:10:11:6455422", "UserManuals Plugin base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            _migrationManager.BuildTable<UserManualProduct>(Create);
            _migrationManager.BuildTable<UserManualCategory>(Create);
            _migrationManager.BuildTable<UserManual>(Create);
        }
    }
}
