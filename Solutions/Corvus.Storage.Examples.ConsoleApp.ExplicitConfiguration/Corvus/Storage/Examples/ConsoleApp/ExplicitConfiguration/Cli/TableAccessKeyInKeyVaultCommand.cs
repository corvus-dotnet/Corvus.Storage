// <copyright file="TableAccessKeyInKeyVaultCommand.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.CommandLine.Invocation;
using System.Threading.Tasks;

using Corvus.Storage.Azure.TableStorage;
using Corvus.Storage.Examples.Azure.Tables;

namespace Corvus.Storage.Examples.ConsoleApp.ExplicitConfiguration.Cli;

/// <summary>
/// Command line handler for running the <see cref="UsingTablesWithExplicitConfig"/>
/// example with configuration that contains a connection string as plain text.
/// </summary>
public class TableAccessKeyInKeyVaultCommand : UseTableWithAppConfigurationBase
{
    /// <summary>
    /// Creates a <see cref="BlobConnectionStringInPlainTextCommand"/>.
    /// </summary>
    public TableAccessKeyInKeyVaultCommand()
        : base("access-key-key-vault")
    {
    }

    /// <summary>
    /// The handler invoked by the System.Command hosting infrastructure, enabling use of DI.
    /// </summary>
    public class Run : RunBase
    {
        /// <summary>
        /// Creates a <see cref="Run"/> instance.
        /// </summary>
        /// <param name="usingTablesWithExplicitConfig">The example that this command relies on.</param>
        /// <param name="configuration">Application configuration settings.</param>
        public Run(
            UsingTablesWithExplicitConfig usingTablesWithExplicitConfig,
            ApplicationConfigurationSettings configuration)
            : base(usingTablesWithExplicitConfig, configuration)
        {
        }

        /// <inheritdoc/>
        public override Task<int> InvokeAsync(InvocationContext context) => this.InvokeAsyncImpl(context);

        /// <inheritdoc/>
        protected override TableConfiguration GetConfiguration() =>
            this.Configuration.TableStorage.AccessKeyInKeyVault;
    }
}