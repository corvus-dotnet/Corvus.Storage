// <copyright file="BlobContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage
{
    using System;
    using System.Threading.Tasks;

    using Corvus.Storage;

    using global::Azure.Storage;
    using global::Azure.Storage.Blobs;

    /// <summary>
    /// A factory for a <see cref="BlobContainerClient"/>.
    /// </summary>
    internal class BlobContainerClientFactory :
        CachingStorageContextFactory<BlobContainerClient, BlobContainerConfiguration>,
        IBlobContainerClientFactory
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
            string contextName,
            BlobContainerConfiguration configuration)
        {
            BlobServiceClient blobClient = await this.CreateBlockBlobClientAsync(configuration)
                .ConfigureAwait(false);

            return blobClient.GetBlobContainerClient(configuration.Container);
        }

        private async Task<BlobServiceClient> CreateBlockBlobClientAsync(BlobContainerConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            if (string.IsNullOrEmpty(configuration.AccountName) || configuration.AccountName!.Equals(DevelopmentStorageConnectionString))
            {
                return new BlobServiceClient(DevelopmentStorageConnectionString);
            }
            else if (string.IsNullOrWhiteSpace(configuration.AccountKeySecretName))
            {
                // As the documentation for BlobStorageConfiguration.AccountName says:
                //  "If the account key secret name is empty, then this should contain
                //   a complete connection string."
                return new BlobServiceClient(configuration.AccountName);
            }
            else
            {
                string accountKey = await this.GetKeyVaultSecretAsync(
                    this.options?.AzureServicesAuthConnectionString,
                    configuration.KeyVaultName!,
                    configuration.AccountKeySecretName!).ConfigureAwait(false);
                var credentials = new StorageSharedKeyCredential(configuration.AccountName, accountKey);
                return new BlobServiceClient(
                    new Uri($"https://{configuration.AccountName}.blob.core.windows.net"),
                    credentials);
            }
        }
    }
}
