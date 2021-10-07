# Storage accounts and authentication

A common requirement across all storage types supported by `Corvus.Storage` is the need to identify which particular storage account to use, and to determine the means by which the application code will authenticate when connecting to that account. And for applications that use multiple storage scopes (e.g., multi-tenanted applications) these account and authentication details might be different for each scope.

Although each storage technology differs in the details, there are a some aspects that are universal, and more that are common:

* Account name
* Access key
* Service identity
* Service-specific logins
* Connection string
  * for storage accounts
  * for service identities
* Associated key vault secret

For many cloud storage services an account name uniquely identifies the service instance. For example, when you create a new Azure Storage account, Azure requires you to pick a name, and it verifies that no other Azure Storage account is already using this name, making it a globally unique identifier. If you know the account name, you can form the primary URLs for the various storage services (blob, queue, etc.). E.g., `https://<accountName>.blob.core.windows.net`, substituting the Azure Storage account name for `<accountName>`. More generally, the account name is often enough to determine the endpoint(s) for using the service.

For most types of access, credentials of some form will be required. One widely used approach is a shared secret, often referred to as an "access key". This is typically a random binary number in a textual representation (e.g., base64). Access keys are problematic. In most Azure services that use them, they are all-or-nothing: either you have the key, and the total power over the account that implies, or you have access only to content that has been configured to be publicly accessible without authentication. This causes two problems. First, it may be impossible to impose a policy of 'least privilege', in which applications have only the level of access they require. And second, because access keys are high-value, they typically need to be changed regularly to limit the damage if a key is leaked. (This process is often called "key rotation.") Distributing shared secrets is a notoriously problematic job, and it is made worse by the need to regularly update all applications that need access to them each time keys are rotated.

For these reasons, Azure has been moving towards a model in which clients authenticate with storage services using Azure AD. Storage services can be configured to grant limited access to one or more accounts in Azure AD. This may sounds like replacing one problem with another near-identical one: instead of distributing access keys, it instead becomes necessary to ensure all applications have access to the credentials required to authenticate through Azure AD. However, this is better in several respects. Firstly, in scenarios where Managed Identities are available, this can completely remove the need to configure application services with sensitive information: applications can authenticate through the Managed Identity system in which Azure fully manages all secrets for you. But even in cases where Managed Identities cannot be used, shifting to Azure AD authentication brings some advantages. Access to individual accounts can be revoked, reducing the value of the credentials held by any particular application; many services offer more fine-grained security when each client service has its own distinct identity, making it possible to grant individual applications just the capabilities they need instead of giving everyone the keys to the kingdom.

