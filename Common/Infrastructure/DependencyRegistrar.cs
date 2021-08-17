#if !NOP_4_4
using Autofac;
#else
using Microsoft.Extensions.DependencyInjection;
#endif
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Widgets.UserManuals.Services;
#if NOP_PRE_4_3
using Autofac.Core;
using Nop.Core.Data;
using Nop.Data;
using Nop.Plugin.Widgets.UserManuals.Data;
using Nop.Plugin.Widgets.UserManuals.Domain;
using Nop.Web.Framework.Infrastructure.Extensions;
#endif

namespace Nop.Plugin.Widgets.UserManuals.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
#if !NOP_4_4
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<UserManualService>().As<IUserManualService>().InstancePerLifetimeScope();
            builder.RegisterType<UserManualModelFactory>().As<IUserManualModelFactory>().InstancePerLifetimeScope();

#if NOP_PRE_4_3
            const string context = "nop_object_context_usermanuals";
            //data context
            builder.RegisterPluginDataContext<UserManualsObjectContext>(context);
            
            //override required repository with our custom context
            builder.RegisterType<EfRepository<UserManual>>()
                .As<IRepository<UserManual>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(context))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<UserManualProduct>>()
                .As<IRepository<UserManualProduct>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(context))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<UserManualCategory>>()
                .As<IRepository<UserManualCategory>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(context))
                .InstancePerLifetimeScope();
#endif
        }
#else
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<UserManualService>();
            services.AddScoped<UserManualModelFactory>();
        }
#endif

        public int Order => 1;
    }
}
