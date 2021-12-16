// <copyright file="CosmosContainerConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Identity.ClientAuthentication.Azure;

namespace Corvus.Storage.Azure.Cosmos;

// Nullable reference types do not work especially well with Microsoft.Extensions.Configuration
// because that does not yet support constructor-based initialization, preventing us from defining
// a primary constructor that guarantees initialization of all non-nullable properties. But we do
// want to signal which properties should always be set. So we leave nullable annotations enabled
// but we disable nullable warnings in this file.
#nullable disable warnings

/// <summary>
/// Encapsulates configuration for a Cosmos DB database, and optionally a particular container
/// within that database.
/// </summary>
/// <remarks>
/// <para>
/// This is defined as a <c>record</c> instead of a plain <c>class</c> to get the compiler to
/// generate a copy constructor, enabling us to use the record <c>with</c> syntax when we need to
/// build copies of a configuration that differ only by the <see cref="Container"/>.
/// </para>
/// </remarks>
public record CosmosContainerConfiguration
{
    /// <summary>
    /// Gets or sets the name of the database to use within the account.
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// Gets or sets the URI of the Cosmos DB account.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Specifying the account URI is mutually exclusive with specifying a connection string.
    /// </para>
    /// </remarks>
    public string? AccountUri { get; set; }

    /// <summary>
    /// Gets or sets the access key with which to connect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is mainly intended for local development scenarios, in which the inconvenience of
    /// putting the relevant secret in a key vault is not sufficiently offset by the increase in
    /// security (e.g, because the account in use for development purposes is not sensitive).
    /// </para>
    /// <para>
    /// Its use is discouraged for production purposes. Production scenarios will normally use
    /// <see cref="AccessKeyInKeyVault"/> instead.
    /// </para>
    /// </remarks>
    public string? AccessKeyPlainText { get; set; }

    /// <summary>
    /// Gets or sets the configuration describing how to retrieve the access key from
    /// an Azure Key Vault.
    /// </summary>
    public KeyVaultSecretConfiguration? AccessKeyInKeyVault { get; set; }

    /// <summary>
    /// Gets or sets the connection string with which to connect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is mainly intended for local development scenarios, in which the inconvenience of
    /// putting the relevant secret in a key vault is not offset by any meaningful increase in
    /// security. (E.g., if the connection string is the well-known shortcut
    /// <c>UseDevelopmentStorage=true</c>, you obviously don't gain anything by putting that in
    /// a key vault).
    /// </para>
    /// <para>
    /// Its use is discouraged for production purposes, because Azure Storage connection
    /// strings are required to contain credentials—either an access key or a SAS token (and
    /// the Azure SDK does not currently support the use of connection strings in conjunction
    /// with Azure AD authentication). When using connection strings, production scenarios will
    /// normally use <see cref="ConnectionStringInKeyVault"/> instead.
    /// </para>
    /// </remarks>
    public string? ConnectionStringPlainText { get; set; }

    /// <summary>
    /// Gets or sets the configuration describing how to retrieve the connection string from
    /// an Azure Key Vault.
    /// </summary>
    public KeyVaultSecretConfiguration? ConnectionStringInKeyVault { get; set; }

    /// <summary>
    /// Gets or sets the configuration describing the Azure AD client identity to use when
    /// connecting to Cosmos DB.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If this is set, you should set <see cref="AccountUri"/>, and you should not use a
    /// connection string (because the Microsoft.Azure.Cosmos client library does not support the
    /// use of connection strings in conjunction with Azure AD authentication, or at least, not as
    /// of v3.23).
    /// </para>
    /// </remarks>
    public ClientIdentityConfiguration? ClientIdentity { get; set; }

    /// <summary>
    /// Gets or sets the container name.
    /// </summary>
    /// <remarks>
    /// This is nullable because some applications may choose to store a single configuration
    /// for the database, instead of having to store multiple near-identical configurations for all
    /// the containers it uses in that database. However, this must be non-null when passed to
    /// <see cref="ICosmosContainerSourceFromDynamicConfiguration"/>.
    /// </remarks>
    public string? Container { get; set; }
}