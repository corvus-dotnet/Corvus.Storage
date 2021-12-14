// <copyright file="AlternateHashImplementations.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;

namespace Corvus.Storage.Benchmarks
{
    internal class AlternateHashImplementations
    {
        private static readonly Lazy<SHA1> HashProvider = new(() => SHA1.Create());
        private static readonly uint[] Lookup32 = CreateLookup32();

        public static string Original(string containerName)
        {
            byte[] byteContents = Encoding.UTF8.GetBytes(containerName);
            byte[] hashedBytes = HashProvider.Value.ComputeHash(byteContents);
            return ByteArrayToHexViaLookup32(hashedBytes);
        }

        public static string Spanified(string containerName)
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
                throw new InvalidOperationException($"Failed to produce hash of expected size");
            }

            return ByteArrayToHexViaLookup32Span(hashedBytes);
        }

        public static string NoLookupTable(string containerName)
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
                throw new InvalidOperationException($"Failed to produce hash of expected size");
            }

            return ByteArrayToHexStringNoLookupTable(hashedBytes);
        }
        public static string GenerateStringInPlace(string containerName)
        {
            const int Sha1Length = 20;
            return string.Create(Sha1Length * 2, containerName, static (result, name) =>
            {
                Encoding encoding = Encoding.UTF8;
                int utfSize = encoding.GetByteCount(name);
                Span<byte> nameUtf8 = utfSize < 200
                    ? stackalloc byte[utfSize]
                    : new byte[utfSize];
                encoding.GetBytes(name, nameUtf8);

                Span<byte> hashedBytes = stackalloc byte[Sha1Length];
                if (!HashProvider.Value.TryComputeHash(nameUtf8, hashedBytes, out int written) || written != Sha1Length)
                {
                    throw new InvalidOperationException($"Failed to produce hash of expected size");
                }

                uint[] lookup32 = Lookup32;
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    uint val = lookup32[hashedBytes[i]];
                    result[2 * i] = (char)val;
                    result[(2 * i) + 1] = (char)(val >> 16);
                }
            });
        }

        private static string ByteArrayToHexStringNoLookupTable(ReadOnlySpan<byte> bytes)
        {
            Span<char> result = stackalloc char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                byte v = bytes[i];
                byte lsn = (byte)(v & 0x0f);
                byte msn = (byte)((v & 0xf0) >> 4);
                result[2 * i] = (char)((msn < 10 ? '0' : ('a' - 10)) + msn);
                result[(2 * i) + 1] = (char)((lsn < 10 ? '0' : ('a' - 10)) + lsn);
            }

            return new string(result);
        }

        private static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            uint[] lookup32 = Lookup32;
            char[] result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                uint val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[(2 * i) + 1] = (char)(val >> 16);
            }

            return new string(result);
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
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("x2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }

            return result;
        }
    }
}
