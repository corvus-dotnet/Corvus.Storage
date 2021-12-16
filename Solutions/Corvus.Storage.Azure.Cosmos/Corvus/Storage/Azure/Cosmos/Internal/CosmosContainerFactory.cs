// <copyright file="CosmosContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Azure.Core;

using Corvus.Identity.ClientAuthentication.Azure;

using Microsoft.Azure.Cosmos;

namespace Corvus.Storage.Azure.Cosmos.Internal
{
    /// <summary>
    /// A factory for a <see cref="Container"/>.
    /// </summary>
    internal class CosmosContainerFactory :
        TwoLevelCachingStorageContextFactory<CosmosClient, Container, CosmosContainerConfiguration, CosmosClientOptions>,
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
        protected override ValueTask<Container> CreateContextAsync(
            CosmosClient parent,
            CosmosContainerConfiguration configuration,
            CosmosClientOptions? connectionOptions,
            CancellationToken cancellationToken)
        {
            Database db = parent.GetDatabase(configuration.Database);
            return new ValueTask<Container>(db.GetContainer(configuration.Container));
        }

        /// <inheritdoc/>
        protected override async ValueTask<CosmosClient> CreateParentContextAsync(
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
            CosmosClientOptions? connectionOptions,
            CancellationToken cancellationToken)
        {
            this.InvalidateCredentials(contextConfiguration.ConnectionStringInKeyVault?.VaultClientIdentity);
            this.InvalidateCredentials(contextConfiguration.AccessKeyInKeyVault?.VaultClientIdentity);
        }

        private static ValueTask<CosmosClient> ClientFromConnectionStringAsPlainText(
            CosmosContainerConfiguration configuration, CosmosClientOptions? clientOptions)
        {
            return new ValueTask<CosmosClient>(
                new CosmosClient(configuration.ConnectionStringPlainText, clientOptions));
        }

        private static ValueTask<CosmosClient> AccountUriAndAccessKeyAsPlainText(
            CosmosContainerConfiguration configuration,
            CosmosClientOptions? clientOptions)
        {
            return new ValueTask<CosmosClient>(
                new CosmosClient(
                    configuration.AccountUri!,
                    configuration.AccessKeyPlainText!,
                    clientOptions));
        }

        private async ValueTask<CosmosClient> CreateCosmosClientAsync(
            CosmosContainerConfiguration configuration,
            CosmosClientOptions? clientOptions,
            CancellationToken cancellationToken)
        {
            string? validationMessage = CosmosContainerConfigurationValidation.Validate(
                configuration, out CosmosContainerConfigurationTypes configurationType);
            if (validationMessage is not null)
            {
                throw new ArgumentException(
                    "Invalid CosmosContainerConfiguration: " + validationMessage,
                    nameof(configuration));
            }

            ValueTask<CosmosClient> r = configurationType switch
            {
                CosmosContainerConfigurationTypes.ConnectionStringAsPlainText =>
                    ClientFromConnectionStringAsPlainText(configuration, clientOptions),

                CosmosContainerConfigurationTypes.ConnectionStringInKeyVault =>
                    this.ClientFromConnectionStringInKeyVault(configuration, clientOptions, cancellationToken),

                CosmosContainerConfigurationTypes.AccountUriAndAccessKeyAsPlainText =>
                    AccountUriAndAccessKeyAsPlainText(configuration, clientOptions),

                CosmosContainerConfigurationTypes.AccountUriAndAccessKeyInKeyVault =>
                    this.AccountUriAndAccessKeyInKeyVault(
                        configuration, clientOptions, cancellationToken),

                CosmosContainerConfigurationTypes.AccountUriAndClientIdentity =>
                    this.ClientFromAccountUriAndClientIdentity(
                        configuration, clientOptions, cancellationToken),

                _ => throw new InvalidOperationException($"Unknown configuration type {configurationType}"),
            };

            return await r.ConfigureAwait(false);
        }

        private async ValueTask<CosmosClient> ClientFromConnectionStringInKeyVault(
            CosmosContainerConfiguration configuration,
            CosmosClientOptions? clientOptions,
            CancellationToken cancellationToken)
        {
            string? connectionString =
                await this.GetKeyVaultSecretFromConfigAsync(
                    configuration.ConnectionStringInKeyVault!,
                    cancellationToken)
                    .ConfigureAwait(false);
            return new CosmosClient(
                    connectionString,
                    clientOptions);
        }

        private async ValueTask<CosmosClient> AccountUriAndAccessKeyInKeyVault(
            CosmosContainerConfiguration configuration,
            CosmosClientOptions? clientOptions,
            CancellationToken cancellationToken)
        {
            string? accessKey = await this.GetKeyVaultSecretFromConfigAsync(
                configuration.AccessKeyInKeyVault!,
                cancellationToken)
                .ConfigureAwait(false);

            if (accessKey is null)
            {
                throw new InvalidOperationException($"Failed to get secret {configuration.AccessKeyInKeyVault!.SecretName} from {configuration.AccessKeyInKeyVault!.VaultName}");
            }

            return new CosmosClient(
                configuration.AccountUri!,
                accessKey,
                clientOptions);
        }

        private async ValueTask<CosmosClient> ClientFromAccountUriAndClientIdentity(
            CosmosContainerConfiguration configuration,
            CosmosClientOptions? clientOptions,
            CancellationToken cancellationToken)
        {
            IAzureTokenCredentialSource credentialSource =
                await this.AzureTokenCredentialSourceFromConfig.CredentialSourceForConfigurationAsync(
                    configuration.ClientIdentity!,
                    cancellationToken)
                .ConfigureAwait(false);
            TokenCredential tokenCredential = await credentialSource.GetTokenCredentialAsync(cancellationToken)
                .ConfigureAwait(false);
            return new CosmosClient(
                configuration.AccountUri!,
                tokenCredential,
                clientOptions);
        }
    }
}