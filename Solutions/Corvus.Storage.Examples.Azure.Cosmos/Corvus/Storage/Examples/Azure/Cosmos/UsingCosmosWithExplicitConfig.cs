// <copyright file="UsingCosmosWithExplicitConfig.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Storage.Azure.Cosmos;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json.Linq;

namespace Corvus.Storage.Examples.Azure.Cosmos;

/// <summary>
/// Example illustrating how to consume Azure Blob Storage containers in scenarios where the
/// application supplies a populated <see cref="CosmosContainerConfiguration"/>, and does not
/// require tenancy or any other kind of scoping, and does not want to use logical container
/// names.
/// </summary>
public class UsingCosmosWithExplicitConfig
{
    private readonly ICosmosContainerSourceByConfiguration cosmosContainerFactory;

    /// <summary>
    /// Creates a <see cref="UsingCosmosWithExplicitConfig"/>.
    /// </summary>
    /// <param name="cosmosContainerFactory">Source of cosmos containers.</param>
    public UsingCosmosWithExplicitConfig(
        ICosmosContainerSourceByConfiguration cosmosContainerFactory)
    {
        this.cosmosContainerFactory = cosmosContainerFactory;
    }

    /// <summary>
    /// Retrieve the specified document.
    /// </summary>
    /// <param name="baseConfiguration">
    /// The blob container configuration from which to build the container-specific
    /// configuration.
    /// </param>
    /// <param name="containerName">The container from which to fetch the blob.</param>
    /// <param name="id">The id of the document to fetch.</param>
    /// <param name="partitionKey">The key of the partition that contains the document.</param>
    /// <returns>A task producing the blob content.</returns>
    public async Task<string> GetDataAsync(
        CosmosContainerConfiguration baseConfiguration,
        string containerName,
        string id,
        string partitionKey)
    {
        CosmosContainerConfiguration containerConfig = baseConfiguration with
        {
            // This seems to be a bug in StyleCop - it doesn't appear to understand the 'with' syntax.
#pragma warning disable SA1101 // Prefix local calls with this
            Container = containerName,
#pragma warning restore SA1101 // Prefix local calls with this
        };

        Container container = await this.cosmosContainerFactory.GetStorageContextAsync(containerConfig)
            .ConfigureAwait(false);

        ItemResponse<JObject> response = await container.ReadItemAsync<JObject>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        return response.Resource.ToString();
    }
}