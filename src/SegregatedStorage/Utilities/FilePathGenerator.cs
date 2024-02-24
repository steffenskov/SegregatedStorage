namespace SegregatedStorage.Utilities;

internal static class FilePathGenerator
{
	public static string CreateFilePath(Guid id)
	{
		return id.ToString();
	}
}