// <copyright file="CosmosContainerConfigurationTypes.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.Cosmos.Internal;

/// <summary>
/// The supported types of <see cref="CosmosContainerConfiguration"/>.
/// </summary>
internal enum CosmosContainerConfigurationTypes
{
    /// <summary>
    /// No configuration type could be determined.
    /// </summary>
    NotRecognized,

    /// <summary>
    /// The account URI, and the access key in plain text.
    /// </summary>
    AccountUriAndAccessKeyAsPlainText,

    /// <summary>
    /// The account URI, and the access key in key vault.
    /// </summary>
    AccountUriAndAccessKeyInKeyVault,

    /// <summary>
    /// A connection string in plain text.
    /// </summary>
    ConnectionStringAsPlainText,

    /// <summary>
    /// A connection string in key vault.
    /// </summary>
    ConnectionStringInKeyVault,

    /// <summary>
    /// The account URI and a client identity.
    /// </summary>
    AccountUriAndClientIdentity,
}