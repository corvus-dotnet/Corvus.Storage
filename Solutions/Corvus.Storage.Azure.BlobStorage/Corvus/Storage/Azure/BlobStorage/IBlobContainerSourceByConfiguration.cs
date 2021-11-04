// <copyright file="IBlobContainerSourceByConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Azure.Storage.Blobs;

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// A source of <see cref="BlobContainerClient"/> in which code has populated one or more
    /// <see cref="BlobContainerConfiguration"/> objects, and wants to get access to the
    /// <see cref="BlobContainerClient"/> objects these represent.
    /// </summary>
    public interface IBlobContainerSourceByConfiguration :
        IStorageContextSourceByConfiguration<BlobContainerClient, BlobContainerConfiguration, BlobClientOptions>
    {
    }
}