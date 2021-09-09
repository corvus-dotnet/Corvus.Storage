// <copyright file="BlobStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using Corvus.Storage;
    using Corvus.Storage.Azure.BlobStorage;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class BlobStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="ISingularScopeSource"/> that reads settings from <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration root from which to read settings.</param>
        /// <param name="configurationSection">
        /// The name of the configuration section under which settings are to be read, or <c>null</c>
        /// if settings are at the configuration root.
        /// </param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddSingularAzureBlobStorageScopeSource(
            this IServiceCollection services,
            IConfiguration configuration,
            string? configurationSection = null)
        {
            return services.AddSingleton<ISingularBlobContainerClientScope>(new SingularBlobContainerClientScope());
        }

        /// <summary>
        /// Adds an <see cref="IBlobContainerClientFactory"/> to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddAzureBlobStorageClient(
            this IServiceCollection services)
        {
            return services.AddSingleton<IBlobContainerClientFactory, BlobContainerClientFactory>();
        }
    }
}