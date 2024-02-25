using SegregatedStorage.Utilities;

namespace SegregatedStorage.UnitTests.Utilities;

public class FilePathGeneratorTests
{
	[Fact]
	public void GenerateFilePath_Guid_GeneratesPath()
	{
		// Arrange
		var id = Guid.NewGuid();

		// Act
		var path = FilePathGenerator.GenerateFilePath(id);

		// Assert
		Assert.NotEqual(string.Empty, path);
	}
}