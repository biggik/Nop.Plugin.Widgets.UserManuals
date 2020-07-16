using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.ResolveAnything;
using EasyCaching.Core;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using System;
using Nop.Web.Framework.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Threading;

namespace UserManuals.PerformanceTests
{
    public class TestEnvironment : IHostingEnvironment
    {
        public string EnvironmentName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string WebRootPath { get => @"N:\nopCommerce 4.20\Presentation\Nop.Web\"; set => throw new NotImplementedException(); }
        public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ContentRootPath { get => @"N:\nopCommerce 4.20\Presentation\Nop.Web"; set => throw new NotImplementedException(); }
        public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class DummyLifetime : IApplicationLifetime, Microsoft.Extensions.Hosting.IApplicationLifetime
    {
        public CancellationToken ApplicationStarted { get; private set; }

        public CancellationToken ApplicationStopping { get; private set; }

        public CancellationToken ApplicationStopped { get; private set; }

        public DummyLifetime()
        {
            ApplicationStarted 
                = ApplicationStopping 
                = ApplicationStopped
                    = new CancellationToken(false);
        }

        public void StopApplication()
        {
        }
    }

    class Program
    {
        private static IContainer container;

        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterType<TestEnvironment>().AsSelf().As<IHostingEnvironment>();
            builder.RegisterType<DummyLifetime>().AsSelf().As<IApplicationLifetime>();
            builder.RegisterType<DummyLifetime>().AsSelf().As<Microsoft.Extensions.Hosting.IApplicationLifetime>();
            
            builder.RegisterType<TestRunner>().AsSelf().As<ITestRunner>();

            CommonHelper.DefaultFileProvider = new NopFileProvider(new TestEnvironment());

            var typeFinder = new AppDomainTypeFinder();
            var config = Config;
            services.AddSingleton(config);

            var dr1 = new Nop.Web.Framework.Infrastructure.DependencyRegistrar();
            dr1.Register(builder, typeFinder, config);
            var dr2 = new Nop.Plugin.Widgets.UserManuals.Infrastructure.DependencyRegistrar();
            dr2.Register(builder, typeFinder, config);

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            var engine = new NopEngine();
            engine.ConfigureServices(services, configuration, config);

            RegisterServices.Register(services);

            services.AddSingleton<ILoggerFactory, LoggerFactory>(sp =>
                new LoggerFactory(
                    sp.GetRequiredService<IEnumerable<ILoggerProvider>>(),
                    sp.GetRequiredService<IOptionsMonitor<LoggerFilterOptions>>()
                )
            );
            builder.Populate(services);

            container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                try
                {
                    var runner = scope.Resolve<ITestRunner>();

                    const int cycles = 4;
                    Console.Out.WriteLine(new string('=', 40));
                    Console.Out.WriteLine("Existing");
                    for (int i = 0; i < cycles; i++)
                    {
                        var res = runner.RunExisting();
                        Console.Out.WriteLine($"      {res.runTime} - with {res.model.Count} records and {res.manuals} manuals");
                    }

                    Console.Out.WriteLine(new string('=', 40));
                    Console.Out.WriteLine("Optimized");
                    for (int i = 0; i < cycles; i++)
                    {
                        var res = runner.RunOptimized();
                        Console.Out.WriteLine($"      {res.runTime} - with {res.model.Count} records and {res.manuals} manuals");
                    }
                }
                catch (Exception ex)
                {
                    while (ex != null)
                    {
                        Console.Out.WriteLine(ex.GetType().FullName);
                        Console.Out.WriteLine(ex.Message);
                        Console.Out.WriteLine(new string('=', 50));
                        ex = ex.InnerException;
                    }
                }
             
                Console.Out.WriteLine("All done");
            }
        }

        private static NopConfig Config =>
            new NopConfig
            {
                //Enable if you want to see the full error in production environment. It's ignored (always enabled) in development environment
                DisplayFullErrorStack = false,

                //Windows Azure BLOB storage.
                //Specify your connection string, container name, end point for BLOB storage here
                AzureBlobStorageConnectionString = "",
                AzureBlobStorageContainerName = "",
                AzureBlobStorageEndPoint = "",
                AzureBlobStorageAppendContainerName = true,

                //Redis support (used by web farms, Azure, etc). Find more about it at https://azure.microsoft.com/en-us/documentation/articles/cache-dotnet-how-to-use-azure-redis-cache/
                RedisEnabled = false,
                //Redis database id; If you need to use a specific redis database, just set its number here. Set empty if should use the different database for each data type (used by default); set -1 if you want to use the default database
                RedisDatabaseId = null,
                RedisConnectionString = "127.0.0.1:6379,ssl=False",
                UseRedisToStoreDataProtectionKeys = false,
                UseRedisForCaching = false,
                UseRedisToStorePluginsInfo = false,

                //You can get the latest version of user agent strings at http://browscap.org/
                //Leave "CrawlersOnlyDatabasePath" attribute empty if you want to use full version of "browscap.xml" file
                UserAgentStringsPath = "~/App_Data/browscap.xml",
                CrawlerOnlyUserAgentStringsPath = "~/App_Data/browscap.crawlersonly.xml",

                //Do not edit this element. For advanced users only
                DisableSampleDataDuringInstallation = false,
                UseFastInstallationService = false,
                PluginsIgnoredDuringInstallation = "",

                //Enable if you want to clear /Plugins/bin directory on application startup
                ClearPluginShadowDirectoryOnStartup = true,
                //Enable if you want to copy "locked" assemblies from /Plugins/bin directory to temporary subdirectories on application startup
                CopyLockedPluginAssembilesToSubdirectoriesOnStartup = false,
                //Enable if you want to copy plugins library to the /Plugins/bin directory on application startup
                UsePluginsShadowCopy = true,
                //Enable if you want to load an assembly into the load-from context, by passing some security checks
                UseUnsafeLoadAssembly = true,

                //Enable for backwards compatibility with SQL Server 2008 and SQL Server 2008R2
                UseRowNumberForPaging = false,

                //Enable to store TempData in the session state
                UseSessionStateTempDataProvider = false
            };
    }
}
