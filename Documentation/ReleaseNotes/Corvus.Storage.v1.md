# Release notes for Corvus.Storage v1.

# V1.3

NuGet updates:

* `Azure.Storage.Blobs` 12.12 -> 12.13
* `Microsoft.Azure.Cosmos` 3.28 -> 3.30
* `Microsoft.Data.SqlClient` 4.1 -> 5.0

Normally that SqlClient upgrade would warrant a major version bump. However, we aren't currently using `Corvus.Storage.Sql` anywhere, and we don't want to disrupt all other uses of `Corvus.Storage`.

# V1.2

Adds `Corvus.Storage.Azure.TableStorage` package.

# V1.1

Adds `Corvus.Storage.Sql` package.

## V1.0

Initial release