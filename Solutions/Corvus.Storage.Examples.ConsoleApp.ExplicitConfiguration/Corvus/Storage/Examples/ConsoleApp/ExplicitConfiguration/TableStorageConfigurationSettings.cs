// <copyright file="TableStorageConfigurationSettings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

// Microsoft.Extensions.Configuration does not get on well with nullable refs
#nullable disable annotations

using Corvus.Storage.Azure.TableStorage;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration;

/// <summary>
/// Describes the structure of the application's Table Storage configuration.
/// </summary>
public class TableStorageConfigurationSettings
{
    /// <summary>
    /// Gets or sets the table storage configuration that puts a connection string directly into
    /// the settings in plain text.
    /// </summary>
    public TableConfiguration ConnectionStringInConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the table storage configuration that puts a connection string in key vault.
    /// </summary>
    public TableConfiguration ConnectionStringInKeyVault { get; set; }

    /// <summary>
    /// Gets or sets the table storage configuration that puts an access key in key vault, and
    /// the account name in configuration.
    /// </summary>
    public TableConfiguration AccessKeyInKeyVault { get; set; }
}