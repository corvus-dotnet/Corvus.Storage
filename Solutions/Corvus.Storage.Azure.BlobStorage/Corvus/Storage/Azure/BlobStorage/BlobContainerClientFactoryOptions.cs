// <copyright file="BlobContainerClientFactoryOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// Configuration settings for <see cref="IBlobContainerClientFactory"/>.
    /// </summary>
    public class BlobContainerClientFactoryOptions
    {
        /// <summary>
        /// Gets or sets the connection string to use when obtaining tokens as the service identity.
        /// </summary>
        /// <remarks>
        /// <p>
        /// In most circumstances, this will be null, indicating that the default behaviour should
        /// be used. The default is that when running in an Azure App Service environment (e.g., a
        /// Function), the Managed Identity will be used. When running locally during development,
        /// it will look for a Visual Studio-supplied token file, and if there isn't one it then
        /// tries asking the Azure CLI for a token.
        /// </p>
        /// <p>
        /// If you want to be able to set this string via settings (e.g., in a local.settings.json
        /// file) use the following code in your startup:
        /// </p>
        /// <code><![CDATA[
        /// services.AddTenantBlobContainerClientFactory(configurationRoot.Get<TenantBlobContainerClientFactoryOptions>());
        /// ]]></code>
        /// <p>
        /// The <c>Get&lt;T&gt;</c> method used here is an extension method in the
        /// <c>Microsoft.Extensions.Configuration</c> namespace, provided by the
        /// <c>Microsoft.Extensions.Configuration.Binder</c> NuGet package. If you're using the
        /// <c>Microsoft.Extensions.Options.ConfigurationExtensions</c> NuGet package to get access
        /// to the integration of <c>IOptions&lt;T&gt;</c> with configuration, you will already
        /// have an implicit reference to the binder package.
        /// </p>
        /// <p>
        /// There is no requirement to use <c>Microsoft.Extensions.Configuration</c>. You are free
        /// to create an instance of <see cref="BlobContainerClientFactoryOptions"/> however you like. The
        /// example above just shows an easy way to support the common configuration convention of
        /// enabling the connection string to be set with an application setting named
        /// <c>AzureServicesAuthConnectionString</c>.
        /// </p>
        /// </remarks>
        public string? AzureServicesAuthConnectionString { get; set; }
    }
}
