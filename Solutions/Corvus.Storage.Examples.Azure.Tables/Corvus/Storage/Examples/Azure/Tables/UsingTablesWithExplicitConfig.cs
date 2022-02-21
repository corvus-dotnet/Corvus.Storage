// <copyright file="UsingTablesWithExplicitConfig.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Azure.Data.Tables;

using Corvus.Storage.Azure.TableStorage;

namespace Corvus.Storage.Examples.Azure.Tables;

/// <summary>
/// Example illustrating how to consume Azure Tables in scenarios where the application supplies a
/// populated <see cref="TableConfiguration"/>, and does not require tenancy or any other kind of
/// scoping, and does not want to use logical container names.
/// </summary>
public class UsingTablesWithExplicitConfig
{
    private readonly ITableSourceFromDynamicConfiguration tableClientFactory;

    /// <summary>
    /// Creates a <see cref="UsingTablesWithExplicitConfig"/>.
    /// </summary>
    /// <param name="blobContainerFactory">Source of blob container clients.</param>
    public UsingTablesWithExplicitConfig(
        ITableSourceFromDynamicConfiguration blobContainerFactory)
    {
        this.tableClientFactory = blobContainerFactory;
    }

    /// <summary>
    /// Retrieve the specified table row.
    /// </summary>
    /// <typeparam name="T">The type into which to deserialize the row.</typeparam>
    /// <param name="tableConfiguration">
    /// The table configuration from which to build the table-specific configuration.
    /// </param>
    /// <param name="tableName">The table from which to fetch the data.</param>
    /// <param name="partitionKey">The partition key of the row to fetch.</param>
    /// <param name="rowKey">The row key of the row to fetch.</param>
    /// <returns>A task producing the blob content.</returns>
    /// <remarks>
    /// <para>
    /// This shows the usage model when the configuration does not specify a table name. This is
    /// common when application code uses multiple tables under the same account: we typically have
    /// a single configuration entry for the account, and application code then plugs in the table
    /// name required for each use.
    /// </para>
    /// </remarks>
    public async Task<T> GetDataAsync<T>(
        TableConfiguration tableConfiguration,
        string tableName,
        string partitionKey,
        string rowKey)
        where T : class, ITableEntity, new()
    {
        TableClient tableClient = await this.GetTableClientAsync(tableConfiguration, tableName).ConfigureAwait(false);

        return await tableClient.GetEntityAsync<T>(partitionKey, rowKey).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieve the specified table row.
    /// </summary>
    /// <typeparam name="T">The type into which to deserialize the row.</typeparam>
    /// <param name="tableConfiguration">
    /// The table configuration from which to build the table-specific configuration.
    /// </param>
    /// <param name="tableName">The table from which to fetch the data.</param>
    /// <param name="row">The data to store in the row.</param>
    /// <returns>A task producing the blob content.</returns>
    /// <remarks>
    /// <para>
    /// This shows the usage model when the configuration does not specify a table name. This is
    /// common when application code uses multiple tables under the same account: we typically have
    /// a single configuration entry for the account, and application code then plugs in the table
    /// name required for each use.
    /// </para>
    /// </remarks>
    public async Task UpsertDataAsync<T>(
        TableConfiguration tableConfiguration,
        string tableName,
        T row)
        where T : class, ITableEntity, new()
    {
        TableClient tableClient = await this.GetTableClientAsync(tableConfiguration, tableName).ConfigureAwait(false);
        await tableClient.CreateIfNotExistsAsync().ConfigureAwait(false);

        await tableClient.UpsertEntityAsync<T>(row).ConfigureAwait(false);
    }

    private async Task<TableClient> GetTableClientAsync(TableConfiguration tableConfiguration, string tableName)
    {
        TableConfiguration containerConfig = tableConfiguration with
        {
            TableName = tableName,
        };

        return await this.tableClientFactory.GetStorageContextAsync(containerConfig)
            .ConfigureAwait(false);
    }
}