// <copyright file="ApplicationConfigurationSettings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

// Microsoft.Extensions.Configuration does not get on well with nullable refs
#nullable disable annotations

using Corvus.Identity.ClientAuthentication.Azure;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration
{
    /// <summary>
    /// Describes the structure of the application's configuration settings.
    /// </summary>
    public class ApplicationConfigurationSettings
    {
        /// <summary>
        /// Gets or sets the configuration settings for blob storage.
        /// </summary>
        public BlobStorageConfigurationSettings BlobStorage { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for Cosmos.
        /// </summary>
        public CosmosConfigurationSettings Cosmos { get; set; }

        /// <summary>
        /// Gets or sets the configuration determining the Azure AD identity to use as the ambient
        /// service identity.
        /// </summary>
        public ClientIdentityConfiguration ServiceIdentity { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for table storage.
        /// </summary>
        public TableStorageConfigurationSettings TableStorage { get; set; }
    }
}