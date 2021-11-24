# Corvus.Storage

`Corvus.Storage` is a set of NuGet packages that provide various features commonly required when developing applications that rely on storage services such as Cosmos DB or Azure Storage:

* configuration settings types for each provider (e.g. [`CosmosConfiguration`](xref:Corvus.Storage.Azure.Cosmos.CosmosConfiguration))
* an abstract model for storage scopes (the basis for tenanted storage in [`Corvus.Tenancy`](https://github.com/corvus-dotnet/Corvus.Tenancy/)) and 'contexts' (corresponding to e.g., containers in Azure Storage)
* caching of 'contexts'
* integration with Azure Key Vault
