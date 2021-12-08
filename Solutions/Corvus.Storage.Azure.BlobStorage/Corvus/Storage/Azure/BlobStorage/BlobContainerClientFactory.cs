// <copyright file="BlobContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;

using Corvus.Identity.ClientAuthentication.Azure;

using Microsoft.Extensions.DependencyInjection;

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// A factory for a <see cref="BlobContainerClient"/>.
    /// </summary>
    internal class BlobContainerClientFactory :
        CachingStorageContextFactory<BlobContainerClient, BlobContainerConfiguration, BlobClientOptions>,
        IBlobContainerSourceFromDynamicConfiguration
    {
        private readonly IAzureTokenCredentialSourceFromDynamicConfiguration azureTokenCredentialSource;
        private readonly IServiceProvider serviceProvider;

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
        {
            this.azureTokenCredentialSource = azureTokenCredentialSource;
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        protected override async ValueTask<BlobContainerClient> CreateContextAsync(
            BlobContainerConfiguration configuration,
            BlobClientOptions? blobClientOptions)
        {
            BlobServiceClient blobClient = await this.CreateBlobServiceClientAsync(configuration, blobClientOptions)
                .ConfigureAwait(false);

            return blobClient.GetBlobContainerClient(configuration.Container);
        }

        /// <inheritdoc/>
        protected override string GetCacheKeyForContext(BlobContainerConfiguration contextConfiguration)
        {
            // TODO: there are many options for configuration, and we need to work out a sound way
            // to reduce that reliably to a cache key.
            // This is a placeholder that kind of works but is bad for a lot of reasons
            // https://github.com/corvus-dotnet/Corvus.Storage/issues/3
            return System.Text.Json.JsonSerializer.Serialize(contextConfiguration);
        }

        private async Task<BlobServiceClient> CreateBlobServiceClientAsync(
            BlobContainerConfiguration configuration,
            BlobClientOptions? blobClientOptions)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // TODO: Handle all the options properly. Check for valid combination.
            if (!string.IsNullOrWhiteSpace(configuration.ConnectionStringPlainText))
            {
                return new BlobServiceClient(configuration.ConnectionStringPlainText, blobClientOptions);
            }

            if (configuration.ConnectionStringInKeyVault is not null)
            {
                string? connectionString = await this.GetKeyVaultSecretFromConfigAsync(configuration.ConnectionStringInKeyVault).ConfigureAwait(false);
                if (connectionString is not null)
                {
                    return new BlobServiceClient(connectionString, blobClientOptions);
                }
            }
            else if (configuration.AccessKeyInKeyVault is not null && configuration.AccountName is not null)
            {
                string? accessKey = await this.GetKeyVaultSecretFromConfigAsync(configuration.AccessKeyInKeyVault).ConfigureAwait(false);
                if (accessKey is not null)
                {
                    return new BlobServiceClient(
                        new Uri($"https://{configuration.AccountName}.blob.core.windows.net"),
                        new StorageSharedKeyCredential(configuration.AccountName, accessKey),
                        blobClientOptions);
                }
            }

            throw new ArgumentException("Invalid configuration", nameof(configuration));
        }

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