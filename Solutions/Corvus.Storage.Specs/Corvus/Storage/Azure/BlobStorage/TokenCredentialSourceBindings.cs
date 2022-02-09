// <copyright file="TokenCredentialSourceBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Identity;

using Corvus.Identity.ClientAuthentication.Azure;

using Microsoft.Extensions.DependencyInjection;

using TechTalk.SpecFlow;

namespace Corvus.Storage.Azure.BlobStorage
{
    [Binding]
    public class TokenCredentialSourceBindings
    {
        public List<ClientIdentityConfiguration> IdentityConfigurations { get; } = new();

        public List<ClientIdentityConfiguration> InvalidatedIdentityConfigurations { get; } = new();

        public void AddFakeTokenCredentialSource(IServiceCollection services)
        {
            services.AddSingleton<IAzureTokenCredentialSourceFromDynamicConfiguration>(new FakeTokenSource(this));
        }

        [Given("I reset the fake token credential source")]
        public void GivenIResetTheFakeTokenCredentialSource()
        {
            this.IdentityConfigurations.Clear();
        }

        private class FakeTokenSource : IAzureTokenCredentialSourceFromDynamicConfiguration
        {
            private readonly TokenCredentialSourceBindings parent;

            public FakeTokenSource(TokenCredentialSourceBindings parent)
            {
                this.parent = parent;
            }

            public ValueTask<IAzureTokenCredentialSource> CredentialSourceForConfigurationAsync(
                ClientIdentityConfiguration configuration,
                CancellationToken cancellationToken = default)
            {
                this.parent.IdentityConfigurations.Add(configuration);
                return new ValueTask<IAzureTokenCredentialSource>(new Source(configuration));
            }

            public void InvalidateFailedAccessToken(ClientIdentityConfiguration configuration)
            {
                this.parent.InvalidatedIdentityConfigurations.Add(configuration);
            }

            private class Source : IAzureTokenCredentialSource
            {
                public Source(
                    ClientIdentityConfiguration configuration)
                {
                    this.Configuration = configuration;
                }

                public ClientIdentityConfiguration Configuration { get; }

                public ValueTask<TokenCredential> GetAccessTokenAsync()
                {
                    // This method is deprecated, so nothing should be calling it in this test.
                    throw new NotSupportedException();
                }

                public ValueTask<TokenCredential> GetReplacementForFailedTokenCredentialAsync(CancellationToken cancellationToken = default)
                {
                    // This method is not called in this test, because the caching contexts invalidate at the
                    // sourcefromconfig level.
                    throw new NotSupportedException();
                }

                public ValueTask<TokenCredential> GetTokenCredentialAsync(CancellationToken cancellationToken = default)
                {
                    return new ValueTask<TokenCredential>(new DefaultAzureCredential());
                }
            }
        }
    }
}