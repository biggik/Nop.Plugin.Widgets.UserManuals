using FluentMigrator;
using FluentMigrator.Infrastructure;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.UserManuals.Domain;

namespace Nop.Plugin.Widgets.UserManuals.Data;

[NopMigration("2020/07/04 09:10:11:6455422", "UserManuals Plugin base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
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
    }

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
}
