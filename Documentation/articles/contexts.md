# Contexts in `Corvus.Storage`

The [`Corvus.Storage.Common` NuGet package](https://www.nuget.org/packages/Corvus.Storage.Common) relies on the concept of storage contexts.

A storage context is, ultimately, the entire point of these libraries: it is the object that enables an application to use the underlying storage service. For example, if you are using Azure Storage blob containers, a context would be represented by a [`BlobContainerClient`](xref:Azure.Storage.Blobs.BlobContainerClient); when using Cosmos DB through its SQL API, it would be a [`Container`](xref:Microsoft.Azure.Cosmos.Container). More generally, a storage context is whatever type the relevant storage client library offers to represent some particular container. The exact nature of the container depends on the storage technology in use.

Applications specify a name when retrieving a context. For example, imagine an application that used Azure Blob Storage, storing user settings in one blob container, and application documents in another. Code in that application could take a dependency (supplied via dependency injection) on [`IBlobContainerClientFactory`](xref:IBlobContainerClientFactory)


 the [`IStorageContextFactory<TStorageContext, TConfiguration>`]()

 ```cs
 public class UsesStorage
 {
    private readonly IBlobContainerSourceByConfiguration blobContainerClientFactory;
    public UsesStorage(IBlobContainerSourceByConfiguration blobContainerClientFactory)
    {
        this.blobContainerClientFactory = blobContainerClientFactory;
    }

    public async Task<string> GetUserInfoAsync(
            BlobContainerConfiguration baseConfiguration,
            string containerName,
            string id)
    {
        BlobContainerConfiguration containerConfig = baseConfiguration.ForContainer(containerName);

        BlobContainerClient container = await this.blobContainerFactory.GetStorageContextAsync(containerConfig)
            .ConfigureAwait(false);

        BlockBlobClient dataBlob = container.GetBlockBlobClient(id);
        Response<BlobDownloadResult> data = await dataBlob.DownloadContentAsync().ConfigureAwait(false);
        return data.Value.Content.ToString();
    }
 }
 ```