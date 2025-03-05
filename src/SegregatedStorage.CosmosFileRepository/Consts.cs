using SegregatedStorage.Aggregates;

namespace SegregatedStorage;

static internal class Consts
{
	public static readonly string PartitionKey = $"/{nameof(StoredFile.State).ToLower()}";
}