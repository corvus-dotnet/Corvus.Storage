// <copyright file="AzureStorageBlobContainerNaming.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Security.Cryptography;
using System.Text;

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// Helpers to convert plain text names for Azure storage items into guaranteed valid hashes.
    /// </summary>
    /// <remarks>
    /// There are various restrictions on entity names in Azure storage. For example, a blob container name can start
    /// with a letter or a number, but a table name has to start with a letter. Both blob container and table names
    /// can be a maximum of 63 characters long, and so on. As a result, it's desirable to have a mechanism for taking
    /// an "ideal world" container name and converting it into a name that's guaranteed to be safe to use. This class
    /// provides a helper method to do that.
    /// </remarks>
    public static class AzureStorageBlobContainerNaming
    {
        private static readonly Lazy<SHA1> HashProvider = new(() => SHA1.Create());
        private static readonly uint[] Lookup32 = CreateLookup32();

        /// <summary>
        /// Make a plain text name safe to use as an Azure storage blob container name.
        /// </summary>
        /// <param name="containerName">The plain text name for the blob container.</param>
        /// <returns>The encoded name.</returns>
        public static string HashAndEncodeBlobContainerName(string containerName)
        {
            Encoding e = Encoding.UTF8;
            int utfSize = e.GetByteCount(containerName);
            Span<byte> nameUtf8 = utfSize < 200
                ? stackalloc byte[utfSize]
                : new byte[utfSize];
            e.GetBytes(containerName, nameUtf8);

            const int Sha1Length = 20;
            Span<byte> hashedBytes = stackalloc byte[Sha1Length];
            if (!HashProvider.Value.TryComputeHash(nameUtf8, hashedBytes, out int written) || written != Sha1Length)
            {
                throw new InvalidOperationException("Failed to produce hash of expected size");
            }

            return ByteArrayToHexViaLookup32Span(hashedBytes);
        }

        private static string ByteArrayToHexViaLookup32Span(ReadOnlySpan<byte> bytes)
        {
            uint[] lookup32 = Lookup32;
            Span<char> result = stackalloc char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                uint val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[(2 * i) + 1] = (char)(val >> 16);
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
}