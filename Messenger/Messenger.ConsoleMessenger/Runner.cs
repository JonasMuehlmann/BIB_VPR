using Messenger.Core.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace Messenger.ConsoleMessenger
{
    class Runner
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            GlobalLogger.Instance.Debug("Setting up the application...");

            // Register services to Host (dependency injection)
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IMessengerApp, MessengerApp>();
                })
                .Build();

            // Create the instance of MessengerApp with the dependencies
            var app = ActivatorUtilities.CreateInstance<MessengerApp>(host.Services);
            app.Run();
            Console.ReadKey();
        }

        /// <summary>
        /// Loads the configuration settings for the application
        /// </summary>
        /// <param name="builder">An instance of ConfigurationBuilder</param>
        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}
