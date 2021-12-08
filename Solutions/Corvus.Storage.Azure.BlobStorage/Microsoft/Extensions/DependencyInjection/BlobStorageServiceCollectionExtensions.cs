// <copyright file="BlobStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Storage.Azure.BlobStorage;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Common configuration code for services using Azure Blob Storage.
    /// </summary>
    public static class BlobStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="IBlobContainerSourceFromDynamicConfiguration"/> to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddAzureBlobStorageClientSourceFromDynamicConfiguration(
            this IServiceCollection services)
        {
            return services
                .AddAzureTokenCredentialSourceFromDynamicConfiguration()
                .AddSingleton<IBlobContainerSourceFromDynamicConfiguration, BlobContainerClientFactory>();
        }
    }
}