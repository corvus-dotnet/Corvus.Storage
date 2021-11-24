// <copyright file="UsingBlobStorage.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

using Corvus.Storage.Azure.BlobStorage;

namespace Corvus.Storage.Examples.ConsoleApp.AppScope
{
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