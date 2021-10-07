// <copyright file="KeyVaultSecretConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

// Types that work with Microsoft.Extensions.Configuration can't satisfactorily work with nullable
// references in C#. Normally we make such types null-oblivious (#nullable disable annotations)
// because that's the world Microsoft.Extensions.Configuration was designed for. However, this
// type has a mixture of optional and required settings, and nullability is a good way to express
// that. So we're leaving annotations enabled, and disabling the corresponding warnings for this
// file instead.
#nullable disable warnings

namespace Corvus.Identity
{
    /// <summary>
    /// Configuration identifying a secret stored in an Azure Key Vault.
    /// </summary>
    /// <remarks>
    /// <para>
    /// TODO: it's not clear if this should move to Corvus.Identity, Corvus.Configuration, or somewhere else.
    /// </para>
    /// </remarks>
    public class KeyVaultSecretConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the key vault that contains the secret.
        /// </summary>
        public string VaultName { get; set; }

        /// <summary>
        /// Gets or sets the name of the secret in the key vault.
        /// </summary>
        public string SecretName { get; set; }

        /// <summary>
        /// Gets or sets the optional configuration describing the client identity to use when
        /// accessing the key vault. If null, the ambient identity (typically an Azure Managed
        /// Identity) will be used.
        /// </summary>
        public ClientIdentityConfiguration? VaultClientIdentity { get; set; }
    }
}