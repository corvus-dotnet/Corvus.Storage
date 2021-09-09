// <copyright file="FakeConfigurationOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

// Currently (as of .NET 5.0) there is no good support for nullable reference types when using
// Microsoft.Extensions.Configuration with configuration binding. The best option is to make
// types used in configuration binding null-oblivious
#nullable disable annotations

namespace Corvus.Storage.Common
{
    /// <summary>
    /// A type used by tests to represent a typical type defining configurable settings.
    /// </summary>
    public class FakeConfigurationOptions
    {
        public string StringProperty1 { get; set; }

        public string StringProperty2 { get; set; }

        public int NumericProperty1 { get; set; }
    }
}
