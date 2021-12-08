# Key Vault Access Uses Default Client Settings

The `SecretClient` type in the Azure Key Vault client SDK is used with its default settings.

## Status

Proposed

## Context

`Corvus.Storage` mostly doesn't use external services itself—its main job is to acquire suitably cached and configured objects from various storage client SDKs, and it's the application that uses them. However, it does need to talk to Azure Key Vault, because one of the supported modes of configuration is that a storage account connection string might be stored in an Azure Key Vault. `Corvus.Storage` will need to retrieve that to be able to configure correctly the objects it supplies to the application.

This means that `Corvus.Storage` needs to work with actual instances of `SecretClient`. This in turn means having some particular position on all of the facets of the settings available through `SecretClientOptions`. Of particular interest are the following facets of the communication pipeline that the `SecretClient` uses:

* Authentication
* Retry policy
* Telemetry
* Logging

We necessarily have to take control of authentication, because we need to present the correct credentials to Key Vault. We defer to `Corvus.Identity` for this, and just use the `TokenCredential` that returns for the configuration that has been supplied to us. But everything else is potentially up for grabs.

## Decision

We do not pass a `SecretClientOptions` when constructing a `SecretClient`, meaning that we get the defaults.

## Consequences

The default retry policy has the following characteristics:

* Exponential backoff, starting at 8 seconds
* Maximum of 3 retries
* Maximum delay of 1 minute

Since we never try to modify what's in Key Vault, there are no concerns around retries having potentially unintended consequences, so these defaults seem reasonable.

Since the `SecretClient` ultimately sends messages via `HttpClient`, the usual interception mechanism employed by Application Insights (or any other system using the same technique) will work.

The default client settings enable logging, and the `Azure.Core` implementation uses the `System.Diagnostics` `EventSource` mechanism. So if anything chooses to listen to events for `Azure.Security.KeyVault.Secrets` through that mechanism, they will be able to see the `SecretClient`'s activity. (And if nothing is listening, then no log information is generated.)
