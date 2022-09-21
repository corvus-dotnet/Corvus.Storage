// <copyright file="UseCosmosWithAppConfigurationBase.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;

using Corvus.Storage.Azure.Cosmos;
using Corvus.Storage.Examples.Azure.Cosmos;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration.Cli
{
    /// <summary>
    /// Base class for Command line handlers for running the
    /// <see cref="UsingCosmosWithExplicitConfig"/> example with configuration loaded via
    /// IConfiguration.
    /// </summary>
    public abstract class UseCosmosWithAppConfigurationBase : Command
    {
        /// <summary>
        /// Creates a <see cref="UseBlobWithAppConfigurationBase"/>.
        /// </summary>
        /// <param name="name">
        /// The command name.
        /// </param>
        protected UseCosmosWithAppConfigurationBase(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Base class for the handler invoked by the System.Command hosting infrastructure, enabling use of DI.
        /// </summary>
        public abstract class RunBase : ICommandHandler
        {
            private readonly UsingCosmosWithExplicitConfig useCosmosWithExplicitConfig;

            /// <summary>
            /// Creates a <see cref="RunBase"/> instance.
            /// </summary>
            /// <param name="useCosmosWithExplicitConfig">
            /// The example that this command relies on.
            /// </param>
            /// <param name="configuration">
            /// Application configuration settings.
            /// </param>
            protected RunBase(
                UsingCosmosWithExplicitConfig useCosmosWithExplicitConfig,
                ApplicationConfigurationSettings configuration)
            {
                this.useCosmosWithExplicitConfig = useCosmosWithExplicitConfig;
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
                CosmosContainerConfiguration containerConfiguration = this.GetConfiguration();

                // This is the container name typically created by the Azure Portal if you ask it
                // to build you an example DB and container in the Quick Start panel.
                const string containerName = "Items";

                // This is the ID of an item that the code in the Quick Start panel crfeates
                const string docId = "Andersen.1";
                string data = await this.useCosmosWithExplicitConfig.GetDataAsync(
                    containerConfiguration,
                    containerName,
                    docId,
                    "Andersen")
                    .ConfigureAwait(false);

                context.Console.Out.WriteLine($"Fetched blob {docId} from {containerName}:");
                context.Console.Out.WriteLine(data);

                return 0;
            }

            /// <summary>
            /// Gets the <see cref="CosmosContainerConfiguration"/> to use for this scenario.
            /// </summary>
            /// <returns>
            /// <para>
            /// The <see cref="CosmosContainerConfiguration"/> to use.
            /// </para>
            /// </returns>
            protected abstract CosmosContainerConfiguration GetConfiguration();
        }
    }
}