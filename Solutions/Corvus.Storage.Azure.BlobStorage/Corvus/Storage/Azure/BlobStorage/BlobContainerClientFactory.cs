// <copyright file="BlobContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Identity;

using Corvus.Identity;
using Corvus.Identity.ClientAuthentication.Azure;

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
        private readonly IAzureTokenCredentialSourceFromDynamicConfiguration azureTokenCredentialSource;
        private readonly IServiceIdentityAzureTokenCredentialSource serviceIdAzureTokenCredentialSource;

        ////private readonly BlobContainerClientFactoryOptions? options;

        /// <summary>
        /// Creates a <see cref="BlobContainerClientFactory"/>.
        /// </summary>
        /// <param name="azureTokenCredentialSource">
        /// Provides <see cref="TokenCredential"/>s in exchange for
        /// <see cref="ClientIdentityConfiguration"/>s.
        /// </param>
        /// <param name="serviceIdAzureTokenCredentialSource">
        /// Provides <see cref="TokenCredential"/>s representing the service identity.
        /// </param>
        /////// <param name="options">Configuration for the TenantBlobContainerClientFactory.</param>
        public BlobContainerClientFactory(
            IAzureTokenCredentialSourceFromDynamicConfiguration azureTokenCredentialSource,
            IServiceIdentityAzureTokenCredentialSource serviceIdAzureTokenCredentialSource)
            ////BlobContainerClientFactoryOptions? options = null)
        {
            ////this.options = options;
            this.azureTokenCredentialSource = azureTokenCredentialSource;
            this.serviceIdAzureTokenCredentialSource = serviceIdAzureTokenCredentialSource;
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
                // If no identity for the key vault is specified we use the ambient service
                // identity. Otherwise, we use the identity configuration supplied.
                IAzureTokenCredentialSource credentialSource = configuration.ConnectionStringInKeyVault.VaultClientIdentity is null
                    ? this.serviceIdAzureTokenCredentialSource
                    : await this.azureTokenCredentialSource
                        .CredentialSourceForConfigurationAsync(configuration.ConnectionStringInKeyVault.VaultClientIdentity)
                        .ConfigureAwait(false);
                TokenCredential? keyVaultCredentials = await credentialSource.GetTokenCredentialAsync()
                    .ConfigureAwait(false);
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
