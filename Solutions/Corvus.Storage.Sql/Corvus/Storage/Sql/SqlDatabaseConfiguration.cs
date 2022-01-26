// <copyright file="SqlDatabaseConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Identity.ClientAuthentication.Azure;

namespace Corvus.Storage.Sql
{
    /// <summary>
    /// Encapsulates configuration for a Azure SQL or SQL Server database.
    /// </summary>
    public record SqlDatabaseConfiguration
    {

        /// <summary>
        /// Gets or sets the connection string with which to connect.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This should only be used in scenarios in which the connection string contains no
        /// secrets, or in local development scenarios, in which the inconvenience of
        /// putting the relevant secret in a key vault is not offset by any meaningful increase in
        /// security.
        /// </para>
        /// <para>
        /// In production, this would typically only be used if <see cref="ClientIdentity"/> is
        /// also set. In that case, this string only identifies the database - authentication is
        /// handled by other means. In cases where the connection string includes credentials,
        /// use <see cref="ConnectionStringInKeyVault"/> instead.
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
        /// connecting to the database.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is intended for use with Azure SQL, with Azure AD-based client authentication
        /// enabled.
        /// </para>
        /// </remarks>
        public ClientIdentityConfiguration? ClientIdentity { get; set; }
    }
}