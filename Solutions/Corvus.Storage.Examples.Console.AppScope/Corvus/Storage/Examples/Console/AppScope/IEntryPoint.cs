// <copyright file="IEntryPoint.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Examples.Console.AppScope
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    internal interface IEntryPoint
    {
        Task MainAsync();
    }
}
