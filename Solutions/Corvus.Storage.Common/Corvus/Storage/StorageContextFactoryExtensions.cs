// <copyright file="StorageContextFactoryExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;

namespace Corvus.Storage
{
    /// <summary>
    /// Extension methods for <see cref="IStorageContextFactory{TStorageContext, TConfiguration}"/>.
    /// </summary>
    public static class StorageContextFactoryExtensions
    {
        /// <summary>
        /// Gets an <see cref="IStorageContextSourceByName{TStorageContext}"/> for a storage
        /// scope.
        /// </summary>
        /// <typeparam name="TStorageContext">
        /// The the storage context type that the underlying storage provider creates.
        /// </typeparam>
        /// <typeparam name="TConfiguration">
        /// The configuration type for the underlying storage provider.
        /// </typeparam>
        /// <param name="source">
        /// The <see cref="IStorageContextFactory{TStorageContext, TConfiguration}"/> that will
        /// supply the storage contexts.
        /// </param>
        /// <param name="scope">
        /// The scope for which storage contexts will be requied.
        /// </param>
        /// <returns>
        /// A <see cref="IStorageContextSourceByName{TStorageContext}"/> that returns
        /// <typeparamref name="TStorageContext"/> instances from logical context names.
        /// </returns>
        public static IStorageContextSourceByName<TStorageContext> GetByNameContextSource<TStorageContext, TConfiguration>(
            this IStorageContextFactory<TStorageContext, TConfiguration> source,
            IStorageContextScope<TConfiguration> scope)
        {
            throw new NotImplementedException();
        }
    }
}