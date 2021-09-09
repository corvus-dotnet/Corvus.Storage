// <copyright file="ConfigurationScopeSource.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage
{
    using System;
    using System.Collections.Concurrent;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// An <see cref="ISingularScopeSource"/> that reads configuration values from an
    /// <see cref="IConfiguration"/>.
    /// </summary>
    // TODO: I'm not sure this is all that useful.
    // App-level config really wants to be able to specify just account-level settings, and we don't
    // really want to have to specify the container names in the config: for app-level usage, we really
    // want just to pass the logical container names straight through.
    internal class ConfigurationScopeSource : ISingularScopeSource
    {
        private readonly IConfiguration configurationRoot;
        private readonly ConcurrentDictionary<Type, object> scopesByType = new ();

        /// <summary>
        /// Creates a <see cref="ConfigurationScopeSource"/>.
        /// </summary>
        /// <param name="configuration">The configuration root from which to read settings.</param>
        /// <param name="configurationSection">
        /// The name of the configuration section under which settings are to be read, or <c>null</c>
        /// if settings are at the configuration root.
        /// </param>
        public ConfigurationScopeSource(IConfiguration configuration, string? configurationSection)
        {
            this.configurationRoot = string.IsNullOrWhiteSpace(configurationSection)
                ? configuration
                : configuration.GetSection(configurationSection);
        }

        /// <inheritdoc/>
        public IStorageContextScope<TConfiguration> For<TConfiguration>()
        {
            return (Scope<TConfiguration>)this.scopesByType.GetOrAdd(
                typeof(TConfiguration),
                type =>
                {
                    IConfigurationSection sectionForThisConfiguration = this.configurationRoot.GetSection(type.Name);
                    if (!sectionForThisConfiguration.Exists())
                    {
                        throw new InvalidOperationException();
                    }

                    TConfiguration configuration = sectionForThisConfiguration.Get<TConfiguration>();
                    return new Scope<TConfiguration>(configuration);
                });
        }

        private class Scope<TConfiguration> : IStorageContextScope<TConfiguration>
        {
            private readonly TConfiguration configuration;

            public Scope(TConfiguration configuration)
            {
                this.configuration = configuration;
            }

            public string CreateCacheKeyForContext(string storageContextName)
            {
                return storageContextName;
            }

            public TConfiguration GetConfigurationForContext(string storageContextName)
            {
                return this.configuration;
            }
        }
    }
}