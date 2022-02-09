// <copyright file="UsingBlobStorageWithExplicitConfig.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

using Corvus.Storage.Azure.BlobStorage;

namespace Corvus.Storage.Examples.Azure.BlobStorage
{
    /// <summary>
    /// Example illustrating how to consume Azure Blob Storage containers in scenarios where the
    /// application supplies a populated <see cref="BlobContainerConfiguration"/>, and does not
    /// require tenancy or any other kind of scoping, and does not want to use logical container
    /// names.
    /// </summary>
    public class UsingBlobStorageWithExplicitConfig
    {
        private readonly IBlobContainerSourceFromDynamicConfiguration blobContainerFactory;

        /// <summary>
        /// Creates a <see cref="UsingBlobStorageWithExplicitConfig"/>.
        /// </summary>
        /// <param name="blobContainerFactory">Source of blob container clients.</param>
        public UsingBlobStorageWithExplicitConfig(
            IBlobContainerSourceFromDynamicConfiguration blobContainerFactory)
        {
            this.blobContainerFactory = blobContainerFactory;
        }

        /// <summary>
        /// Retrieve the specified blob.
        /// </summary>
        /// <param name="baseConfiguration">
        /// The blob container configuration from which to build the container-specific
        /// configuration.
        /// </param>
        /// <param name="containerName">The container from which to fetch the blob.</param>
        /// <param name="id">The id of the blob to fetch.</param>
        /// <returns>A task producing the blob content.</returns>
        public async Task<string> GetDataAsync(
            BlobContainerConfiguration baseConfiguration,
            string containerName,
            string id)
        {
            BlobContainerConfiguration containerConfig = baseConfiguration with
            {
                Container = containerName,
            };

            BlobContainerClient container = await this.blobContainerFactory.GetStorageContextAsync(containerConfig)
                .ConfigureAwait(false);

            BlockBlobClient dataBlob = container.GetBlockBlobClient(id);
            Response<BlobDownloadResult> data = await dataBlob.DownloadContentAsync().ConfigureAwait(false);
            return data.Value.Content.ToString();
        }
    }
}