// <copyright file="ApplicationConfigurationSettings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

// Microsoft.Extensions.Configuration does not get on well with nullable refs
#nullable disable annotations

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
    }
}