// <copyright file="BlobContainerConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// Encapsulates configuration for a storage account.
    /// </summary>
    // TODO: in tenanted systems, this is read from the tenant properties, and we have a rule that says Container
    // has to be set, and it's the app's responsibility to ensure that a container of that name exists.
    // Conversely, the TenantProviderBlobStore (which implements ITenantProvider over blob storage,
    // itself a layer over ITenantBlobContainerClientFactory) requires that Container not be set, because it
    // generates its own container names.
    public class BlobContainerConfiguration
    {
        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        /// <remarks>If the account key secret name is empty, then this should contain a complete connection string.</remarks>
        public string? AccountName { get; set; }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        /// <remarks>
        /// This must be the actual container name, so it must conform to the naming rules imposed
        /// by Azure, and it must unique within the storage account for this configuration, and for
        /// any other configurations referring to the same storage account.
        /// TODO: the original docs point people at TenantedContainerNaming.MakeUniqueSafeBlobContainerName(string, string)
        /// We may need to introduce something similar.
        /// </remarks>
        public string? Container { get; set; }

        /// <summary>
        /// Gets or sets the key value name.
        /// </summary>
        public string? KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the account key secret mame.
        /// </summary>
        public string? AccountKeySecretName { get; set; }
    }
}