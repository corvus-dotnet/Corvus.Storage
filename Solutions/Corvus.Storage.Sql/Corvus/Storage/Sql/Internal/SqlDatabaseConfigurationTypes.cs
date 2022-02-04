// <copyright file="SqlDatabaseConfigurationTypes.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Sql.Internal;

/// <summary>
/// The supported types of <see cref="SqlDatabaseConfiguration"/>.
/// </summary>
internal enum SqlDatabaseConfigurationTypes
{
    /// <summary>
    /// No configuration type could be determined.
    /// </summary>
    NotRecognized,

    /// <summary>
    /// A connection string in plain text.
    /// </summary>
    ConnectionStringAsPlainText,

    /// <summary>
    /// A connection string in key vault.
    /// </summary>
    ConnectionStringInKeyVault,
}