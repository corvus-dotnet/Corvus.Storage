// <copyright file="SqlDatabaseServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Storage.Sql;
using Corvus.Storage.Sql.Internal;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Common configuration code for services using Azure SQL and SQL Server.
/// </summary>
public static class SqlDatabaseServiceCollectionExtensions
{
    /// <summary>
    /// Adds an <see cref="ISqlConnectionFromDynamicConfiguration"/> to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddSqlConnectionFromDynamicConfiguration(
        this IServiceCollection services)
    {
        return services
            .AddAzureTokenCredentialSourceFromDynamicConfiguration()
            .AddSingleton<ISqlConnectionFromDynamicConfiguration, SqlConnectionFactory>();
    }
}