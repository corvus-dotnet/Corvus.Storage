// <copyright file="IBlobContainerSourceByName.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using global::Azure.Storage.Blobs;

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// A source of <see cref="BlobContainerClient"/> in which code asks for contains by their
    /// logical name.
    /// </summary>
    public interface IBlobContainerSourceByName :
        IStorageContextSourceByName<BlobContainerClient>
    {
    }
}
