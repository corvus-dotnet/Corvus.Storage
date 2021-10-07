// <copyright file="StorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Storage;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class StorageServiceCollectionExtensions
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
        // TODO: might not actually be useful?
        public static IServiceCollection AddStorageScopeSourceFromConfiguration(
            this IServiceCollection services,
            IConfiguration configuration,
            string? configurationSection = null)
        {
            return services.AddSingleton<ISingularScopeSource>(new ConfigurationScopeSource(configuration, configurationSection));
        }
    }
}