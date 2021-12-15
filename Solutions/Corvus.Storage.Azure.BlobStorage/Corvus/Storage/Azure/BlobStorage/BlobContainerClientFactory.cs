// <copyright file="BlobContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;

using Corvus.Identity.ClientAuthentication.Azure;
using Corvus.Storage.Azure.BlobStorage.Internal;

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// A factory for a <see cref="BlobContainerClient"/>.
    /// </summary>
    internal class BlobContainerClientFactory :
        CachingStorageContextFactory<BlobContainerClient, IAzureTokenCredentialSource?, BlobContainerConfiguration, BlobClientOptions>,
        IBlobContainerSourceFromDynamicConfiguration
    {
        private readonly IAzureTokenCredentialSourceFromDynamicConfiguration azureTokenCredentialSourceFromConfig;

        /// <summary>
        /// Creates a <see cref="BlobContainerClientFactory"/>.
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
        public BlobContainerClientFactory(
            IAzureTokenCredentialSourceFromDynamicConfiguration azureTokenCredentialSource,
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this.azureTokenCredentialSourceFromConfig = azureTokenCredentialSource;
        }

        /// <inheritdoc/>
        protected override async ValueTask<(BlobContainerClient Context, IAzureTokenCredentialSource? ContextData)> CreateContextAsync(
            BlobContainerConfiguration configuration,
            BlobClientOptions? blobClientOptions,
            CancellationToken cancellationToken)
        {
            (BlobServiceClient blobClient, IAzureTokenCredentialSource? tokenSource) =
                await this.CreateBlobServiceClientAsync(
                    configuration,
                    blobClientOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            return (blobClient.GetBlobContainerClient(configuration.Container), tokenSource);
        }

        /// <inheritdoc/>
        protected override string GetCacheKeyForConfiguration(BlobContainerConfiguration contextConfiguration)
        {
            // TODO: there are many options for configuration, and we need to work out a sound way
            // to reduce that reliably to a cache key.
            // This is a placeholder that kind of works but is bad for a lot of reasons
            // https://github.com/corvus-dotnet/Corvus.Storage/issues/3
            return System.Text.Json.JsonSerializer.Serialize(contextConfiguration);
        }

        /// <inheritdoc/>
        protected override async ValueTask InvalidateForConfigurationAsync(
            BlobContainerConfiguration configuration,
            IAzureTokenCredentialSource? azureTokenSource,
            BlobClientOptions? connectionOptions,
            CancellationToken cancellationToken)
        {
            if (azureTokenSource is not null)
            {
                await azureTokenSource.GetReplacementForFailedTokenCredentialAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private static Uri AccountUri(string accountName)
            => new ($"https://{accountName}.blob.core.windows.net");

        private static ValueTask<(BlobServiceClient Client, IAzureTokenCredentialSource? TokenSource)> ClientFromConnectionStringAsPlainText(
            BlobContainerConfiguration configuration, BlobClientOptions? blobClientOptions)
        {
            return new ValueTask<(BlobServiceClient, IAzureTokenCredentialSource?)>(
                (new BlobServiceClient(configuration.ConnectionStringPlainText, blobClientOptions), null));
        }

        private static ValueTask<(BlobServiceClient Client, IAzureTokenCredentialSource? TokenSource)> AccountNameAndAccessKeyAsPlainText(
            BlobContainerConfiguration configuration,
            BlobClientOptions? blobClientOptions)
        {
            return new ValueTask<(BlobServiceClient, IAzureTokenCredentialSource?)>(
                (new BlobServiceClient(
                    AccountUri(configuration.AccountName!),
                    new StorageSharedKeyCredential(configuration.AccountName, configuration.AccessKeyPlainText!),
                    blobClientOptions),
                null));
        }

        private async Task<(BlobServiceClient Client, IAzureTokenCredentialSource? TokenSource)> CreateBlobServiceClientAsync(
            BlobContainerConfiguration configuration,
            BlobClientOptions? blobClientOptions,
            CancellationToken cancellationToken)
        {
            string? validationMessage = BlobContainerConfigurationValidation.Validate(
                configuration, out BlobContainerConfigurationTypes configurationType);
            if (validationMessage is not null)
            {
                throw new ArgumentException(
                    "Invalid BlobContainerConfiguration: " + validationMessage,
                    nameof(configuration));
            }

            ValueTask<(BlobServiceClient, IAzureTokenCredentialSource?)> r = configurationType switch
            {
                BlobContainerConfigurationTypes.ConnectionStringAsPlainText =>
                    ClientFromConnectionStringAsPlainText(configuration, blobClientOptions),

                BlobContainerConfigurationTypes.ConnectionStringInKeyVault =>
                    this.ClientFromConnectionStringInKeyVault(configuration, blobClientOptions, cancellationToken),

                BlobContainerConfigurationTypes.AccountNameAndAccessKeyAsPlainText =>
                    AccountNameAndAccessKeyAsPlainText(configuration, blobClientOptions),

                BlobContainerConfigurationTypes.AccountNameAndAccessKeyInKeyVault =>
                    this.AccountNameAndAccessKeyInKeyVault(
                        configuration, blobClientOptions, cancellationToken),

                BlobContainerConfigurationTypes.AccountNameAndClientIdentity =>
                    this.ClientFromAccountNameAndClientIdentity(
                        configuration, blobClientOptions, cancellationToken),

                _ => throw new InvalidOperationException($"Unknown configuration type {configurationType}"),
            };

            return await r.ConfigureAwait(false);
        }

        private async ValueTask<(BlobServiceClient Client, IAzureTokenCredentialSource? TokenSource)> ClientFromConnectionStringInKeyVault(
            BlobContainerConfiguration configuration,
            BlobClientOptions? blobClientOptions,
            CancellationToken cancellationToken)
        {
            (string? connectionString, IAzureTokenCredentialSource tokenSource) =
                await this.GetKeyVaultSecretFromConfigAsync(
                    configuration.ConnectionStringInKeyVault!,
                    cancellationToken)
                    .ConfigureAwait(false);
            return (
                new BlobServiceClient(
                    connectionString,
                    blobClientOptions),
                tokenSource);
        }

        private async ValueTask<(BlobServiceClient Client, IAzureTokenCredentialSource? TokenSource)> AccountNameAndAccessKeyInKeyVault(
            BlobContainerConfiguration configuration,
            BlobClientOptions? blobClientOptions,
            CancellationToken cancellationToken)
        {
            (string? accessKey, IAzureTokenCredentialSource tokenSource) = await this.GetKeyVaultSecretFromConfigAsync(
                configuration.AccessKeyInKeyVault!,
                cancellationToken)
                .ConfigureAwait(false);

            if (accessKey is null)
            {
                throw new InvalidOperationException($"Failed to get secret {configuration.AccessKeyInKeyVault!.SecretName} from {configuration.AccessKeyInKeyVault!.VaultName}");
            }

            return (
                new BlobServiceClient(
                    AccountUri(configuration.AccountName!),
                    new StorageSharedKeyCredential(configuration.AccountName, accessKey),
                    blobClientOptions),
                tokenSource);
        }

        private async ValueTask<(BlobServiceClient Client, IAzureTokenCredentialSource? TokenSource)> ClientFromAccountNameAndClientIdentity(
            BlobContainerConfiguration configuration,
            BlobClientOptions? blobClientOptions,
            CancellationToken cancellationToken)
        {
            IAzureTokenCredentialSource credentialSource =
                await this.azureTokenCredentialSourceFromConfig.CredentialSourceForConfigurationAsync(
                    configuration.ClientIdentity!,
                    cancellationToken)
                .ConfigureAwait(false);
            TokenCredential tokenCredential = await credentialSource.GetTokenCredentialAsync(cancellationToken)
                .ConfigureAwait(false);
            return (
                new BlobServiceClient(
                    AccountUri(configuration.AccountName!),
                    tokenCredential,
                    blobClientOptions),
                credentialSource);
        }
    }
}