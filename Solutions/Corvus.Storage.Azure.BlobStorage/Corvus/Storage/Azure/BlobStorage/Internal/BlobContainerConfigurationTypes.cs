// <copyright file="BlobContainerConfigurationTypes.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Internal;

/// <summary>
/// The supported types of <see cref="BlobContainerConfiguration"/>.
/// </summary>
internal enum BlobContainerConfigurationTypes
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