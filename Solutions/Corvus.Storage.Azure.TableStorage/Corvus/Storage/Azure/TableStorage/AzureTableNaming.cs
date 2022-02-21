// <copyright file="AzureTableNaming.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;

namespace Corvus.Storage.Azure.TableStorage;

/// <summary>
/// Helpers to convert plain text names for Azure storage items into guaranteed valid hashes.
/// </summary>
/// <remarks>
/// There are various restrictions on entity names in Azure storage. For example, a blob container name can start
/// with a letter or a number, but a table name has to start with a letter. Both blob container and table names
/// can be a maximum of 63 characters long, and so on. As a result, it's desirable to have a mechanism for taking
/// an "ideal world" table name and converting it into a name that's guaranteed to be safe to use. This class
/// provides a helper method to do that.
/// </remarks>
public static class AzureTableNaming
{
    private static readonly Lazy<SHA1> HashProvider = new(() => SHA1.Create());
    private static readonly uint[] Lookup32 = CreateLookup32();

    // TODO: are there different rules for Azure Storage tables and Cosmos DB tables?
    // The docs don't mention anything of the sort, but by inspection we find that 254 characters is maximum
    // length for a table name in Cosmos DB, which is a lot longer than the 63 character limit described in
    // https://docs.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model
    // Also, CosmosDB allows names that are blocked such as "tables".
    // CosmosDB is case sensitive, making it possible to create two tables differing only by case, which
    // you can't do in Azure Storage.
    // Azure Storage doesn't allow non-alphanumeric characters/
    // There's also this:
    // https://docs.microsoft.com/en-us/azure/cosmos-db/table/table-api-faq#where-is-table-api-not-identical-with-azure-table-storage-behavior-
    // which doesn't mention any of the above other than case sensitivity, but outlines some other differences.

    /// <summary>
    /// Makes a plain text name safe to use as an Azure storage table name.
    /// </summary>
    /// <param name="tableName">The plain text name for the table.</param>
    /// <returns>The encoded name.</returns>
    public static string HashAndEncodeTableName(string tableName)
    {
        Encoding e = Encoding.UTF8;
        int utfSize = e.GetByteCount(tableName);
        Span<byte> nameUtf8 = utfSize < 200
            ? stackalloc byte[utfSize]
            : new byte[utfSize];
        e.GetBytes(tableName, nameUtf8);

        const int Sha1Length = 20;
        Span<byte> hashedBytes = stackalloc byte[Sha1Length];
        if (!HashProvider.Value.TryComputeHash(nameUtf8, hashedBytes, out int written) || written != Sha1Length)
        {
            throw new InvalidOperationException("Failed to produce hash of expected size");
        }

        // Table names can't start with a number, so prefix all names with a letter
        return PrefixedByteArrayToHexViaLookup32Span('t', hashedBytes);
    }

    private static string PrefixedByteArrayToHexViaLookup32Span(char prefix, ReadOnlySpan<byte> bytes)
    {
        uint[] lookup32 = Lookup32;
        Span<char> result = stackalloc char[(bytes.Length * 2) + 1];
        result[0] = prefix;
        Span<char> rest = result[1..];
        for (int i = 0; i < bytes.Length; i++)
        {
            uint val = lookup32[bytes[i]];
            rest[2 * i] = (char)val;
            rest[(2 * i) + 1] = (char)(val >> 16);
        }

        return new string(result);
    }

    private static uint[] CreateLookup32()
    {
        uint[] result = new uint[256];
        Span<char> digits = stackalloc char[2];
        for (int i = 0; i < 256; i++)
        {
            i.TryFormat(digits, out _, "x2");
            result[i] = ((uint)digits[0]) + ((uint)digits[1] << 16);
        }

        return result;
    }
}