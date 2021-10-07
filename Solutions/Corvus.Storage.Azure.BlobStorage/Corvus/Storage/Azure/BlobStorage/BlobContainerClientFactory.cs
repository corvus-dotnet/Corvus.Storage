// <copyright file="BlobContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Identity;

using Corvus.Identity;

using global::Azure.Storage.Blobs;

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// A factory for a <see cref="BlobContainerClient"/>.
    /// </summary>
    internal class BlobContainerClientFactory :
        CachingStorageContextFactory<BlobContainerClient, BlobContainerConfiguration>,
        IBlobContainerSourceByConfiguration
    {
        private const string DevelopmentStorageConnectionString = "UseDevelopmentStorage=true";
        private readonly BlobContainerClientFactoryOptions? options;

        /// <summary>
        /// Creates a <see cref="BlobContainerClientFactory"/>.
        /// </summary>
        /// <param name="options">Configuration for the TenantBlobContainerClientFactory.</param>
        public BlobContainerClientFactory(BlobContainerClientFactoryOptions? options = null)
        {
            this.options = options;
        }

        /// <inheritdoc/>
        protected override async ValueTask<BlobContainerClient> CreateContextAsync(
            BlobContainerConfiguration configuration)
        {
            BlobServiceClient blobClient = await this.CreateBlockBlobClientAsync(configuration)
                .ConfigureAwait(false);

            return blobClient.GetBlobContainerClient(configuration.Container);
        }

        /// <inheritdoc/>
        protected override string GetCacheKeyForContext(BlobContainerConfiguration contextConfiguration)
        {
            // TODO: there are many options for configuration, and we need to work out a sound way
            // to reduce that reliably to a cache key.
            // This is a placeholder that kind of works but is bad for a lot of reasons
            return System.Text.Json.JsonSerializer.Serialize(contextConfiguration);
        }

        private async Task<BlobServiceClient> CreateBlockBlobClientAsync(BlobContainerConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // TODO: Handle all the options properly. Check for valid combination.
            if (!string.IsNullOrWhiteSpace(configuration.ConnectionStringPlainText))
            {
                return new BlobServiceClient(configuration.ConnectionStringPlainText);
            }

            if (configuration.ConnectionStringInKeyVault is not null)
            {
                ClientIdentitySourceTypes keyVaultClientIdentitySourceType =
                    configuration.ConnectionStringInKeyVault.VaultClientIdentity?.IdentitySourceType
                    ?? ClientIdentitySourceTypes.Managed;
                TokenCredential? keyVaultCredentials = keyVaultClientIdentitySourceType switch
                {
                    ClientIdentitySourceTypes.Managed => new ManagedIdentityCredential(),
                    ClientIdentitySourceTypes.AzureIdentityDefaultAzureCredential => new DefaultAzureCredential(),
                    _ => null,
                };
                if (keyVaultCredentials is not null)
                {
                    string connectionString = await this.GetKeyVaultSecretAsync(
                        keyVaultCredentials,
                        configuration.ConnectionStringInKeyVault.VaultName,
                        configuration.ConnectionStringInKeyVault.SecretName)
                        .ConfigureAwait(false);

                    return new BlobServiceClient(connectionString);
                }
            }

            throw new ArgumentException("Invalid configuration", nameof(configuration));

            // This is the old behaviour. We will need to support this to enable use of legacy configuration,
            // but may want to make that something switchable.
            ////// Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            ////if (string.IsNullOrEmpty(configuration.AccountName) || configuration.AccountName!.Equals(DevelopmentStorageConnectionString))
            ////{
            ////    return new BlobServiceClient(DevelopmentStorageConnectionString);
            ////}
            ////else if (string.IsNullOrWhiteSpace(configuration.AccountKeySecretName))
            ////{
            ////    // As the documentation for BlobStorageConfiguration.AccountName says:
            ////    //  "If the account key secret name is empty, then this should contain
            ////    //   a complete connection string."
            ////    return new BlobServiceClient(configuration.AccountName);
            ////}
            ////else
            ////{
            ////    string accountKey = await this.GetKeyVaultSecretAsync(
            ////        this.options?.AzureServicesAuthConnectionString,
            ////        configuration.KeyVaultName!,
            ////        configuration.AccountKeySecretName!).ConfigureAwait(false);
            ////    var credentials = new StorageSharedKeyCredential(configuration.AccountName, accountKey);
            ////    return new BlobServiceClient(
            ////        new Uri($"https://{configuration.AccountName}.blob.core.windows.net"),
            ////        credentials);
            ////}
        }
    }
}
