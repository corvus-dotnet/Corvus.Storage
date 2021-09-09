// <copyright file="UsingBlobStorage.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Examples.Console.AppScope
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Corvus.Storage.Azure.BlobStorage;
    using global::Azure;
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;
    using global::Azure.Storage.Blobs.Specialized;

    internal class UsingBlobStorage : IEntryPoint
    {
        private readonly IBlobContainerClientFactory blobContainerClientFactory;
        private readonly ISingularBlobContainerClientScope storageScope;

        public UsingBlobStorage(
            IBlobContainerClientFactory blobContainerClientFactory,
            ISingularBlobContainerClientScope storageScope)
        {
            this.blobContainerClientFactory = blobContainerClientFactory ?? throw new ArgumentNullException(nameof(blobContainerClientFactory));
            this.storageScope = storageScope ?? throw new ArgumentNullException(nameof(storageScope));
        }

        public async Task MainAsync()
        {
            BlobContainerClient usersContainer =
                await this.blobContainerClientFactory.GetContextForScopeAsync(this.storageScope, "users")
                .ConfigureAwait(false);
            BlobContainerClient dataContainer =
                await this.blobContainerClientFactory.GetContextForScopeAsync(this.storageScope, "data")
                .ConfigureAwait(false);

            await usersContainer.CreateIfNotExistsAsync().ConfigureAwait(false);
            await dataContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            BlockBlobClient userBlob = usersContainer.GetBlockBlobClient("JoeBlogs.json");
            Response<BlobDownloadResult> userContent = await userBlob.DownloadContentAsync().ConfigureAwait(false);
            if (userContent.GetRawResponse().Status == 404)
            {
                var r = new Random();
                var newUser = new Person("Joe Blogs", new DateTime(1986, r.Next(1, 12), r.Next(1, 28)));
                byte[] serializedUser = JsonSerializer.SerializeToUtf8Bytes(newUser);
                Response<BlobContentInfo> result = await userBlob.UploadAsync(
                    new MemoryStream(serializedUser),
                    new BlobUploadOptions
                    {
                        Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
                    }).ConfigureAwait(false);
            }
        }

        private class Person
        {
            public Person(
                string name,
                DateTime dateOfBirth)
            {
                this.Name = name;
                this.DateOfBirth = dateOfBirth;
            }

            public string Name { get; }

            public DateTime DateOfBirth { get; }
        }
    }
}