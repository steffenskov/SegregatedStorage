namespace SegregatedStorage.Cosmos.IntegrationTests.Fixtures;

[CollectionDefinition(nameof(ConfigurationCollection))]
public class ConfigurationCollection : ICollectionFixture<ContainerFixture>;