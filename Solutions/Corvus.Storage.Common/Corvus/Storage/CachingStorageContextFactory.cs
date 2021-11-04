// <copyright file="CachingStorageContextFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Azure;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;

using Corvus.Identity.ClientAuthentication.Azure;

namespace Corvus.Storage
{
    /// <summary>
    /// Common logic for caching storage contexts.
    /// </summary>
    /// <typeparam name="TStorageContext">
    /// The type of storage context (e.g., a blob container, a CosmosDB collection, or a SQL
    /// database).
    /// </typeparam>
    /// <typeparam name="TConfiguration">
    /// The type containing the information identifying a particular physical, tenant-specific
    /// instance of a context.
    /// </typeparam>
    /// <typeparam name="TConnectionOptions">
    /// The type containing information describing the particular connection requirements (e.g.,
    /// retry settings, pipeline configuration).
    /// </typeparam>
    public abstract class CachingStorageContextFactory<TStorageContext, TConfiguration, TConnectionOptions> :
        IStorageContextSourceByConfiguration<TStorageContext, TConfiguration, TConnectionOptions>
    {
        private readonly ConcurrentDictionary<string, Task<TStorageContext>> contexts = new ();
        private readonly Random random = new ();
        ////private readonly IAzureTokenCredentialSource keyVaultTokenCredentialSource;

        /////// <summary>
        /////// Creates a <see cref="CachingStorageContextFactory{TStorageContext, TConfiguration}"/>.
        /////// </summary>
        /////// <param name="keyVaultTokenCredentialSource">
        /////// The source from which to get <see cref="TokenCredential"/>s to authenticate when using
        /////// Azure Key Vault.
        /////// </param>
        ////protected CachingStorageContextFactory(
        ////    IAzureTokenCredentialSource keyVaultTokenCredentialSource)
        ////{
        ////    this.keyVaultTokenCredentialSource = keyVaultTokenCredentialSource ?? throw new ArgumentNullException(nameof(keyVaultTokenCredentialSource));
        ////}

        /////// <summary>
        /////// Get a storage container within a particular scope.
        /////// </summary>
        /////// <param name="scope">The scope (e.g. tenant) for which to retrieve the context.</param>
        /////// <param name="contextName">
        /////// The name of the required context (e.g. an Azure Storage container name, or .
        /////// </param>
        /////// <returns>
        /////// A task that produces the storage context instance for the specified scope and
        /////// container.
        /////// </returns>
        /////// <remarks>
        /////// This caches context instances to ensure that a singleton is used for all request for
        /////// the same scope and container definition.
        /////// </remarks>
        ////public async ValueTask<TStorageContext> GetContextForScopeAsync(
        ////    IStorageContextScope<TConfiguration> scope,
        ////    string contextName)
        ////{

        ////}

        /// <inheritdoc/>
        public async ValueTask<TStorageContext> GetStorageContextAsync(TConfiguration contextConfiguration, TConnectionOptions? connectionOptions)
        {
            if (contextConfiguration is null)
            {
                throw new ArgumentNullException(nameof(contextConfiguration));
            }

            string key = this.GetCacheKeyForContext(contextConfiguration);

            // TODO: what about contexts that need a rental modal because they don't support
            // concurrent use?
            Task<TStorageContext> result = this.contexts.GetOrAdd(
                key,
                async _ => await this.CreateContextAsync(contextConfiguration, connectionOptions).ConfigureAwait(false));

            if (result.IsFaulted)
            {
                // If a task has been created in the previous statement, it won't have completed yet. Therefore if it's
                // faulted, that means it was added as part of a previous request to this method, and subsequently
                // failed. As such, we will remove the item from the dictionary, and attempt to create a new one to
                // return. If removing the value fails, that's likely because it's been removed by a different thread,
                // so we will ignore that and just attempt to create and return a new value anyway.
                this.contexts.TryRemove(key, out Task<TStorageContext> _);

                // Wait for a short and random time, to reduce the potential for large numbers of spurious container
                // recreation that could happen if multiple threads are trying to rectify the failure simultanously.
                await Task.Delay(this.random.Next(150, 250)).ConfigureAwait(false);

                result = this.contexts.GetOrAdd(
                    key,
                    async _ => await this.CreateContextAsync(contextConfiguration, connectionOptions).ConfigureAwait(false));
            }

            return await result.ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a secret from Azure Key Vault.
        /// </summary>
        /// <param name="azureServicesAuthConnectionString">
        /// The connection string determining the credentials with which to connect to Azure Storage.
        /// </param>
        /// <param name="keyVaultName">
        /// The name of the key vault.
        /// </param>
        /// <param name="secretName">
        /// The name of the secret in the key vault.
        /// </param>
        /// <returns>
        /// A task producing the secret.
        /// </returns>
        protected async ValueTask<string> GetKeyVaultSecretAsync(
            string? azureServicesAuthConnectionString,
            string keyVaultName,
            string secretName)
        {
            var keyVaultCredentials = LegacyAzureServiceTokenProviderConnectionString.ToTokenCredential(azureServicesAuthConnectionString ?? string.Empty);
            return await this.GetKeyVaultSecretAsync(keyVaultCredentials, keyVaultName, secretName);
        }

        /// <summary>
        /// Retrieves a secret from Azure Key Vault.
        /// </summary>
        /// <param name="tokenCredential">
        /// The connection string determining the credentials with which to connect to Azure Storage.
        /// </param>
        /// <param name="keyVaultName">
        /// The name of the key vault.
        /// </param>
        /// <param name="secretName">
        /// The name of the secret in the key vault.
        /// </param>
        /// <returns>
        /// A task producing the secret.
        /// </returns>
        protected async ValueTask<string> GetKeyVaultSecretAsync(
            TokenCredential tokenCredential,
            string keyVaultName,
            string secretName)
        {
            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            var keyVaultClient = new SecretClient(keyVaultUri, tokenCredential);

            Response<KeyVaultSecret> accountKeyResponse = await keyVaultClient.GetSecretAsync(secretName).ConfigureAwait(false);
            return accountKeyResponse.Value.Value;
        }

        /// <summary>
        /// Create the context.
        /// </summary>
        /// <param name="configuration">The container configuration.</param>
        /// <param name="connectionOptions">Connection options (e.g., retry settings).</param>
        /// <returns>A <see cref="ValueTask"/> the produces the instance of the context.</returns>
        protected abstract ValueTask<TStorageContext> CreateContextAsync(
            TConfiguration configuration,
            TConnectionOptions? connectionOptions);

        /// <summary>
        /// Produces a unique cache key based on the particular storage context that the
        /// configuration identifies.
        /// </summary>
        /// <param name="contextConfiguration">
        /// Configuration describing the storage context.
        /// </param>
        /// <returns>
        /// A key that is unique to the storage context identified by this configuration.
        /// </returns>
        protected abstract string GetCacheKeyForContext(TConfiguration contextConfiguration);

        ////private async Task<TStorageContext> CreateContainerAsync(
        ////    IStorageContextScope<TConfiguration> scope,
        ////    string contextName)
        ////{
        ////    TConfiguration configuration = scope.GetConfigurationForContext(contextName);

        ////    return await this.CreateContextAsync(contextName, configuration).ConfigureAwait(false);
        ////}
    }
}
