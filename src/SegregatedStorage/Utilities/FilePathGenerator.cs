namespace SegregatedStorage.Utilities;

internal static class FilePathGenerator
{
	public static string GenerateFilePath(Guid id)
	{
		return id.ToString();
	}
}