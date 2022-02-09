// <copyright file="BlobStorageConfigurationSettings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

// Microsoft.Extensions.Configuration does not get on well with nullable refs
#nullable disable annotations

using Corvus.Storage.Azure.BlobStorage;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration
{
    /// <summary>
    /// Describes the structure of the application's Blob Storage configuration.
    /// </summary>
    public class BlobStorageConfigurationSettings
    {
        /// <summary>
        /// Gets or sets the blob storage configuration that puts a connection string directly into
        /// the settings in plain text.
        /// </summary>
        public BlobContainerConfiguration ConnectionStringInConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the blob storage configuration that puts a connection string in key vault.
        /// </summary>
        public BlobContainerConfiguration ConnectionStringInKeyVault { get; set; }

        /// <summary>
        /// Gets or sets the blob storage configuration that puts an access key in key vault, and
        /// the account name in configuration.
        /// </summary>
        public BlobContainerConfiguration AccessKeyInKeyVault { get; set; }
    }
}