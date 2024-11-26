// <copyright file="CachingStorageContextFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azure;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;

using Corvus.Identity.ClientAuthentication.Azure;

using Microsoft.Extensions.DependencyInjection;

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
    internal abstract class CachingStorageContextFactory<TStorageContext, TConfiguration, TConnectionOptions> :
        IStorageContextSourceFromDynamicConfiguration<TStorageContext, TConfiguration, TConnectionOptions>
        where TConnectionOptions : class
    {
        private readonly ConcurrentDictionary<string, Task<TStorageContext>> contexts = new();
        private readonly List<(WeakReference<TConnectionOptions> Options, string Id)> trackedConnections = new();
        private readonly IServiceProvider serviceProvider;
        private IAzureTokenCredentialSourceFromDynamicConfiguration? azureTokenCredentialSourceFromConfig;
        private int nextConnectionsOptionsId = 1;

        /// <summary>
        /// Creates a <see cref="CachingStorageContextFactory{TStorageContext, TConfiguration, TConnectionOptions}"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// Provides access to dependencies that are only needed in certain scenarios, and which
        /// we don't want to cause a DI initialization failure for if they are absent. (We depend
        /// on <see cref="IServiceIdentityAzureTokenCredentialSource"/>, but only in certain
        /// scenarios.)
        /// </param>
        protected CachingStorageContextFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets a random number generated used for picking a delay time before retrying something.
        /// </summary>
        internal Random Random { get; } = new();

        /// <summary>
        /// Gets an <see cref="IAzureTokenCredentialSourceFromDynamicConfiguration"/> from DI.
        /// (This retrieves this on demand, so if DI has not been configured so as to supply
        /// this, it will fail on first use of this property, rather than at the point where DI
        /// constructs us.)
        /// </summary>
        protected IAzureTokenCredentialSourceFromDynamicConfiguration AzureTokenCredentialSourceFromConfig
            => this.azureTokenCredentialSourceFromConfig ??= this.serviceProvider.GetRequiredService<IAzureTokenCredentialSourceFromDynamicConfiguration>();

        /// <inheritdoc/>
        public async ValueTask<TStorageContext> GetStorageContextAsync(
            TConfiguration contextConfiguration,
            TConnectionOptions? connectionOptions,
            CancellationToken cancellationToken)
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
                async _ => await this.CreateContextAsync(contextConfiguration, connectionOptions, cancellationToken).ConfigureAwait(false));

            if (result.IsFaulted)
            {
                // If a task has been created in the previous statement, it won't have completed yet. Therefore, if it's
                // faulted, that means it was added as part of a previous request to this method, and subsequently
                // failed. As such, we will remove the item from the dictionary, and attempt to create a new one to
                // return. If removing the value fails, that's likely because it's been removed by a different thread,
                // so we will ignore that and just attempt to create and return a new value anyway.
                this.contexts.TryRemove(key, out _);

                // Wait for a short and random time, to reduce the potential for large numbers of spurious container
                // recreation that could happen if multiple threads are trying to rectify the failure simultaneously.
                await Task.Delay(this.Random.Next(150, 250), cancellationToken).ConfigureAwait(false);

                result = this.contexts.GetOrAdd(
                    key,
                    async _ => await this.CreateContextAsync(contextConfiguration, connectionOptions, cancellationToken).ConfigureAwait(false));
            }

            TStorageContext context = await result.ConfigureAwait(false);
            return context;
        }

        /// <inheritdoc/>
        public async ValueTask<TStorageContext> GetReplacementForFailedStorageContextAsync(
            TConfiguration contextConfiguration,
            TConnectionOptions? connectionOptions,
            CancellationToken cancellationToken)
        {
            string key = this.GetCacheKeyForContext(contextConfiguration, connectionOptions);
            if (this.contexts.TryRemove(key, out Task<TStorageContext>? _))
            {
                this.InvalidateForConfiguration(contextConfiguration, connectionOptions, cancellationToken);
            }

            return await this.GetStorageContextAsync(
                contextConfiguration, connectionOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Create the context.
        /// </summary>
        /// <param name="contextConfiguration">
        /// Configuration describing the storage context.
        /// </param>
        /// <param name="connectionOptions">Connection options (e.g., retry settings).</param>
        /// <param name="cancellationToken">
        /// May enable the operation to be cancelled.
        /// </param>
        /// <returns>A <see cref="ValueTask"/> the produces the instance of the context.</returns>
        protected abstract ValueTask<TStorageContext> CreateContextAsync(
            TConfiguration contextConfiguration,
            TConnectionOptions? connectionOptions,
            CancellationToken cancellationToken);

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
        protected abstract string GetCacheKeyForConfiguration(TConfiguration contextConfiguration);

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

        /// <summary>
        /// Invalidate anything that should be invalidated (e.g. cached client identities) when
        /// the application has indicated that a context has become invalid.
        /// </summary>
        /// <param name="contextConfiguration">
        /// Configuration describing the storage context.
        /// </param>
        /// <param name="connectionOptions">Connection options (e.g., retry settings).</param>
        /// <param name="cancellationToken">
        /// May enable the operation to be cancelled.
        /// </param>
        protected abstract void InvalidateForConfiguration(
            TConfiguration contextConfiguration,
            TConnectionOptions? connectionOptions,
            CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a secret from Azure Key Vault.
        /// </summary>
        /// <param name="secretConfiguration">
        /// Describes the secret to retrieve.
        /// </param>
        /// <param name="cancellationToken">
        /// May enable the operation to be cancelled.
        /// </param>
        /// <returns>
        /// A task producing the secret, or null of no secret was found.
        /// </returns>
        protected async ValueTask<string?> GetKeyVaultSecretFromConfigAsync(
            KeyVaultSecretConfiguration secretConfiguration,
            CancellationToken cancellationToken)
        {
            // If no identity for the key vault is specified we use the ambient service
            // identity. Otherwise, we use the identity configuration supplied.
            IAzureTokenCredentialSource credentialSource = secretConfiguration.VaultClientIdentity is null
                ? this.serviceProvider.GetRequiredService<IServiceIdentityAzureTokenCredentialSource>()
                : await this.AzureTokenCredentialSourceFromConfig
                    .CredentialSourceForConfigurationAsync(secretConfiguration.VaultClientIdentity, cancellationToken)
                    .ConfigureAwait(false);
            TokenCredential? keyVaultCredentials = await credentialSource.GetTokenCredentialAsync(cancellationToken)
                .ConfigureAwait(false);

            if (keyVaultCredentials is not null)
            {
                var keyVaultUri = new Uri($"https://{secretConfiguration.VaultName}.vault.azure.net/");
                var keyVaultClient = new SecretClient(keyVaultUri, keyVaultCredentials);

                Response<KeyVaultSecret> accountKeyResponse = await keyVaultClient.GetSecretAsync(
                    secretConfiguration.SecretName,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                return accountKeyResponse.Value.Value;
            }

            return null;
        }

        /// <summary>
        /// Invalidates any credentials that may be cached for a particular identity.
        /// </summary>
        /// <param name="clientIdentity">
        /// The configuration describing the identity for which cached credentials are suspected to
        /// be out of date. Accepts nulls to save the need for callers to check for null. (This does
        /// nothing if this argument is null.)
        /// </param>
        protected void InvalidateCredentials(ClientIdentityConfiguration? clientIdentity)
        {
            if (clientIdentity is not null)
            {
                this.AzureTokenCredentialSourceFromConfig.InvalidateFailedAccessToken(clientIdentity);
            }
        }
    }
}