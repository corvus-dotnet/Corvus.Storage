// <copyright file="CosmosContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Azure.Core;

using Corvus.Identity.ClientAuthentication.Azure;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Corvus.Storage.Azure.Cosmos
{
    /// <summary>
    /// A factory for a <see cref="Container"/>.
    /// </summary>
    internal class CosmosContainerFactory :
        TwoLevelCachingStorageContextFactory<CosmosClient, Container, CosmosContainerConfiguration, CosmosClientOptions>,
        ICosmosContainerSourceFromDynamicConfiguration
    {
        private readonly IAzureTokenCredentialSourceFromDynamicConfiguration azureTokenCredentialSource;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Creates a <see cref="CosmosContainerFactory"/>.
        /// </summary>
        /// <param name="azureTokenCredentialSource">
        /// Provides <see cref="TokenCredential"/>s in exchange for
        /// <see cref="ClientIdentityConfiguration"/>s.
        /// </param>
        /// <param name="serviceProvider">
        /// Provides access to dependencies that are only needed in certain scenarios, and which
        /// we don't want to cause a DI initialization failure for if they are absent. (We depend
        /// on <see cref="IServiceIdentityAzureTokenCredentialSource"/>, but only in certain
        /// scenarios.)
        /// </param>
        public CosmosContainerFactory(
            IAzureTokenCredentialSourceFromDynamicConfiguration azureTokenCredentialSource,
            IServiceProvider serviceProvider)
        {
            this.azureTokenCredentialSource = azureTokenCredentialSource;
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        protected override ValueTask<Container> CreateContextAsync(
            CosmosClient parent, CosmosContainerConfiguration configuration, CosmosClientOptions? connectionOptions)
        {
            Database db = parent.GetDatabase(configuration.Database);
            return new ValueTask<Container>(db.GetContainer(configuration.Container));
        }

        /// <inheritdoc/>
        protected override async ValueTask<CosmosClient> CreateParentContextAsync(
            CosmosContainerConfiguration configuration, CosmosClientOptions? clientOptions)
        {
            CosmosClient client = await this.CreateCosmosClientAsync(configuration, clientOptions)
                .ConfigureAwait(false);

            return client;
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

        private async ValueTask<CosmosClient> CreateCosmosClientAsync(
            CosmosContainerConfiguration configuration,
            CosmosClientOptions? clientOptions)
        {
            // TODO: Handle all the options properly. Check for valid combination.
            if (!string.IsNullOrWhiteSpace(configuration.ConnectionStringPlainText))
            {
                return new CosmosClient(configuration.ConnectionStringPlainText, clientOptions);
            }

            if (configuration.ConnectionStringInKeyVault is not null)
            {
                string? connectionString = await this.GetKeyVaultSecretFromConfigAsync(configuration.ConnectionStringInKeyVault).ConfigureAwait(false);
                if (connectionString is not null)
                {
                    return new CosmosClient(connectionString, clientOptions);
                }
            }
            else if (configuration.AccessKeyInKeyVault is not null && configuration.AccountUri is not null)
            {
                string? accessKey = await this.GetKeyVaultSecretFromConfigAsync(configuration.AccessKeyInKeyVault).ConfigureAwait(false);
                if (accessKey is not null)
                {
                    return new CosmosClient(
                        configuration.AccountUri,
                        accessKey,
                        clientOptions);
                }
            }

            throw new ArgumentException("Invalid configuration", nameof(configuration));
        }

        // TODO:
        //  1: move this into somewhere shared
        //  2: add some means of triggering a re-read to support key rotation
        private async Task<string?> GetKeyVaultSecretFromConfigAsync(KeyVaultSecretConfiguration secretConfiguration)
        {
            // If no identity for the key vault is specified we use the ambient service
            // identity. Otherwise, we use the identity configuration supplied.
            IAzureTokenCredentialSource credentialSource = secretConfiguration.VaultClientIdentity is null
                ? this.serviceProvider.GetRequiredService<IServiceIdentityAzureTokenCredentialSource>()
                : await this.azureTokenCredentialSource
                    .CredentialSourceForConfigurationAsync(secretConfiguration.VaultClientIdentity)
                    .ConfigureAwait(false);
            TokenCredential? keyVaultCredentials = await credentialSource.GetTokenCredentialAsync()
                .ConfigureAwait(false);
            if (keyVaultCredentials is not null)
            {
                string secret = await this.GetKeyVaultSecretAsync(
                    keyVaultCredentials,
                    secretConfiguration.VaultName,
                    secretConfiguration.SecretName)
                    .ConfigureAwait(false);

                return secret;
            }

            return null;
        }
    }
}