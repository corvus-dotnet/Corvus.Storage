// <copyright file="UseTableWithAppConfigurationBase.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;

using Azure;
using Azure.Data.Tables;

using Corvus.Storage.Azure.TableStorage;
using Corvus.Storage.Examples.Azure.Tables;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration.Cli;

/// <summary>
/// Base class for Command line handlers for running the
/// <see cref="UsingTablesWithExplicitConfig"/> example with configuration loaded via
/// IConfiguration.
/// </summary>
public abstract class UseTableWithAppConfigurationBase : Command
{
    /// <summary>
    /// Creates a <see cref="UseTableWithAppConfigurationBase"/>.
    /// </summary>
    /// <param name="name">
    /// The command name.
    /// </param>
    protected UseTableWithAppConfigurationBase(string name)
        : base(name)
    {
    }

    /// <summary>
    /// Base class for the handler invoked by the System.Command hosting infrastructure, enabling use of DI.
    /// </summary>
    public abstract class RunBase : ICommandHandler
    {
        private readonly UsingTablesWithExplicitConfig useTablesWithExplicitConfig;

        /// <summary>
        /// Creates a <see cref="RunBase"/> instance.
        /// </summary>
        /// <param name="useTablesWithExplicitConfig">
        /// The example that this command relies on.
        /// </param>
        /// <param name="configuration">
        /// Application configuration settings.
        /// </param>
        protected RunBase(
            UsingTablesWithExplicitConfig useTablesWithExplicitConfig,
            ApplicationConfigurationSettings configuration)
        {
            this.useTablesWithExplicitConfig = useTablesWithExplicitConfig;
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the application configuration settings.
        /// </summary>
        protected ApplicationConfigurationSettings Configuration { get; }

        /// <inheritdoc/>
        /// <remarks>
        /// Deriving types need to implement this themselves because otherwise, the System.CommandLine hosting
        /// stuff determines that the implementing type is RunBase and not the concrete type, and does
        /// the wrong thing for DI purposes. They can all just forward to <see cref="InvokeAsyncImpl(InvocationContext)"/>
        /// but every derived type needs to supply its own version because of how
        /// <see cref="HostingExtensions.UseCommandHandler{TCommand, THandler}(Microsoft.Extensions.Hosting.IHostBuilder)"/>
        /// works.
        /// </remarks>
        public abstract Task<int> InvokeAsync(InvocationContext context);

        /// <inheritdoc/>
        public int Invoke(InvocationContext context)
        {
            throw new System.NotImplementedException("Synchronous invocation not supported");
        }

        /// <summary>
        /// To be called by derived types' overrides of <see cref="InvokeAsync(InvocationContext)"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A task producing the program's exit code.</returns>
        protected async Task<int> InvokeAsyncImpl(InvocationContext context)
        {
            TableConfiguration tableConfiguration = this.GetConfiguration();

            const string tableName = "corvusstorageexampletesttable";

            const string partitionKey = "TestPartition";
            const string rowKey = "TestData";
            RowData dataIn = new()
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
            };

            Console.WriteLine("Setting row data");
            await this.useTablesWithExplicitConfig.UpsertDataAsync(
                tableConfiguration,
                tableName,
                dataIn);

            Console.WriteLine("Retrieving row data");

            RowData dataOut = await this.useTablesWithExplicitConfig.GetDataAsync<RowData>(
                tableConfiguration,
                tableName,
                partitionKey,
                rowKey)
                .ConfigureAwait(false);

            context.Console.Out.WriteLine($"Fetched row {partitionKey},{rowKey} from {tableName}:");
            context.Console.Out.WriteLine(dataOut.ToString());

            return 0;
        }

        /// <summary>
        /// Gets the <see cref="TableConfiguration"/> to use for this scenario.
        /// </summary>
        /// <returns>
        /// <para>
        /// The <see cref="TableConfiguration"/> to use.
        /// </para>
        /// </returns>
        protected abstract TableConfiguration GetConfiguration();

        // Using record mainly to get the automatic ToString.
        private record RowData : ITableEntity
        {
            // Due to serialization requirements, we can't satisfy nullability checks, but
            // we do want annotations, so we just suppress warnings.
#nullable disable warnings
            public string PartitionKey { get; set; }

            public string RowKey { get; set; }

            public DateTimeOffset? Timestamp { get; set; }
#nullable restore warnings

            // The ETag should be nullable because there are scenarios in which we can't
            // set it (e.g., creating new rows). But we can't annotate it as such because
            // C# then considers it to be mismatched with the ITableEntity.ETag definition.
            // So for this one, we disable annotations so C# doesn't think this will
            // necessarily be non-null.
#nullable disable annotations
            public ETag ETag { get; set; }
#nullable restore annotations
        }
    }
}