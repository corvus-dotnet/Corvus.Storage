// <copyright file="TableConfigurationTypes.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.TableStorage.Internal;

/// <summary>
/// The supported types of <see cref="TableConfiguration"/>.
/// </summary>
internal enum TableConfigurationTypes
{
    /// <summary>
    /// No configuration type could be determined.
    /// </summary>
    NotRecognized,

    /// <summary>
    /// The account name, and the access key in plain text.
    /// </summary>
    AccountNameAndAccessKeyAsPlainText,

    /// <summary>
    /// The account name, and the access key in key vault.
    /// </summary>
    AccountNameAndAccessKeyInKeyVault,

    /// <summary>
    /// A connection string in plain text.
    /// </summary>
    ConnectionStringAsPlainText,

    /// <summary>
    /// A connection string in key vault.
    /// </summary>
    ConnectionStringInKeyVault,

    /// <summary>
    /// The account name and a client identity.
    /// </summary>
    AccountNameAndClientIdentity,
}