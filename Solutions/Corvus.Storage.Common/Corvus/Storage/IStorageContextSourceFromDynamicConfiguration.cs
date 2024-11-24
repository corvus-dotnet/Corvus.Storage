// <copyright file="IStorageContextSourceFromDynamicConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace Corvus.Storage
{
    /// <summary>
    /// A source of <typeparamref name="TStorageContext"/> in which code has populated one or more
    /// <typeparamref name="TConfiguration"/> objects, and wants to get access to the storage
    /// contexts these configuration objects represent.
    /// </summary>
    /// <typeparam name="TStorageContext">
    /// The type of storage context (e.g., a blob container, a CosmosDB collection, or a SQL
    /// connection string).
    /// </typeparam>
    /// <typeparam name="TConfiguration">
    /// The type containing the information identifying a particular physical, tenant-specific
    /// instance of a context.
    /// </typeparam>
    /// <typeparam name="TConnectionOptions">
    /// The type containing information describing the particular connection requirements (e.g.,
    /// retry settings, pipeline configuration).
    /// </typeparam>
    public interface IStorageContextSourceFromDynamicConfiguration<TStorageContext, TConfiguration, TConnectionOptions>
    {
        /// <summary>
        /// Gets a <typeparamref name="TStorageContext"/>  for the context described in a
        /// <typeparamref name="TConfiguration"/>.
        /// </summary>
        /// <param name="contextConfiguration">
        /// Configuration describing the context required.
        /// </param>
        /// <param name="connectionOptions">
        /// Connection options (e.g., retry settings).
        /// </param>
        /// <param name="cancellationToken">
        /// May enable the request to be cancelled.
        /// </param>
        /// <returns>
        /// A task that produces a <typeparamref name="TStorageContext"/> with access to the
        /// context described in the <typeparamref name="TConfiguration"/>.
        /// </returns>
        ValueTask<TStorageContext> GetStorageContextAsync(
            TConfiguration? contextConfiguration,
            TConnectionOptions? connectionOptions,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a <typeparamref name="TStorageContext"/>  to replace one that seems to have
        /// stopped working.
        /// </summary>
        /// <param name="contextConfiguration">
        /// Configuration describing the context required.
        /// </param>
        /// <param name="connectionOptions">
        /// Connection options (e.g., retry settings).
        /// </param>
        /// <param name="cancellationToken">
        /// May enable the request to be cancelled.
        /// </param>
        /// <returns>
        /// A task that produces a <typeparamref name="TStorageContext"/> with access to the
        /// context described in the <typeparamref name="TConfiguration"/>.
        /// </returns>
        ValueTask<TStorageContext> GetReplacementForFailedStorageContextAsync(
            TConfiguration? contextConfiguration,
            TConnectionOptions? connectionOptions,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a <typeparamref name="TStorageContext"/>  for the context described in a
        /// <typeparamref name="TConfiguration"/>.
        /// </summary>
        /// <param name="contextConfiguration">
        /// Configuration describing the context required.
        /// </param>
        /// <param name="cancellationToken">
        /// May enable the request to be cancelled.
        /// </param>
        /// <returns>
        /// A task that produces a <typeparamref name="TStorageContext"/> with access to the
        /// context described in the <typeparamref name="TConfiguration"/>.
        /// </returns>
        ValueTask<TStorageContext> GetStorageContextAsync(
            TConfiguration? contextConfiguration,
            CancellationToken cancellationToken = default)
        {
            return this.GetStorageContextAsync(contextConfiguration, default, cancellationToken);
        }

        /// <summary>
        /// Gets a <typeparamref name="TStorageContext"/>  to replace one that seems to have
        /// stopped working.
        /// </summary>
        /// <param name="contextConfiguration">
        /// Configuration describing the context required.
        /// </param>
        /// <param name="cancellationToken">
        /// May enable the request to be cancelled.
        /// </param>
        /// <returns>
        /// A task that produces a <typeparamref name="TStorageContext"/> with access to the
        /// context described in the <typeparamref name="TConfiguration"/>.
        /// </returns>
        ValueTask<TStorageContext> GetReplacementForFailedStorageContextAsync(
            TConfiguration? contextConfiguration,
            CancellationToken cancellationToken = default)
        {
            return this.GetReplacementForFailedStorageContextAsync(contextConfiguration, default, cancellationToken);
        }
    }
}