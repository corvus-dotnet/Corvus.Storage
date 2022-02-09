// <copyright file="TwoLevelCachingStorageContextFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Corvus.Storage
{
    /// <summary>
    /// Common logic for caching storage contexts that need to cache at two levels (typically at
    /// the container level, but also the storage account or server level).
    /// </summary>
    /// <typeparam name="TStorageContextParent">
    /// The outer cacheable type. This might typically represent a particular account or server.
    /// </typeparam>
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
    /// <remarks>
    /// <para>
    /// Some storage types have client SDKs that provide some sort of type representing a broader
    /// scope than an individual context. We might be able to obtain multiple different storage
    /// contexts from the same server for example, and with some client SDKs, the docs recommended
    /// that the object representing this broader scope be shared. This might be because that
    /// object's lifetime is associated with the lifetime of connections to the service. Creating a
    /// new per-service object for every single container we hand out might cause significant
    /// performance problems. So for client SDKs in which this is important we offer this base
    /// class that caches not just the individual containers, but also the higher level scopes to
    /// which those containers belong.
    /// </para>
    /// </remarks>
    internal abstract class TwoLevelCachingStorageContextFactory<TStorageContextParent, TStorageContext, TConfiguration, TConnectionOptions> :
        CachingStorageContextFactory<TStorageContext, TConfiguration, TConnectionOptions>
        where TConnectionOptions : class
    {
        private readonly ConcurrentDictionary<string, Task<TStorageContextParent>> parentContexts = new();

        /// <summary>
        /// Creates a <see cref="TwoLevelCachingStorageContextFactory{TStorageContextParent, TStorageContext, TConfiguration, TConnectionOptions}"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// Required by the base class.
        /// </param>
        protected TwoLevelCachingStorageContextFactory(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <inheritdoc/>
        protected override async ValueTask<TStorageContext> CreateContextAsync(
            TConfiguration configuration,
            TConnectionOptions? connectionOptions,
            CancellationToken cancellationToken)
        {
            string parentContextKey = this.GetCacheKeyForParentContext(configuration, connectionOptions);

            Task<TStorageContextParent> parentContextTask = this.parentContexts.GetOrAdd(
                parentContextKey,
                async _ => await this.CreateParentContextAsync(configuration, connectionOptions, cancellationToken)
                    .ConfigureAwait(false));

            if (parentContextTask.IsFaulted)
            {
                // If a task has been created in the previous statement, it won't have completed yet. Therefore if it's
                // faulted, that means it was added as part of a previous request to this method, and subsequently
                // failed. As such, we will remove the item from the dictionary, and attempt to create a new one to
                // return. If removing the value fails, that's likely because it's been removed by a different thread,
                // so we will ignore that and just attempt to create and return a new value anyway.
                this.parentContexts.TryRemove(parentContextKey, out Task<TStorageContextParent> _);

                // Wait for a short and random time, to reduce the potential for large numbers of spurious container
                // recreation that could happen if multiple threads are trying to rectify the failure simultanously.
                await Task.Delay(this.Random.Next(150, 250), cancellationToken).ConfigureAwait(false);

                parentContextTask = this.parentContexts.GetOrAdd(
                    parentContextKey,
                    async _ => await this.CreateParentContextAsync(configuration, connectionOptions, cancellationToken)
                        .ConfigureAwait(false));
            }

            TStorageContextParent parentContext = await parentContextTask.ConfigureAwait(false);
            return await this.CreateContextAsync(parentContext, configuration, connectionOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Create the context.
        /// </summary>
        /// <param name="parent">The parent in which to create the container.</param>
        /// <param name="configuration">The container configuration.</param>
        /// <param name="connectionOptions">Connection options (e.g., retry settings).</param>
        /// <param name="cancellationToken">
        /// May enable the operation to be cancelled.
        /// </param>
        /// <returns>A <see cref="ValueTask"/> the produces the instance of the context.</returns>
        protected abstract ValueTask<TStorageContext> CreateContextAsync(
            TStorageContextParent parent,
            TConfiguration configuration,
            TConnectionOptions? connectionOptions,
            CancellationToken cancellationToken);

        /// <summary>
        /// Create the parent context.
        /// </summary>
        /// <param name="configuration">The container configuration.</param>
        /// <param name="connectionOptions">Connection options (e.g., retry settings).</param>
        /// <param name="cancellationToken">
        /// May enable the operation to be cancelled.
        /// </param>
        /// <returns>A <see cref="ValueTask"/> the produces the instance of the context.</returns>
        protected abstract ValueTask<TStorageContextParent> CreateParentContextAsync(
            TConfiguration configuration,
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
        protected virtual string GetCacheKeyForParentContext(
            TConfiguration contextConfiguration,
            TConnectionOptions? connectionOptions)
        {
            return this.GetCacheKeyForParentConfiguration(contextConfiguration) + "/" + this.GetCacheKeyForConnectionOptions(connectionOptions);
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
        protected abstract string GetCacheKeyForParentConfiguration(TConfiguration contextConfiguration);
    }
}