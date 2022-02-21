// <copyright file="TableConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Identity.ClientAuthentication.Azure;

namespace Corvus.Storage.Azure.TableStorage;

/// <summary>
/// Encapsulates configuration for a storage account, and optionally a particular container
/// within that account.
/// </summary>
public record class TableConfiguration
{
    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Specifying the account name is mutually exclusive with specifying a connection string.
    /// Note that if you are using Azure AD authentication, you must specify an account name,
    /// because the Azure SDK does not support the use of connection strings in conjunction
    /// with Azure AD authentication.
    /// </para>
    /// </remarks>
    public string? AccountName { get; set; }

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
    /// connecting to storage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If this is set, you should set <see cref="AccountName"/>, and you should not use a
    /// connection string (because the Azure SDK does not support the use of connection strings
    /// in conjunction with Azure AD authentication, or at least, not as of v12.10).
    /// </para>
    /// </remarks>
    public ClientIdentityConfiguration? ClientIdentity { get; set; }

    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This must be the actual container name, so it must conform to the naming rules imposed
    /// by Azure, and it must unique within the storage account for this configuration, and for
    /// any other configurations referring to the same storage account. You can use the
    /// <see cref="AzureTableNaming.HashAndEncodeTableName(string)"/> method to convert any string
    /// into a table name. This uses a hash function to create a name that conforms to the Azure
    /// Storage rules for table names, and which is exceedingly unlikely to clash with any other
    /// name. (The names it produces also bear no obvious relation to the names you pass in, which
    /// is either a security feature, or very annoying, depending on your perspective.)
    /// </para>
    /// <para>
    /// Note that if you're using Cosmos DB to store your tables, it has much less stringent
    /// naming requirements. (E.g., table names can be up to 253 characters long.)
    /// </para>
    /// </remarks>
    public string? TableName { get; set; }
}