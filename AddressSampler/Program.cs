using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using StructureMap;

namespace AddressSampler
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = Configure();
            ConfigureSerilog(configuration);
            var container = CreateContainer(configuration);
            var service = container.GetInstance<ISamplingService>();
            try{
                service.Run().Wait();
            }catch(AggregateException e){
                e.Flatten().Handle((arg) =>
                {
                    Log.Logger.Error(e, "Service Error.");
                    return true;
                });
            }
        }

        private static Container CreateContainer(IConfigurationRoot configuration)
        {
            var container = new Container();
            container.Configure((obj) => {
                obj.Scan((_) => {
                    _.AssemblyContainingType<ISamplingService>();
                    _.WithDefaultConventions();
                });

                obj.ForSingletonOf<ILogger>().Use(Log.Logger);
            });

            return container;
        }

        private static void ConfigureSerilog(IConfigurationRoot configuration)
        {
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.LiterateConsole()
				.WriteTo.File(configuration["Logging:LogFile"])
				.CreateLogger();
        }

        private static IConfigurationRoot Configure()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
