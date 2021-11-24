# Storage context usage modes

This library supports several ways of obtaining a storage context (such as a `BlobContainerClient`), based on a couple of dimensions:

* Container identification: logical name vs configuration
* Scope requirement: none vs scoped (e.g. tenanted)

## Unscoped, explicit configuration

In this scenario, `Corvus.Storage` is doing the least work. The application code does not need storage to be split across scopes (so there is no need for tenanted storage, or any other sort of partitioning). And the application code is going to provide a populated configuration object. In this scenario, the code is looking for `Corvus.Storage` to supply just these features:

* Handling authentication details (e.g., retrieving secrets from KeyVault, or a acquiring a suitable Access Token)
* Avoiding duplication of effort through suitable caching or pooling

Client application code wants to be able to do something like this:

```cs
internal class BlobConsumingService
{
    private readonly IBlobContainerFromConfigFactory blobContainerFactory;
    private readonly BlobContainerConfiguration blobConfig;

    public BlobConsumingService(
        IBlobContainerFromConfigFactory blobContainerFactory,
        BlobContainerConfiguration blobConfig)
    {
        this.blobContainerFactory = blobContainerFactory;
        this.blobConfig = blobConfig;
    }

    public async Task<string> GetDataAsync(string id)
    {
        BlobContainerClient container =
            await this.blobContainerFactory.GetStorageContextAsync(this.blobConfig);

        BlockBlobClient dataBlob = container.GetBlockBlobClient(id);
        Response<BlobDownloadResult> data = await userBlob.DownloadContentAsync();
        return data.Value.Content.ToString();
    }
}
```

This particular example presumes that not only is the storage service available through DI, so is the configuration. This specific approach won't work if you're using more than one container. However, it would be straightforward to introduce some application-specific class that defines multiple properties of type [`BlobContainerConfiguration`](xref:Corvus.Storage.Azure.BlobStorage.BlobContainerConfiguration), one for each container in use. In that case, `BlobConsumingService` would depend on this type (e.g., `AppBlobContainerConfiguration`) and instead of passing `this.blogConfig` to `GetStorageContextAsync`, it would pass, say, `this.blobConfig.UsersContainer`, or `this.blobConfig.AppointmentsContainer`.

## Unscoped, logical container name

In this scenario, application code does not want to be directly concerned with locating the appropriate storage configuration. Instead, it would want to use names to identify the containers.

```cs
internal class BlobConsumingService
{
    private readonly IBlobContainerFromContextNameFactory blobContainerFactory;

    public BlobConsumingService(
        IBlobContainerFromContextNameFactory blobContainerFactory)
    {
        this.blobContainerFactory = blobContainerFactory;
    }

    public async Task<string> GetDataAsync(string id)
    {
        BlobContainerClient container =
            await this.blobContainerFactory.GetStorageContextAsync("users");

        BlockBlobClient dataBlob = container.GetBlockBlobClient(id);
        Response<BlobDownloadResult> data = await userBlob.DownloadContentAsync();
        return data.Value.Content.ToString();
    }
}
```