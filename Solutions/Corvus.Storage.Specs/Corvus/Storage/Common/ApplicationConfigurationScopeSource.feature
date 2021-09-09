Feature: ApplicationConfigurationScopeSource
	As a developer
	Who wants to use Corvus.Storage and does not need multiple scopes
	I want an ISingularScopeSource that reads configuration settings from IConfiguration

Scenario Outline: Settings in configuration
	Given a configuration setting of 'FakeConfigurationOptions:StringProperty1' at path '<sectionPath>' has a value of 'Test text'
	And a configuration setting of 'FakeConfigurationOptions:NumericProperty1' at path '<sectionPath>' has a value of '42'
	And a configuration-based scope source with section path '<sectionPath>'
	When a storage scope for FakeConfigurationOptions is fetched
	And the storage scope's configuration is fetched for context '<contextName>'
	And the storage scope's cache key is fetched for context '<contextName>'
	Then the property 'StringProperty1' of the configuration returned by the storage scope should have the value 'Test text'
	Then the property 'StringProperty2' of the configuration returned by the storage scope should have a null value
	And the property 'NumericProperty1' of the configuration returned by the storage scope should have the value  '42'
	And the cache key returned by the storage scope should be '<contextName>'

	Examples:
		| sectionPath | contextName |
		| <null>      | foo         |
		| <null>      | bar         |
		| OneLevel    | foo         |
		| OneLevel    | bar         |
		| Two:Levels  | foo         |
		| Two:Levels  | bar         |

Scenario: Missing settings at configuration root
	Given a configuration-based scope source with section path '<null>'
	When attempting to fetch a storage scope for FakeConfigurationOptions
	Then ISingularScopeSource.For should throw an InvalidOperationException

# A configuration section has been specified, and it is present, but the particular type of
# configuration requested is not present.
Scenario Outline: Missing settings in a configuration section
	Given a configuration setting of 'My:Path:OtherSettings:UnrelatedProperty' at path '<sectionPath>' has a value of 'Test text'
	And a configuration-based scope source with section path '<sectionPath>'
	When attempting to fetch a storage scope for FakeConfigurationOptions
	Then ISingularScopeSource.For should throw an InvalidOperationException

	Examples:
		| sectionPath |
		| OneLevel    |
		| Two:Levels  |

# A configuration section has been specified, and is not present.
Scenario: Missing configuration section
	Given a configuration-based scope source with section path '<sectionPath>'
	When attempting to fetch a storage scope for FakeConfigurationOptions
	Then ISingularScopeSource.For should throw an InvalidOperationException

	Examples:
		| sectionPath |
		| OneLevel    |
		| Two:Levels  |
