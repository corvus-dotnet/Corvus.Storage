// <copyright file="IBlobContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using global::Azure.Storage.Blobs;

    /// <summary>
    /// A factory for a <see cref="BlobContainerClient"/>.
    /// </summary>
    public interface IBlobContainerClientFactory : IStorageContextFactory<BlobContainerClient, BlobContainerConfiguration>
    {
    }
}
