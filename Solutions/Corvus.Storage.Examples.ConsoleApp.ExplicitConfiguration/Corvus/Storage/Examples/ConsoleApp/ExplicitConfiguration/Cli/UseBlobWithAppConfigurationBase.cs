// <copyright file="UseBlobWithAppConfigurationBase.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;

using Corvus.Storage.Azure.BlobStorage;
using Corvus.Storage.Examples.Azure.BlobStorage;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration.Cli
{
    /// <summary>
    /// Base class for Command line handlers for running the
    /// <see cref="UsingBlobStorageWithExplicitConfig"/> example with configuration loaded via
    /// IConfiguration.
    /// </summary>
    public abstract class UseBlobWithAppConfigurationBase : Command
    {
        /// <summary>
        /// Creates a <see cref="UseBlobWithAppConfigurationBase"/>.
        /// </summary>
        /// <param name="name">
        /// The command name.
        /// </param>
        protected UseBlobWithAppConfigurationBase(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Base class for the handler invoked by the System.Command hosting infrastructure, enabling use of DI.
        /// </summary>
        public abstract class RunBase : ICommandHandler
        {
            private readonly UsingBlobStorageWithExplicitConfig useBlobWithExplicitConfig;

            /// <summary>
            /// Creates a <see cref="RunBase"/> instance.
            /// </summary>
            /// <param name="useBlobWithExplicitConfig">
            /// The example that this command relies on.
            /// </param>
            /// <param name="configuration">
            /// Application configuration settings.
            /// </param>
            protected RunBase(
                UsingBlobStorageWithExplicitConfig useBlobWithExplicitConfig,
                ApplicationConfigurationSettings configuration)
            {
                this.useBlobWithExplicitConfig = useBlobWithExplicitConfig;
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

            /// <summary>
            /// To be called by derived types' overrides of <see cref="InvokeAsync(InvocationContext)"/>.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns>A task producing the program's exit code.</returns>
            protected async Task<int> InvokeAsyncImpl(InvocationContext context)
            {
                BlobContainerConfiguration containerConfiguration = this.GetConfiguration();

                // This is the hashed, encoded blob container name for the root tenant container
                // when using Corvus.Tenancy's blob storage implementation
                const string containerName = "cce7b3deef3998aad88f5f0116f922a94e7cb6c4";

                // This is the Service Tenants blob.
                const string blobId = "live/3633754ac4c9be44b55bfe791b1780f1";
                string data = await this.useBlobWithExplicitConfig.GetDataAsync(
                    containerConfiguration,
                    containerName,
                    blobId)
                    .ConfigureAwait(false);

                context.Console.Out.WriteLine($"Fetched blob {blobId} from {containerName}:");
                context.Console.Out.WriteLine(data);

                return 0;
            }

            /// <summary>
            /// Gets the <see cref="BlobContainerConfiguration"/> to use for this scenario.
            /// </summary>
            /// <returns>
            /// <para>
            /// The <see cref="BlobContainerConfiguration"/> to use.
            /// </para>
            /// </returns>
            protected abstract BlobContainerConfiguration GetConfiguration();
        }
    }
}