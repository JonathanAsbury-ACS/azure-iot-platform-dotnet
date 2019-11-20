// Copyright (c) Microsoft. All rights reserved.

using Autofac;
using Mmm.Platform.IoT.Common.Services.Diagnostics;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Runtime;
using Mmm.Platform.IoT.Common.WebService;
using Mmm.Platform.IoT.Common.WebService.Runtime;
using Mmm.Platform.IoT.Config.Services;
using Mmm.Platform.IoT.Config.Services.External;
using Mmm.Platform.IoT.Config.Services.Runtime;
using Mmm.Platform.IoT.Config.WebService.Runtime;
using System.Reflection;

namespace Mmm.Platform.IoT.Config.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder, ILogger logger, IHttpClient httpClient)
        {
            // Auto-wire additional assemblies
            var assembly = typeof(IServicesConfig).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

            // Make sure the configuration is read only once.
            IConfig config = new Runtime.Config(new ConfigData(new Logger(Uptime.ProcessId, LogLevel.Info)));
            builder.RegisterInstance(config).As<IConfig>().SingleInstance();

            // Service configuration is generated by the entry point, so we
            // prepare the instance here.
            builder.RegisterInstance(config.ServicesConfig).As<IServicesConfig>().SingleInstance();

            // Auth and CORS setup
            Auth.Startup.SetupDependencies(builder, config);

            // By default Autofac uses a request lifetime, creating new objects
            // for each request, which is good to reduce the risk of memory
            // leaks, but not so good for the overall performance.
            builder.RegisterType<Storage>().As<IStorage>().SingleInstance();
            builder.RegisterType<AzureResourceManagerClient>().As<IAzureResourceManagerClient>().SingleInstance();
        }
    }
}
