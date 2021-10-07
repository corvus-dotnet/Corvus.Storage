// <copyright file="ClientIdentitySourceTypes.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Identity
{
    /// <summary>
    /// The various sources from which client identity information can be drawn.
    /// </summary>
    public enum ClientIdentitySourceTypes
    {
        /// <summary>
        /// No client identity is in use (e.g., because the target API allows anonymous access).
        /// </summary>
        None,

        /// <summary>
        /// The client identity is determined by an explicitly configured Client ID (aka App ID)
        /// and authentication with Azure AD is achieved by presenting a client secret.
        /// </summary>
        ClientIdAndSecret,

        /// <summary>
        /// The client identity is determined by an explicitly configured Client ID (aka App ID)
        /// and authentication with Azure AD is achieved with a client certificate over TLS.
        /// </summary>
        ClientIdAndCertificate,

        /// <summary>
        /// The ambient Azure Managed Identity should be used.
        /// </summary>
        Managed,

        /// <summary>
        /// The identity with which the Azure CLI is current logged in should be used. (For local
        /// development purposes only.)
        /// </summary>
        AzureCli,

        /// <summary>
        /// Visual Studio's authentication should be used to determine the identity. (For local
        /// development purposes only.)
        /// </summary>
        VisualStudio,

        /// <summary>
        /// The scheme followed by <c>Azure.Identity</c>'s <c>DefaultAzureCredential</c> should be
        /// used, i.e. look for explicit configuration via environment variables, and if that's not
        /// present, look for a Managed Identity, and then try falling back to a series of sources
        /// suitable for local development purposes.
        /// </summary>
        AzureIdentityDefaultAzureCredential,
    }
}