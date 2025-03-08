using SegregatedStorage.Aggregates;

namespace SegregatedStorage;

static internal class Consts
{
	public const string EmulatorDefaultAccountKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

	public const string PartitionKey = "partitionKey";

	public static readonly string PartitionKeyPath = $"/{nameof(CosmosStoredFile.PartitionKey).ToCamelCase()}";
}

static file class Extensions
{
	public static string ToCamelCase(this string s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return s;
		}

		return char.ToLower(s[0]) + s[1..];
	}
}