// <copyright file="IStorageContextSourceByName.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace Corvus.Storage
{
    /// <summary>
    /// A source of <typeparamref name="TStorageContext"/> in which code retrieves contexts by name.
    /// </summary>
    /// <typeparam name="TStorageContext">
    /// The type of storage context (e.g., a blob container, a CosmosDB collection, or a SQL
    /// connection string).
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// Code using this interface does not directly concern itself with either scopes or
    /// configuration. Scopes might still be in use—an instance of this interface might have been
    /// obtained from a <see cref="IStorageContextSourceByName{TStorageContext}"/> via the
    /// <see cref="StorageContextFactoryExtensions.GetByNameContextSource{TStorageContext, TConfiguration}(IStorageContextFactory{TStorageContext, TConfiguration}, IStorageContextScope{TConfiguration})"/>
    /// extension method, for example. An application might configure DI in such a way that each
    /// scope gets an instance of this type configured for the appropriate storage scope for the
    /// operation being processed. Equally, this could be used in code that has no use for scoped
    /// storage.
    /// </para>
    /// </remarks>
    public interface IStorageContextSourceByName<TStorageContext>
    {
        /// <summary>
        /// Gets a <typeparamref name="TStorageContext"/>  for a named context.
        /// </summary>
        /// <param name="contextName">
        /// The logical name of the storage context required.
        /// </param>
        /// <returns>
        /// A task that produces a <typeparamref name="TStorageContext"/> with access to the
        /// context identified by <paramref name="contextName"/>.
        /// </returns>
        ValueTask<TStorageContext> GetContextAsync(string contextName);
    }
}