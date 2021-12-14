// <copyright file="CachingStorageContextFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        IStorageContextSourceFromDynamicConfiguration<TStorageContext, TConfiguration, TConnectionOptions>
        where TConnectionOptions : class
    {
        private readonly ConcurrentDictionary<string, Task<TStorageContext>> contexts = new ();
        private readonly List<(WeakReference<TConnectionOptions> Options, string Id)> trackedConnections = new ();
        private int nextConnectionsOptionsId = 1;

        /// <summary>
        /// Gets a random number generated used for picking a delay time before retrying something.
        /// </summary>
        internal Random Random { get; } = new ();

        /// <inheritdoc/>
        public async ValueTask<TStorageContext> GetStorageContextAsync(TConfiguration contextConfiguration, TConnectionOptions? connectionOptions)
        {
            if (contextConfiguration is null)
            {
                throw new ArgumentNullException(nameof(contextConfiguration));
            }

            string key = this.GetCacheKeyForContext(contextConfiguration, connectionOptions);

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
                await Task.Delay(this.Random.Next(150, 250)).ConfigureAwait(false);

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
            return await this.GetKeyVaultSecretAsync(keyVaultCredentials, keyVaultName, secretName).ConfigureAwait(false);
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
        /// <param name="contextConfiguration">
        /// Configuration describing the storage context.
        /// </param>
        /// <param name="connectionOptions">Connection options (e.g., retry settings).</param>
        /// <returns>A <see cref="ValueTask"/> the produces the instance of the context.</returns>
        protected abstract ValueTask<TStorageContext> CreateContextAsync(
            TConfiguration contextConfiguration,
            TConnectionOptions? connectionOptions);

        /// <summary>
        /// Produces a unique cache key based on the combination of a particular storage context
        /// that the configuration identifies, and a particular set of connection options.
        /// </summary>
        /// <param name="contextConfiguration">
        /// Configuration describing the storage context.
        /// </param>
        /// <param name="connectionOptions">Connection options (e.g., retry settings).</param>
        /// <returns>
        /// A key that is unique to the combination of the storage context identified by this
        /// configuration and the specified connection options.
        /// </returns>
        protected virtual string GetCacheKeyForContext(
            TConfiguration contextConfiguration,
            TConnectionOptions? connectionOptions)
        {
            return this.GetCacheKeyForConfiguration(contextConfiguration) + "/" + this.GetCacheKeyForConnectionOptions(connectionOptions);
        }

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
        protected abstract string GetCacheKeyForConfiguration(
            TConfiguration contextConfiguration);

        /// <summary>
        /// Produces a unique cache key based on a particular set of connection options.
        /// </summary>
        /// <param name="connectionOptions">Connection options (e.g., retry settings).</param>
        /// <returns>
        /// A key that is unique to the specified connection options.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This default implementation is designed for when connection options types don't provide
        /// a good way to inspect their contents. (Sadly, most of these types in the Azure SDK
        /// don't support this because a lot of the interested state is not publicly visible.)
        /// The best we can do at this point is track specific instances we've seen before,
        /// which we do via weak references.
        /// </para>
        /// </remarks>
        protected virtual string GetCacheKeyForConnectionOptions(
            TConnectionOptions? connectionOptions)
        {
            if (connectionOptions == null)
            {
                return "none";
            }

            lock (this.trackedConnections)
            {
                string? id = null;
                for (int i = this.trackedConnections.Count - 1; i >= 0; i--)
                {
                    (WeakReference<TConnectionOptions> weakRef, string currentId) = this.trackedConnections[i];
                    if (weakRef.TryGetTarget(out TConnectionOptions? currentOptions))
                    {
                        if (ReferenceEquals(currentOptions, connectionOptions))
                        {
                            if (id is not null)
                            {
                                throw new InvalidOperationException("Error! Same connection options has ended up with more than one id");
                            }

                            id = currentId;

                            // We don't break at this point because we want to clear out any defunct WeakReferences.
                        }
                    }
                    else
                    {
                        this.trackedConnections.RemoveAt(i);
                    }
                }

                if (id == null)
                {
                    id = (this.nextConnectionsOptionsId++).ToString();
                    this.trackedConnections.Add((new WeakReference<TConnectionOptions>(connectionOptions), id));
                }

                return id;
            }
        }
    }
}