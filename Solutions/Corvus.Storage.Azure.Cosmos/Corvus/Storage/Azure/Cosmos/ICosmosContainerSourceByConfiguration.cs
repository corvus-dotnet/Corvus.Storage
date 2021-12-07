// <copyright file="ICosmosContainerSourceByConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.Azure.Cosmos;

namespace Corvus.Storage.Azure.Cosmos;

/// <summary>
/// A source of <see cref="Container"/> in which code has populated one or more
/// <see cref="CosmosContainerConfiguration"/> objects, and wants to get access to the
/// <see cref="Container"/> objects these represent.
/// </summary>
public interface ICosmosContainerSourceByConfiguration :
        IStorageContextSourceByConfiguration<Container, CosmosContainerConfiguration, CosmosClientOptions>
{
}