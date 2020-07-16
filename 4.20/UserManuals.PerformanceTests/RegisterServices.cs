using Nop.Web.Framework.Infrastructure.Extensions;

namespace UserManuals.PerformanceTests
{
    public static class RegisterServices
    {
        public static void Register(Microsoft.Extensions.DependencyInjection.ServiceCollection services)
        {
            services.AddHttpContextAccessor();
        }
    }
}
