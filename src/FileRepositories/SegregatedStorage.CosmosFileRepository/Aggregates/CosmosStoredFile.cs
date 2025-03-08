namespace SegregatedStorage.Aggregates;

internal record CosmosStoredFile : StoredFile
{
	public CosmosStoredFile(StoredFile storedFile) : base(storedFile) // Copy constructor
	{
	}

	// Required for deserialization
	// ReSharper disable once UnusedMember.Global
	public CosmosStoredFile()
	{
	}

	public string PartitionKey { get; private init; } = Consts.PartitionKey;
}