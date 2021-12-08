// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.CommandLine.Parsing;
using System.Threading.Tasks;

using Corvus.Storage.Examples.Azure.BlobStorage;
using Corvus.Storage.Examples.Azure.Cosmos;
using Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration.Cli;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Parser commandLineParser = AppCommandBuilder.MakeRootCommand(ConfigureHostBuilder);
            ////using IHost host = CreateHostBuilder(args).Build();
            await commandLineParser.InvokeAsync(args).ConfigureAwait(false);
        }

        private static void ConfigureHostBuilder(IHostBuilder hostBuilder) => hostBuilder
            .ConfigureServices((hostContext, services) =>
            {
                ApplicationConfigurationSettings configuration = hostContext.Configuration.Get<ApplicationConfigurationSettings>();
                services.AddSingleton(configuration);
                services.AddAzureBlobStorageClientSourceFromDynamicConfiguration();
                services.AddCosmosContainerSourceFromDynamicConfiguration();

                services.AddAzureTokenCredentialSourceFromDynamicConfiguration();
                services.AddServiceIdentityAzureTokenCredentialSourceFromClientIdentityConfiguration(configuration.ServiceIdentity);

                services.AddSingleton<UsingBlobStorageWithExplicitConfig>();
                services.AddSingleton<UsingCosmosWithExplicitConfig>();
            });
    }
}