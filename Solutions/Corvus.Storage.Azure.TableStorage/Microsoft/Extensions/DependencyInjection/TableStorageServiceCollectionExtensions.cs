// <copyright file="TableStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Storage.Azure.TableStorage;
using Corvus.Storage.Azure.TableStorage.Internal;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Common configuration code for services using Azure Table Storage.
/// </summary>
public static class TableStorageServiceCollectionExtensions
{
    /// <summary>
    /// Adds an <see cref="ITableSourceFromDynamicConfiguration"/> to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddAzureTableClientSourceFromDynamicConfiguration(
            this IServiceCollection services)
    {
        return services
            .AddAzureTokenCredentialSourceFromDynamicConfiguration()
            .AddSingleton<ITableSourceFromDynamicConfiguration, TableClientFactory>();
    }
}