using SegregatedStorage.Aggregates;

namespace SegregatedStorage;

static internal class Consts
{
	public const string EmulatorDefaultAccountKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
	public static readonly string PartitionKey = $"/{nameof(StoredFile.State).ToLower()}";
}