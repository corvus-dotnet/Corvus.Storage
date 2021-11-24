// <copyright file="BlobContainerConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Identity.ClientAuthentication.Azure;

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// Encapsulates configuration for a storage account.
    /// </summary>
    public class BlobContainerConfiguration
    {
        /// <summary>
        /// Creates a <see cref="BlobContainerConfiguration"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// We define a cloning constructor to make it easy to create new versions of
        /// configurations that have specific properties changed but which are identical in other
        /// respects, but this has the side effect of disabling C#'s default constructor
        /// generation. Nice though it would be to be able to declare one more constructor that
        /// requires all non-optional properties, Microsoft.Extensions.Configuration can't cope
        /// with that, so we have to supply a no-args constructor.
        /// </para>
        /// </remarks>
        public BlobContainerConfiguration()
        {
        }

        /// <summary>
        /// Creates a new <see cref="BlobContainerConfiguration"/> with the same settings as an
        /// existing one.
        /// </summary>
        /// <param name="source">The configuration object from which to copy settings.</param>
        /// <remarks>
        /// <para>
        /// This supports scenarios in which applications want to use multiple containers in the
        /// same storage account, and doesn't want to store multiple configuration entries that
        /// are all identical except for the <see cref="Container"/> name. Instead, they can create
        /// a single configuration entry, and then use this to create modified versions with the
        /// relevant container name plugged in.
        /// </para>
        /// <para>
        /// TODO: is it possible to write this in such a way that it supports the <c>with</c>
        /// syntax? C# 10 opens that up to a wider range of scenarios, and if there's some
        /// convention we can follow that means you could just write <c>config with { Container = name }</c>
        /// that would be great.
        /// </para>
        /// </remarks>
        public BlobContainerConfiguration(
            BlobContainerConfiguration source)
        {
            this.AccountName = source.AccountName;
            this.AccessKeyPlainText = source.AccessKeyPlainText;
            this.AccessKeyInKeyVault = source.AccessKeyInKeyVault;
            this.ConnectionStringPlainText = source.ConnectionStringPlainText;
            this.ConnectionStringInKeyVault = source.ConnectionStringInKeyVault;
            this.StorageClientIdentity = source.StorageClientIdentity;
            this.Container = source.Container;
        }

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
        public ClientIdentityConfiguration? StorageClientIdentity { get; set; }

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
        /// Builds a new configuration that has a different <see cref="Container"/>, but which is
        /// otherwise identical to this one.
        /// </summary>
        /// <param name="containerName">
        /// The value the new configuration's <see cref="Container"/> should have.
        /// </param>
        /// <returns>
        /// A <see cref="BlobContainerConfiguration"/> which is a copy of this one, but for a
        /// different blob container.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This enables applications to use multiple containers in the same storage account while
        /// storing just a single <see cref="BlobContainerConfiguration"/> in configuration.
        /// </para>
        /// </remarks>
        public BlobContainerConfiguration ForContainer(string containerName)
        {
            return new BlobContainerConfiguration(this)
            {
                Container = containerName,
            };
        }
    }
}