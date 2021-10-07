# Storage context configuration

At the lowest level, the various storage-technology-specific libraries in `Corvus.Storage` take some configuration object and return a context object of the corresponding type. For example, if you've got a `BlobContainerConfiguration`, these libraries will give you a `BlobContainerClient` (an Azure SDK type) providing access to the container described by the configuration object.

