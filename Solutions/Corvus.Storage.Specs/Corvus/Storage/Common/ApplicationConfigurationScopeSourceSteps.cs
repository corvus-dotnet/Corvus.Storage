// <copyright file="ApplicationConfigurationScopeSourceSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Common
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TechTalk.SpecFlow;

    [Binding]
    public class ApplicationConfigurationScopeSourceSteps : IDisposable
    {
        private readonly Dictionary<string, string> configurationSettings = new ();
        private ServiceProvider? serviceProvider;
        private ISingularScopeSource? scopeSource;
        private IStorageContextScope<FakeConfigurationOptions>? scope;
        private Exception? exceptionFromAttemptToFetchScope;
        private FakeConfigurationOptions? configuration;
        private string? cacheKey;

        private IStorageContextScope<FakeConfigurationOptions> Scope => this.scope ??
            throw new InvalidOperationException("Step initializing scope has not been run.");

        [Given("a configuration setting of '(.*)' at path '(.*)' has a value of '(.*)'")]
        public void GivenAConfigurationSettingOfAtPathHasAValueOf(string settingName, string sectionPath, object value)
        {
            this.configurationSettings[GetSettingName(settingName, sectionPath)] = value.ToString() !;
        }

        [Given("a configuration-based scope source with section path '(.*)'")]
        public void GivenAConfiguration_BasedScopeSourceWithSectionPath(string sectionPath)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(this.configurationSettings);
            IConfigurationRoot config = configBuilder.Build();

            var services = new ServiceCollection();
            services.AddStorageScopeSourceFromConfiguration(
                config,
                GetNullable(sectionPath));
            this.serviceProvider = services.BuildServiceProvider();

            this.scopeSource = this.serviceProvider.GetRequiredService<ISingularScopeSource>();
        }

        [When("a storage scope for FakeConfigurationOptions is fetched")]
        public void WhenAStorageScopeForFakeConfigurationOptionsIsFetched()
        {
            ISingularScopeSource scopeSource = this.scopeSource
                ?? throw new InvalidOperationException("Step initializing scope source has not been run.");

            this.scope = scopeSource.For<FakeConfigurationOptions>();
        }

        [When("attempting to fetch a storage scope for FakeConfigurationOptions")]
        public void WhenAttemptingToFetchAStorageScopeForFakeConfigurationOptions()
        {
            try
            {
                this.WhenAStorageScopeForFakeConfigurationOptionsIsFetched();
            }
            catch (Exception x)
            {
                this.exceptionFromAttemptToFetchScope = x;
            }
        }

        [When("the storage scope's configuration is fetched for context '(.*)'")]
        public void WhenTheStorageScopeConfigurationIsFetchedFor(string contextName)
        {
            this.configuration = this.Scope.GetConfigurationForContext(contextName);
        }

        [When("the storage scope's cache key is fetched for context '(.*)'")]
        public void WhenTheStorageScopeCacheKeyIsFetchedFor(string contextName)
        {
            this.cacheKey = this.Scope.CreateCacheKeyForContext(contextName);
        }

        [Then("the property '(.*)' of the configuration returned by the storage scope should have the value '(.*)'")]
        public void ThenThePropertyOfTheConfigurationReturnedByTheStorageScopeShouldHaveTheValue(
            string propertyName, string expectedValue)
        {
            PropertyInfo pi = typeof(FakeConfigurationOptions).GetProperty(propertyName)
                ?? throw new ArgumentException($"Property {propertyName} not found", nameof(propertyName));
            string? actualValue = (string?)pi.GetValue(this.configuration);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Then("the property '(.*)' of the configuration returned by the storage scope should have a null value")]
        public void ThenThePropertyOfTheConfigurationReturnedByTheStorageScopeShouldHaveANullValue(string propertyName)
        {
            PropertyInfo pi = typeof(FakeConfigurationOptions).GetProperty(propertyName)
                ?? throw new ArgumentException($"Property {propertyName} not found", nameof(propertyName));
            string? actualValue = (string?)pi.GetValue(this.configuration);
            Assert.IsNull(actualValue);
        }

        [Then("the property '(.*)' of the configuration returned by the storage scope should have the value  '(.*)'")]
        public void ThenThePropertyOfTheConfigurationReturnedByTheStorageScopeShouldHaveTheValue(string propertyName, int expectedValue)
        {
            PropertyInfo pi = typeof(FakeConfigurationOptions).GetProperty(propertyName)
                ?? throw new ArgumentException($"Property {propertyName} not found", nameof(propertyName));
            int actualValue = (int)pi.GetValue(this.configuration) !;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Then("the cache key returned by the storage scope should be '(.*)'")]
        public void ThenTheCacheKeyReturnedByTheStorageScopeShouldBe(string expectedValue)
        {
            Assert.AreEqual(expectedValue, this.cacheKey);
        }

        [Then(@"ISingularScopeSource\.For should throw an InvalidOperationException")]
        public void ThenISingularScopeSource_ForShouldThrowAnInvalidOperationException()
        {
            Assert.IsInstanceOf<InvalidOperationException>(this.exceptionFromAttemptToFetchScope);
        }

        void IDisposable.Dispose()
        {
            if (this.serviceProvider != null)
            {
                this.serviceProvider.Dispose();
            }
        }

        private static string GetSettingName(string settingName, string sectionPath) =>
            string.IsNullOrWhiteSpace(GetNullable(sectionPath))
                ? settingName
                : $"{sectionPath}:{settingName}";

        private static string? GetNullable(string value) => value == "<null>" ? null : value;
    }
}