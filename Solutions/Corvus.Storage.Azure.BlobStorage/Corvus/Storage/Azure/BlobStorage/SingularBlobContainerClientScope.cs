// <copyright file="SingularBlobContainerClientScope.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// Storage scope for BlobContainerClient contexts for scenarios where only a single (e.g.
    /// application-wide) scope is required.
    /// </summary>
    internal class SingularBlobContainerClientScope : ISingularBlobContainerClientScope
    {
        /// <inheritdoc/>
        public string CreateCacheKeyForContext(string storageContextName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public BlobContainerConfiguration GetConfigurationForContext(string storageContextName)
        {
            throw new NotImplementedException();
        }
    }
}
