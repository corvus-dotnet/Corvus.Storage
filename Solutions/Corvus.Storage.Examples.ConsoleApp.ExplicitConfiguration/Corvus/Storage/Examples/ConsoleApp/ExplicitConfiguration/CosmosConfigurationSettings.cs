// <copyright file="CosmosConfigurationSettings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

// Microsoft.Extensions.Configuration does not get on well with nullable refs
#nullable disable annotations

using Corvus.Storage.Azure.Cosmos;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration
{
    /// <summary>
    /// Describes the structure of the application's Cosmos configuration.
    /// </summary>
    public class CosmosConfigurationSettings
    {
        /// <summary>
        /// Gets or sets the Cosmos configuration that puts a connection string directly into
        /// the settings in plain text.
        /// </summary>
        public CosmosContainerConfiguration ConnectionStringInConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos configuration that puts a connection string in key vault.
        /// </summary>
        public CosmosContainerConfiguration ConnectionStringInKeyVault { get; set; }
    }
}