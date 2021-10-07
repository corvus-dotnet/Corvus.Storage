// <copyright file="IEntryPoint.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace Corvus.Storage.Examples.ConsoleApp.AppScope
{
    internal interface IEntryPoint
    {
        Task MainAsync();
    }
}
