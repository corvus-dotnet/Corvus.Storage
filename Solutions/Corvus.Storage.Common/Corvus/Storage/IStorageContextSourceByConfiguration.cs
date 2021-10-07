// <copyright file="IStorageContextSourceByConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

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
    public interface IStorageContextSourceByConfiguration<TStorageContext, TConfiguration>
    {
        /// <summary>
        /// Gets a <typeparamref name="TStorageContext"/>  for the context described in a
        /// <typeparamref name="TConfiguration"/>.
        /// </summary>
        /// <param name="contextConfiguration">
        /// Configuration describing the context required.
        /// </param>
        /// <returns>
        /// A task that produces a <typeparamref name="TStorageContext"/> with access to the
        /// context described in the <typeparamref name="TConfiguration"/>.
        /// </returns>
        ValueTask<TStorageContext> GetStorageContextAsync(TConfiguration contextConfiguration);
    }
}
