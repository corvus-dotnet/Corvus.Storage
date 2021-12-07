// <copyright file="CosmosServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Storage.Azure.Cosmos;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Common configuration code for services using Azure Cosmos DB via SQL.
    /// </summary>
    public static class CosmosServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="ICosmosContainerSourceByConfiguration"/> to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddCosmosContainerSource(
            this IServiceCollection services)
        {
            return services
                .AddAzureTokenCredentialSourceFromDynamicConfiguration()
                .AddSingleton<ICosmosContainerSourceByConfiguration, CosmosContainerFactory>();
        }
    }
}