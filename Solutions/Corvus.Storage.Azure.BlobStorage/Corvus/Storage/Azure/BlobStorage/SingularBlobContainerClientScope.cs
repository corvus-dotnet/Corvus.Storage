// <copyright file="SingularBlobContainerClientScope.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Extensions.Configuration;

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
