// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>


using BenchmarkDotNet.Running;

using Corvus.Storage.Benchmarks;

BenchmarkRunner.Run<NameHashBenchmarks>();