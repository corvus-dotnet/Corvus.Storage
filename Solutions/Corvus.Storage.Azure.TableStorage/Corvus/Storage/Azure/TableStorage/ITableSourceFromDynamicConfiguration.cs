// <copyright file="ITableSourceFromDynamicConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Azure.Data.Tables;

namespace Corvus.Storage.Azure.TableStorage;

/// <summary>
/// A source of <see cref="TableClient"/> in which code has populated one or more
/// <see cref="TableConfiguration"/> objects, and wants to get access to the
/// <see cref="TableClientOptions"/> objects these represent.
/// </summary>
public interface ITableSourceFromDynamicConfiguration :
    IStorageContextSourceFromDynamicConfiguration<TableClient, TableConfiguration, TableClientOptions>
{
}