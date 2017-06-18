using System;
using AddressSampler.Sampling;
using Microsoft.Extensions.Configuration;
using Serilog;
using StructureMap;

namespace AddressSampler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = Configure();
            ConfigureSerilog(configuration);
            Log.Logger.Information("Program Started.");
            var container = CreateContainer(configuration);
            try
            {
                var service = container.GetInstance<ISamplingService>();
                service.Run();
            }
            catch (AggregateException e)
            {
                e.Flatten().Handle((arg) =>
                {
                    Log.Logger.Error(e, "Service Error.");
                    return true;
                });
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Error.");
            }

            Log.Information("Program Complete.");
        }

        private static Container CreateContainer(IConfiguration configuration)
        {
            var samplingOptions = configuration
                .GetSection("Sample")
                .Get<SamplingOptions>();
            Log.Logger.Information($"Found Sampling configuration: {samplingOptions.AsJson()}");

            var container = new Container();
            container.Configure((obj) =>
            {
                obj.Scan((_) =>
                {
                    _.AssemblyContainingType<ISamplingService>();
                    _.WithDefaultConventions();
                });

                obj.ForSingletonOf<ILogger>().Use(Log.Logger);
                obj.ForSingletonOf<SamplingOptions>().Use(samplingOptions);
            });

            return container;
        }

        private static void ConfigureSerilog(IConfiguration configuration)
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