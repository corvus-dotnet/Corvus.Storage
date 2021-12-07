// <copyright file="AppCommandBuilder.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;

using Microsoft.Extensions.Hosting;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration.Cli
{
    internal static class AppCommandBuilder
    {
        public static Parser MakeRootCommand(Action<IHostBuilder> configureHost)
        {
            var rootCommand = new RootCommand("Corvus.Storage examples using explicit configuration")
            {
                new Command("blob", "Azure Blob Storage commands")
                {
                    new BlobAccessKeyInKeyVaultCommand(),
                    new BlobConnectionStringInPlainTextCommand(),
                    new BlobConnectionStringInKeyVaultCommand(),
                },
                new Command("cosmos", "Azure Cosmos commands")
                {
                    new CosmosConnectionStringInPlainTextCommand(),
                    new CosmosConnectionStringInKeyVaultCommand(),
                },
            };

            // Using generic host https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host
            // because it sets up configuration and DI in the usual way.
            return new CommandLineBuilder(rootCommand)
                .UseHost(
                args => Host.CreateDefaultBuilder(args),
                builder =>
                {
                    ////builder
                    configureHost(builder);
                    builder.UseCommandHandler<BlobConnectionStringInPlainTextCommand, BlobConnectionStringInPlainTextCommand.Run>();
                    builder.UseCommandHandler<BlobConnectionStringInKeyVaultCommand, BlobConnectionStringInKeyVaultCommand.Run>();
                    builder.UseCommandHandler<BlobAccessKeyInKeyVaultCommand, BlobAccessKeyInKeyVaultCommand.Run>();
                    builder.UseCommandHandler<CosmosConnectionStringInPlainTextCommand, CosmosConnectionStringInPlainTextCommand.Run>();
                    builder.UseCommandHandler<CosmosConnectionStringInKeyVaultCommand, CosmosConnectionStringInKeyVaultCommand.Run>();
                })
                .Build();
        }
    }
}