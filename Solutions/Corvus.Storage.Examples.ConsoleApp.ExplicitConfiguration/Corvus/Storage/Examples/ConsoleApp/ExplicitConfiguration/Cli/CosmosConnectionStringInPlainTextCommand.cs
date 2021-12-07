// <copyright file="CosmosConnectionStringInPlainTextCommand.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.CommandLine.Invocation;
using System.Threading.Tasks;

using Corvus.Storage.Azure.Cosmos;
using Corvus.Storage.Examples.Azure.Cosmos;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration.Cli
{
    /// <summary>
    /// Command line handler for running the <see cref="UsingCosmosWithExplicitConfig"/>
    /// example with configuration that contains a connection string as plain text.
    /// </summary>
    public class CosmosConnectionStringInPlainTextCommand : UseCosmosWithAppConfigurationBase
    {
        /// <summary>
        /// Creates a <see cref="BlobConnectionStringInPlainTextCommand"/>.
        /// </summary>
        public CosmosConnectionStringInPlainTextCommand()
            : base("connection-string-plain-text")
        {
        }

        /// <summary>
        /// The handler invoked by the System.Command hosting infrastructure, enabling use of DI.
        /// </summary>
        public class Run : RunBase
        {
            /// <summary>
            /// Creates a <see cref="Run"/> instance.
            /// </summary>
            /// <param name="useCosmosWithExplicitConfig">The example that this command relies on.</param>
            /// <param name="configuration">Application configuration settings.</param>
            public Run(
                UsingCosmosWithExplicitConfig useCosmosWithExplicitConfig,
                ApplicationConfigurationSettings configuration)
                : base(useCosmosWithExplicitConfig, configuration)
            {
            }

            /// <inheritdoc/>
            public override Task<int> InvokeAsync(InvocationContext context) => this.InvokeAsyncImpl(context);

            /// <inheritdoc/>
            protected override CosmosContainerConfiguration GetConfiguration() =>
                this.Configuration.Cosmos.ConnectionStringInConfiguration;
        }
    }
}
