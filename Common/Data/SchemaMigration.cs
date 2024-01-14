using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.UserManuals.Domain;
using FluentMigrator.Infrastructure;

#if NOP_45
using Nop.Data.Extensions;
#endif

namespace Nop.Plugin.Widgets.UserManuals.Data
{
#if !NOP_45
    [SkipMigrationOnUpdate]
#endif
    [NopMigration("2020/07/04 09:10:11:6455422", "UserManuals Plugin base schema", MigrationProcessType.Installation)]
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
            if (!_productExists)
            {
                Create.TableFor<UserManualProduct>();
            }
            if (!_categoryExists)
            {
                Create.TableFor<UserManualCategory>();
            }
            if (!_manualExists)
            {
                Create.TableFor<UserManual>();
            }
#else
            _migrationManager.BuildTable<UserManualProduct>(Create);
            _migrationManager.BuildTable<UserManualCategory>(Create);
            _migrationManager.BuildTable<UserManual>(Create);
#endif
        }

#if NOP_45
        private bool _productExists;
        private bool _categoryExists;
        private bool _manualExists;
        public override void GetUpExpressions(IMigrationContext context)
        {
            // Schema migration is always attempting to create the tables, so override 
            // here to get out of this situation
            _productExists = context.QuerySchema.TableExists("dbo", "StatusUserManualProduct");
            _categoryExists = context.QuerySchema.TableExists("dbo", "StatusUserManualCategory");
            _manualExists = context.QuerySchema.TableExists("dbo", "StatusUserManual");

            base.GetUpExpressions(context);
        }
#endif
    }
}
