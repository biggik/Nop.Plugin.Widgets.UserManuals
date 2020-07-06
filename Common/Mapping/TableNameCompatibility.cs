using Nop.Data.Mapping;
using Nop.Plugin.Widgets.UserManuals.Domain;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.UserManuals.Mapping
{
    public partial class TableNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(UserManualProduct), "StatusUserManualProduct" },
            { typeof(UserManualCategory), "StatusUserManualCategory" },
            { typeof(UserManual), "StatusUserManual" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}
