// <copyright file="CosmosContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Azure.Core;

using Corvus.Identity.ClientAuthentication.Azure;

using Microsoft.Azure.Cosmos;

namespace Corvus.Storage.Azure.Cosmos
{
    /// <summary>
    /// A factory for a <see cref="Container"/>.
    /// </summary>
    internal class CosmosContainerFactory :
        TwoLevelCachingStorageContextFactory<CosmosClient, IAzureTokenCredentialSource?, Container, IAzureTokenCredentialSource?, CosmosContainerConfiguration, CosmosClientOptions>,
        ICosmosContainerSourceFromDynamicConfiguration
    {
        /// <summary>
        /// Creates a <see cref="CosmosContainerFactory"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// Required by the base class.
        /// </param>
        public CosmosContainerFactory(
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <inheritdoc/>
        protected override ValueTask<(Container, IAzureTokenCredentialSource?)> CreateContextAsync(
            CosmosClient parent,
            CosmosContainerConfiguration configuration,
            CosmosClientOptions? connectionOptions,
            CancellationToken cancellationToken)
        {
            Database db = parent.GetDatabase(configuration.Database);
            return new ValueTask<(Container, IAzureTokenCredentialSource?)>((db.GetContainer(configuration.Container), null));
        }

        /// <inheritdoc/>
        protected override async ValueTask<(CosmosClient, IAzureTokenCredentialSource?)> CreateParentContextAsync(
            CosmosContainerConfiguration configuration,
            CosmosClientOptions? clientOptions,
            CancellationToken cancellationToken)
        {
            return await this.CreateCosmosClientAsync(configuration, clientOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string GetCacheKeyForConfiguration(CosmosContainerConfiguration contextConfiguration)
        {
            // TODO: there are many options for configuration, and we need to work out a sound way
            // to reduce that reliably to a cache key.
            // This is a placeholder that kind of works but is bad for a lot of reasons
            // https://github.com/corvus-dotnet/Corvus.Storage/issues/3
            return System.Text.Json.JsonSerializer.Serialize(contextConfiguration);
        }

        /// <inheritdoc/>
        protected override string GetCacheKeyForParentConfiguration(CosmosContainerConfiguration contextConfiguration)
        {
            CosmosContainerConfiguration nonContainerSpecificConfiguration = contextConfiguration with
            {
                // StyleCop thinks this is a local call. It's probably because it doesn't
                // understand with 'with' syntax. We should be able to remove this at
                // some point, because StyleCop shouldn't be fazed by this.
#pragma warning disable SA1101 // Prefix local calls with this
                Container = null,
#pragma warning restore SA1101 // Prefix local calls with this
            };

            return this.GetCacheKeyForConfiguration(nonContainerSpecificConfiguration);
        }

        /// <inheritdoc/>
        protected override void InvalidateForConfiguration(
            CosmosContainerConfiguration contextConfiguration,
            IAzureTokenCredentialSource? tokenCredentialSource,
            CosmosClientOptions? connectionOptions,
            CancellationToken cancellationToken)
        {
            this.InvalidateCredentials(contextConfiguration.ConnectionStringInKeyVault?.VaultClientIdentity);
            this.InvalidateCredentials(contextConfiguration.AccessKeyInKeyVault?.VaultClientIdentity);
        }

        private async ValueTask<(CosmosClient, IAzureTokenCredentialSource?)> CreateCosmosClientAsync(
            CosmosContainerConfiguration configuration,
            CosmosClientOptions? clientOptions,
            CancellationToken cancellationToken)
        {
            // TODO: Handle all the options properly. Check for valid combination.
            if (!string.IsNullOrWhiteSpace(configuration.ConnectionStringPlainText))
            {
                return (new CosmosClient(configuration.ConnectionStringPlainText, clientOptions), null);
            }

            if (configuration.ConnectionStringInKeyVault is not null)
            {
                (string? connectionString, IAzureTokenCredentialSource tokenCredentialSource) = await this.GetKeyVaultSecretFromConfigAsync(
                    configuration.ConnectionStringInKeyVault,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (connectionString is not null)
                {
                    return (new CosmosClient(connectionString, clientOptions), tokenCredentialSource);
                }
            }
            else if (configuration.AccessKeyInKeyVault is not null && configuration.AccountUri is not null)
            {
                (string? accessKey, IAzureTokenCredentialSource tokenCredentialSource) = await this.GetKeyVaultSecretFromConfigAsync(
                    configuration.AccessKeyInKeyVault,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (accessKey is not null)
                {
                    return (
                        new CosmosClient(
                            configuration.AccountUri,
                            accessKey,
                            clientOptions),
                        tokenCredentialSource);
                }
            }

            throw new ArgumentException("Invalid configuration", nameof(configuration));
        }
    }
}