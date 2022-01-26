// <copyright file="ISqlConnectionFromDynamicConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Data.SqlClient;

namespace Corvus.Storage.Sql;

/// <summary>
/// A source of <see cref="SqlConnection"/> in which code has populated one or more
/// <see cref="SqlDatabaseConfiguration"/> objects, and wants to get access to the
/// <see cref="SqlConnection"/> objects these represent.
/// </summary>
public interface ISqlConnectionFromDynamicConfiguration :
        IStorageContextSourceFromDynamicConfiguration<SqlConnection, SqlDatabaseConfiguration, object>
{
}