Another fairly common model is for storage services to implement their own security system, making it possible to define accounts within, say, a particular database. (This is common with SQL Server, for example. Azure SQL does also support Azure AD integration, but SQL Server logins are still very widely used because they've been available for decades, whereas Azure AD integration is a relatively recent feature.) Typically this means applications will need to store a username and password in addition to any other information required to locate the relevant storage account.

Many services allow the account name and necessary secrets (such as access keys or, for systems such as SQL Server logins, a username and password) to be combined in a single piece of text called a connection string. In some cases this can contain further information. (For example, in an Azure Storage account configured for global availability, the connection string can identify the secondary services.) Many storage APIs provide convenient mechanisms for taking a connection string and producing a ready-to-use object for accessing the relevant service. The attraction of this is that all of the information required to connect to and authenticate with the relevant service can be stored in a single text setting. But this presents a similar problem to the use of access keys: you need to distribute a text string with important secrets in it to all client applications.

Connection strings are not just used for storage accounts. It is also common to use connection strings to hold the information required to use a service identity. (E.g., in cases where a Managed Identity cannot be used, an application might use a connection string that contains an Azure AD Tenant ID, the Application ID, and the Client Secret to use when obtaining a token.)

Many applications in Azure use Azure Key Vault to distribute sensitive settings such as access keys or connection strings (or perhaps other credentials) to application. This avoids the need to store sensitive secrets directly in the application configuration, reducing the chance of such sensitive information being leaked. Azure Key Vault is always secured through Azure AD. The granularity of access control is coarse: various types of access (read, list, modify) can be granted, but it's always across the entire vault. So if a particular identity (a service or user account in Azure AD) is able to read one secret in a Key Vault, it is able to read any secret in that key vault, as long as it know's the secret's name (or has been granted the permission to list secrets). In multi-tenant applications, this can mean we end up wanting multiple key vaults.

Storage account endpoint identification:

* Account name in configuration
* Connection string in configuration
* Connection string in Key Vault

Associated secret (e.g., access key):

* None, because:
  * Authentication is not required (e.g., reading from a public Azure Storage blob container)
  * The connection string supplies the secret
  * We are authenticating as an Azure AD identity
* Secret in configuration
* Secret in Key Vault

Azure AD Identity to use:

* None, because:
  * Authentication is not required (e.g., reading from a public Azure Storage blob container)
  * Authentication is handled by supplying an associated secret
  * Authentication is handled by a secret that is in the connection string
* Application's Managed Identity
* Application-defined Service Principle (that is not a Managed Identity)
* Customer-defined Service Principle (no way this could be a Managed Identity)

In the cases where we're using an azure AD identity that is not a Managed Identity, we also need to choose the form of credentials we will be presenting:

* Client secret
* Certificate

In either case, it will be necessary for the application to get access to the relevant credentials. For a client secret, that will mean getting the secret value into the application. For certificates it might be more indirect: either the relevant certificate will be installed into the compute environment in which the application is running, meaning it could be used via .NET's crypto APIs, or it might installed in Key Vault, in which case the application will need to ask Key Vault to generate a signature. So in practice, for the non-managed Identity cases, it's one of:

* Client secret in configuration
* Client secret in key vault
* Certificate thumbprint in configuration, with certificate and private key installed in application's compute environment
* Certificate in key vault

Observant readers will have noticed we now have a problem: we need these credentials in order to be able to authenticated via Azure AD. But in some of the cases above, those credentials will be in Key Vault...but to make use of credentials in Key Vault, we must authenticate to Key Vault. So in order to obtain credentials for authentication, we must first obtain credentials for authentication! Oops.

In practice, what this means is that we might need a few steps involving different identities, perhaps initially authentication as a Managed Identity to connect to Key Vault to access the credentials required to authenticate as some other identity when accessing the storage account. For example, suppose the customer has stipulated the following, in order to comply with their own security requirements:

* To access the storage account, our application must authenticate as an Azure AD Application defined in the customer tenant ('customer-defined AD application')
* The customer requires us to use certificate-based credentials when authenticating as the customer-defined AD application; we generate the certificates with a lifetime and replacement scheduler agreed with the customer; each time we generate a new certificate, we send it (but not the key) to the customer, and they add it to the credentials list for their customer-defined AD application
* The customer's security policy requires that we store the private key for these certificates in a dedicated Azure Key Vault managed by us, and that is not used with any other customers ('customer-specific application-defined Key Vault')

In this example, we will assume that although the customer requires us to create a Key Vault just for them, they won't demand that we set up a dedicated service principle in our tenant with which to access that Key Vault. The customer-specific application-defined Key Vault will be configured to grant signature generation permission to the Managed Identity (or identities) of whichever application component(s) need to perform this data access.

So the chain of events that needs to occur for the application to access the customer storage account is:

* Generate a JWT suitable for presenting to Azure AD's v2.0 token endpoint ('token request JWT') with the `iss` (issuer) set to the Client ID of the customer-defined AD application
* Use Managed Identity to get an OAuth token from Azure AD for accessing Azure Key Vault
* Invoke the `sign` operation on the customer-specific application-defined Key Vault to generate a signature for our token request JWT; use the token obtained in the preceding step to authenticate to Key Vault as the Managed Identity
* Combine the signature produced by Key Vault with the token request JWT, and send this as the `client_assertion` in a request to the Azure AD v2.0 token endpoint asking for a token with whatever scope is required to access the storage account

The token returned by Azure AD in the final step is the one to use when authenticating requests to the storage service.




## Multi-tenant scenarios with multiple Azure AD tenants

For some multi-tenanted scenarios, customers may want require that certain data be stored in storage accounts that are entirely under their control. For example, imagine an application that can perform analysis over data in an Azure Data Lake. We might have a customer who wants to use this application, and to have it operate directly on data that is already in a Data Lake in their own Azure Subscription, and they do not want to copy it into some other storage account to be able to use the services our application provides.

So there will be two mostly-separate worlds here: a customer Azure Subscription, and our application's Azure Subscription; a customer Azure AD tenant, and our application's Azure AD tenant. (For brevity, we'll refer to the customer subscription, customer tenant, application subscription, and application tenant.) Our application will run in compute resources associated with the application subscription, and if we enable a Managed Identity, that identity will exist in our application tenant. But the Data Lake our customer wants us to use is in a storage account in the customer subscription, and for authentication and access control purposes, it will only recognize identities known to the customer tenant.

In this scenario, the customer is not going to want to supply us with the relevant storage account's access keys. (That might be the simplest technical solution, but unless the storage account in question is being used only for the purposes of integrating with our application, it will be unacceptable from a security perspective. In any case, coordinating key rotation would be problematic.) Instead, they are likely to want to create a service principle in their own Azure AD tenant and have our application authenticate with that identity when accessing their Data Lake. That way they can control the exact level of access our application has to their data. The account with the necessary access is defined in the customer tenant, meaning they have complete control over it, and can revoke it at any time.

The question then becomes: how is our application going to authenticate as the customer-defined service principle in the customer tenant?

One possible answer to this is to use a multi-tenant Azure AD application. (**Note**: multi-tenanting of Azure AD applications is a distinct technical mechanism from the broader idea of a multi-tenanted service. Unfortunately these two similar but different concepts have the same name.) If we define such an application in the application tenant, it is possible to create a service principle associated with that application in the customer tenant. (This is essentially the service principle equivalent of adding a user from an external domain as a guest.) The customer can choose to recognize a multi-tenanted AD application, at which point a new service principle gets created in the customer tenant, but it is associated with the Azure AD application in the application tenant. A significant advantage of this is that the credentials for the application belong to the application tenant, but the customer gets to decide what privileges the application has within the customer tenant, and they are free to revoke the application's membership of the customer tenant at any time. In this model, we retain full ownership of the application credentials (meaning that we do not need to coordinate with the customer in order to determine the mechanism used for authentication—e.g. client ID and password vs certificates—nor to rotate keys or otherwise refresh credentials), but the customer remains in full control of what our application is able to do with their resources. (Typically, they would grant the application no capabilities beyond access to the relevant storage account.)

There are two downsides to multi-tenanted Azure AD applications. The first is that Managed Identities do not (as of September 2021) support multi-tenanting. The second is that some customers will simply refuse to use them. It is therefore necessary to be able to authenticate as 

## Scenarios supported by Corvus.Storage

We do not aim to support every conceivable scenario. The following sections describe the supported scenarios, and the use cases in which these are typically useful.

### Connection string with embedded credentials in configuration

This is intended only for local development because it involves putting secrets into configuration. For production use we recommend avoiding putting valuable credentials into configuration, preferring either to avoid the need for credentials entirely through the use of managed identities, or, in cases where that's not viable, to store all secrets in Azure Key Vault. But for local development, it's common to use strings that can only connect to local service instances. Sometimes, these contain no real secrets at all, at which point, the considerable inconvenience of using a Key Vault for local development purposes would add no value. So local configuration files might contain something like this:

```json
"SomeStorageConfig": {
  "ConnectionStringPlainText": "DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=<account-key>"
}
```


To enable systems using older versions to transition, we need to be able to support the older mechanism. Connection strings for Azure Storage were supported through a somewhat cryptic convention: you left the `AccountKeySecretName` blank, and put the connection string in the `AccountName`:

```jsonc
// Legacy configuration
"SomeStorageConfig": {
  "AccountName": "DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=<account-key>"
}
```

Note that the old `Corvus.Tenancy` libraries did not provide any way to store the 


### Connection string with embedded credentials in a Key Vault accessible to service's Managed Identity


```json
"SomeStorageConfig": {
  "ConnectionStringInKeyVault": { "VaultName": "myvault", "SecretName": "MyStorageConnectionString" }
}
```



### Connection string with embedded credentials in a customer-controller Key Vault, accessed with a separate service principle with client/secret credentials in a Key Vault accessible to service's Managed Identity


```json
"SomeStorageConfig": {
  "ConnectionStringInKeyVault": {
    "VaultName": "someoneelsesvault",
    "SecretName": "MyStorageConnectionString",
    "VaultClientIdentity": {
      "AzureAdAppClientId": "<appIdWithWhichWeAccessClientKeyVault>",
      "AzureAdAppClientSecretInKeyVault" {
        "VaultName": "myvault",
        "SecretName": "ClientSecretForAzureAdAppWithWhichWeAccessClientKeyVault",
        "VaultClientIdentity": { "IdentitySourceType": "Managed" }
      }
    }
  }
}
```

Here w

### Account name with access key

This is intended only for local development because it involves putting secrets into configuration. For production use we recommend avoiding putting valuable credentials into configuration, preferring either to avoid the need for credentials entirely through the use of managed identities, or, in cases where that's not viable, to store all secrets in Azure Key Vault. But for local development, it's common to use strings that can only connect to local service instances. Sometimes, these contain no real secrets at all, at which point, the considerable inconvenience of using a Key Vault for local development purposes would add no value. So local configuration files might contain something like this:

```json
"SomeStorageConfig": {
  "AccountName": "mystorageaccount",
  "AccessKeyPlainText": "<accessKey>"
}
```
### Account name with access key in a Key Vault accessible to service's Managed Identity


```json
"SomeStorageConfig": {
  "AccountName": "mystorageaccount",
  "AccessKeyInKeyVault": { "VaultName": "myvault", "SecretName": "MyAccessKey" }
}
```

### Account name with access key in a customer-controller Key Vault, accessed with a separate service principle with client/secret credentials in a Key Vault accessible to service's Managed Identity


```json
"SomeStorageConfig": {
  "AccountName": "mystorageaccount",
  "AccessKeyInKeyVault": {
    "VaultName": "someoneelsesvault",
    "SecretName": "MyAccessKey",
    "VaultClientIdentity": {
      "AzureAdAppClientId": "<appIdWithWhichWeAccessClientKeyVault>",
      "AzureAdAppClientSecretInKeyVault" {
        "VaultName": "myvault",
        "SecretName": "ClientSecretForAzureAdAppWithWhichWeAccessClientKeyVault",
        "VaultClientIdentity": { "IdentitySourceType": "Managed" }
      }
    }
  }
}
```



### Account name with Azure AD auth using Managed Identity

```json
"SomeStorageConfig": {
  "AccountName": "mystorageaccount",
  "StorageClientIdentity": { "IdentitySourceType": "Managed" }
}
```


### Account name with Azure AD auth using Managed Identity if available, falling back to local dev options

```json
"SomeStorageConfig": {
  "AccountName": "mystorageaccount",
  "StorageClientIdentity": { "IdentitySourceType": "AzureIdentityDefaultAzureCredential" }
}
```

### Account name with Azure AD auth, with service principle client/secret credentials in configuration

Not recommended for production use.

```json
"SomeStorageConfig": {
  "AccountName": "mystorageaccount",
  "StorageClientIdentity": {
    "AzureAdAppClientId": "<appid>",
    "AzureAdAppClientSecretPlainText": "<clientsecret>"
  }
}
```


### Account name with Azure AD auth, with service principle client/secret credentials in a Key Vault accessible to service's Managed Identity

```json
"SomeStorageConfig": {
  "AccountName": "mystorageaccount",
  "StorageClientIdentity": {
    "AzureAdAppClientId": "<appid>",
    "AzureAdAppClientSecretInKeyVault" {
      "VaultName": "myvault",
      "SecretName": "MyAzureAdAppClientSecret" 
    }
  }
}
```


### Account name with Azure AD auth, with service principle client/secret credentials in a customer-controller Key Vault, accessed with a separate service principle with client/secret credentials in a Key Vault accessible to service's Managed Identity


```json
"SomeStorageConfig": {
  "AccountName": "mystorageaccount",
  "StorageClientIdentity": {
    "AzureAdAppClientId": "<appid>",
    "AzureAdAppClientSecretInKeyVault" {
      "VaultName": "someoneelsesvault",
      "SecretName": "CustomerAzureAdAppClientSecret",
      "VaultClientIdentity": {
        "AzureAdAppClientId": "<appIdWithWhichWeAccessClientKeyVault>",
        "AzureAdAppClientSecretInKeyVault" {
          "VaultName": "myvault",
          "SecretName": "ClientSecretForAzureAdAppWithWhichWeAccessClientKeyVault",
          "VaultClientIdentity": { "IdentitySourceType": "Managed" }
        }
      }
    }
  }
}
```


We probably need to generalise across secrets, and then independently enable multiple modes, because there are multiple different kinds of things that need to be handled as secrets.