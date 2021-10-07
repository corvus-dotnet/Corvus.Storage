// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Corvus.Storage.Examples.ConsoleApp.AppScope
{
    /// <summary>
    /// Entry point for console application demonstrating use of application-wide scope.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private static async Task Main(string[] args)
        {
            // Using generic host https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host
            // because it sets up configuration and DI in the usual way.
            using IHost host = CreateHostBuilder(args).Build();

            string mode = args.Length > 0 ? args[0] : "blob";
            IEntryPoint? entryPoint = mode switch
            {
                "blob" => host.Services.GetRequiredService<UsingBlobStorage>(),

                _ => null
            };

            if (entryPoint == null)
            {
                Console.WriteLine("Unknown mode: " + mode);
            }
            else
            {
                await entryPoint.MainAsync().ConfigureAwait(false);
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingularAzureBlobStorageScopeSource(hostContext.Configuration);
                    services.AddAzureBlobStorageClient();

                    services.AddSingleton<UsingBlobStorage>();
                });
    }
}
