// <copyright file="NameHashBenchmarks.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using BenchmarkDotNet.Attributes;

using Corvus.Storage.Azure.BlobStorage;

namespace Corvus.Storage.Benchmarks
{
    [MemoryDiagnoser]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static", Justification = "BenchmarkDotNet requires benchmarks to be non-static")]
    public class NameHashBenchmarks
    {
        const string LogicalContainerName = "3633754ac4c9be44b55bfe791b1780f1-corvustenancy";

        // The original implementation from Corvus.Tenancy.
        // This is quite allocatey - it creates arrays for:
        //  * the UTF8-encoded version of the input
        //  * the hash
        //  * the char[] array in which the string is built up
        //  * the output string
        // But it does use a 256-entry lookup table that offers the following benefits
        //  * does the conversion to hex just once on startup
        //  * enables the main loop to avoid splitting each input byte into a pair of nibbles -
        //      each byte produces two digits of output, but this table contains pairs of
        //      digits, so it doesn't need to convert each nibble to hex separately
        [Benchmark(Baseline = true)]
        public string Original() => AlternateHashImplementations.Original(LogicalContainerName);

        // This works in more or less exactly the same way as the original, but all the temporary
        // buffers are now created using stackalloc (falling back to an array for the UTF8 one in
        // the case where the input is too large), and processed via spans.
        // When testing on .NET 6.0, this gives us a 25% speed boost, and drops memory consumption
        // by a factor of 3.6.
        [Benchmark]
        public string Spanified() => AlternateHashImplementations.Spanified(LogicalContainerName);

        // This tests how much benefit the lookup table is adding in practice. Or whether it's a
        // benefit at all for that matter. There are a couple of reasons to suppose it might slow
        // things down. One is that modern CPUs generally don't like indirection; if the JIT were
        // able to produce a good vectorized implementation of the code that converts the numbers
        // to hex, then it's plausible that this would actually run faster than the lookup-driven
        // one.
        // In practice, the JIT currently doesn't vectorise either this non-lookup-based version
        // or the lookup-based one. In fact it doesn't do a brilliant job in general with the
        // convert-to-hex loop: it seems to be doing bounds checking on every iteration. I suspect
        // this is because it's working with two spans which appear to have different lengths, and
        // the JIT hasn't been smart enough to realise that those lengths are directly connected,
        // and that it could eliminate the bounds checks entirely. So this goes a bit slower than
        // the spans+lookup version (although it's still about 17% faster than the original, and
        // enjoys the same factor of 3.6 memory use reduction).
        // It's conceivable that if we could somehow enable the JIT to remove or lift the bounds
        // checks, that would then open the door to vectorisation, at which point it's possible
        // the non-lookup version would then go faster.
        // The other issue with the lookup table, which is harder to quantify in a benchmark,
        // is that it does collateral damage to the CPU cache. We do not expect this function to
        // be called regularly in a tight loop - in practice, these name hashes occur from time
        // to time in the middle of other work. Given that the input to the hex conversion is
        // going to be uniformly distributed (because it's the output of a hash), we expect a
        // 20 byte hash to hit roughly 8% of the lookup table entries. However, cache lines are
        // typically 64 bytes, so the upshot will be that generating the hex for a 20 byte hash
        // will end up fetching most, and quite likely all, of the lookup array into the cache,
        // but to minimal effect, because the majority of those lines won't then be used again.
        // (The lookup size of 1024 bytes and the hash size of 20 bytes are almost optimally sized
        // for inefficient cache usage.)
        // This effect is missed entirely by the benchmark, because it calls the code again and
        // again in a tight loop, meaning that the lookup table is likely to remain resident in
        // the L1 cache for the entire duration of the benchmark, which is unlikely to be
        // representative of real-world behaviour in which this function is called much less often.
        // The effect in practice may be to load 1024 of lookup table into the L1 cache for
        // minimal benefit, displacing 1 kilobyte of other data that might otherwise have been
        // useful, meaning that code running after this method may run slower than it would
        // otherwise have done.
        // By replacing the lookup table with runtime calculations, we use considerably less
        // space in the cache, while going only slightly slower even in this benchmarking scenario
        // that artificially boosts the benefit offered by caching of the lookup table. So it
        // may well be that in practice this non-lookup-based version would result in better
        // overall application performance than the lookup-based one. (It's also conceivable
        // that this will have slightly better cold start performance, because it does less
        // work on startup.
        // However, the differences are small enough that it is unlikely to make a meaningful
        // difference either way. So we retain this purely out of interest.
        [Benchmark]
        public string NoLookupTable() => AlternateHashImplementations.NoLookupTable(LogicalContainerName);

        // This is a modification of the span-based implementation that avoids some data copying.
        // The span-based version builds the output string into a stackalloc buffer, and then
        // constructs a string from that. But this entails copying the string from that stack-based
        // buffer into the string on the heap. The string.Create method enables us to avoid that
        // copy: it allocates space for the string and then invokes a callback passing a Span<char>
        // that we can use to write the output string directly in place on the heap, instead of
        // writing it to the stack and then copying.
        // This adds a bit of complication, mainly because limitations on generics prevent us
        // from passing a ReadOnlySpan<byte> into the callback as a state argument so we have to
        // restructure things a bit to be able to use this technique. And it makes no measurable
        // performance difference in this scenario. In theory it's more efficient, but in practice
        // it looks identical. It likely only matters for significantly larger strings - it's not
        // worth the effort for 20-character strings.
        [Benchmark]
        public string StringInPlace() => AlternateHashImplementations.GenerateStringInPlace(LogicalContainerName);

        // This invokes the implementation present in the actual library being tested.
        [Benchmark]
        public string Actual() => AzureStorageBlobContainerNaming.HashAndEncodeBlobContainerName(LogicalContainerName);
    }
}
