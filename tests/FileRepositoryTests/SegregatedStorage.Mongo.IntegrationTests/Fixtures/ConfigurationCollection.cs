namespace SegregatedStorage.Mongo.IntegrationTests.Fixtures;

[CollectionDefinition(nameof(ConfigurationCollection))]
public class ConfigurationCollection : ICollectionFixture<ContainerFixture>;