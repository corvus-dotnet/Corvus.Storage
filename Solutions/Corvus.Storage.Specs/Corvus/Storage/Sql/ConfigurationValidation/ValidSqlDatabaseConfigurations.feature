Feature: ValidSqlDatabaseConfiguration
    As the person responsible for deploying and configuring an application that uses SQL Server or Azure SQL databases
    I need to configuration that supplies the necessary details and credentials in various different ways to be accepted as valid
    So that I can connect to the correct storage account while meeting the security requirements of my application


Scenario Outline: Plain text connection string
    Given a SqlDatabaseConfiguration
    And a SqlDatabaseConfiguration.ConnectionStringPlainText of 'Server=(localdb)\\mssqllocaldb;Trusted_Connection=True'
    And SqlDatabaseConfiguration.ClientIdentity set to use '<clientIdentityType>'
    When I validate the SqlDatabaseConfiguration
    Then the SqlDatabaseConfiguration should succeed, and type should be 'ConnectionStringAsPlainText'

    Examples:
        | clientIdentityType |
        | None               |
        | Managed            |
