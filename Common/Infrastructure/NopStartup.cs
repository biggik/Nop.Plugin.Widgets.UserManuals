using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.UserManuals.Services;
using Nop.Services.Events;
using Nop.Web.Framework.Events;

namespace Nop.Plugin.Widgets.UserManuals.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserManualService, UserManualService>();
        services.AddScoped<IUserManualModelFactory, UserManualModelFactory>();
#if NOP_48
        services.AddScoped<IConsumer<AdminMenuCreatedEvent>, AdminMenuEventHandler>();
#endif
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 1;
}
