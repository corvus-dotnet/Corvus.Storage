// <copyright file="BlobConnectionStringInKeyVaultCommand.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.CommandLine.Invocation;
using System.Threading.Tasks;

using Corvus.Storage.Azure.BlobStorage;
using Corvus.Storage.Examples.Azure.BlobStorage;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration.Cli
{
    /// <summary>
    /// Command line handler for running the <see cref="UsingBlobStorageWithExplicitConfig"/>
    /// example with configuration that contains a connection string as plain text.
    /// </summary>
    public class BlobConnectionStringInKeyVaultCommand : UseBlobWithAppConfigurationBase
    {
        /// <summary>
        /// Creates a <see cref="BlobConnectionStringInPlainTextCommand"/>.
        /// </summary>
        public BlobConnectionStringInKeyVaultCommand()
            : base("connection-string-key-vault")
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
            /// <param name="useBlobWithExplicitConfig">The example that this command relies on.</param>
            /// <param name="configuration">Application configuration settings.</param>
            public Run(
                UsingBlobStorageWithExplicitConfig useBlobWithExplicitConfig,
                ApplicationConfigurationSettings configuration)
                : base(useBlobWithExplicitConfig, configuration)
            {
            }

            /// <inheritdoc/>
            public override Task<int> InvokeAsync(InvocationContext context) => this.InvokeAsyncImpl(context);

            /// <inheritdoc/>
            protected override BlobContainerConfiguration GetConfiguration() =>
                this.Configuration.BlobStorage.ConnectionStringInKeyVault;
        }
    }
}
