Feature: TableNaming
    As a developer using blob storage
    In order to satisfy Azure Blob Storage's restrictions around container naming
    I need to be able to convert logical names into container names that do not break the rules

# In Corvus tenancy, the root tenant id is f26450ab1668784bb327951c8b08f347. The convention is
# to append a hyphen followed by the logical table name to form the tenanted logical table
# name, e.g. 'f26450ab1668784bb327951c8b08f347-mytable'. The hashed version of this name prefixed
# with "t" is the name we use in practice. Hashing reduces it to fit the size constraints. (Although
# Cosmos DB's table store allows names up to 254 characters, Azure Storage Tables have a limit of
# 63 characters.) The "t" prefix is there to deal with the fact that Azure Storage requires a table
# name to begin with a letter. (The rest of the name can be a mixture of letters and numbers, but
# it must start with a letter.)
Scenario: Name for table in root tenant is hash of tenanted name
    Given a logical table name of 'f26450ab1668784bb327951c8b08f347-mytable'
    When the logical name is passed to AzureTableNaming.HashAndEncodeContainerName
    Then the resulting table name is 'tf82134e4477d6c845eff0e1994f32896ef349d47'

# In practice it is rare for systems to work directly against the root tenant (especially any Marain
# services), and since tenant names get longer once you get beyond direct children of the root,
# we should check that the physical names containue to be as short as required.
Scenario: Name for table in child tenant is hash of tenanted name
    Given a logical table name of '5d24a47b6af0cb4b8f19dea061f3213114f113704acd824590a56e324bedcdff-corvustenancy'
    When the logical name is passed to AzureTableNaming.HashAndEncodeContainerName
    Then the resulting table name is 't29fdf9f0c9ddaa45b1828b9daf6adfaee524c2ce'
