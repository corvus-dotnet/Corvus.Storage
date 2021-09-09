// <copyright file="ISingularScopeSource.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage
{
    using System;

    /// <summary>
    /// A source of <see cref="IStorageContextScope{TConfiguration}"/> for scenarios in which
    /// multiple scopes are not in use.
    /// </summary>
    public interface ISingularScopeSource
    {
        /// <summary>
        /// Retrieves a storage context scope for the specified configuration type.
        /// </summary>
        /// <typeparam name="TConfiguration">The configuration type required.</typeparam>
        /// <returns>An <see cref="IStorageContextScope{TConfiguration}"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no configuration is available for the specified configuration type.
        /// </exception>
        IStorageContextScope<TConfiguration> For<TConfiguration>();
    }
}