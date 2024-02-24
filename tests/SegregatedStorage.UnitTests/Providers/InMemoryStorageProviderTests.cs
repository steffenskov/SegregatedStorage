using SegregatedStorage.Providers;

namespace SegregatedStorage.UnitTests.Providers;

public class InMemoryStorageProviderTests
{
	[Theory]
	[InlineData("126B495A-5D91-40E4-932E-B399A57CFBB7")]
	public async Task UploadAsync_NewFilePath_IsCreated(string filePath)
	{
		// Arrange
		var provider = new InMemoryStorageProvider();
		var bytes = "Hello world"u8.ToArray();
		var stream = new MemoryStream(bytes);

		// Act
		await provider.UploadAsync(filePath, stream);

		// Assert
		var fetched = await provider.DownloadAsync(filePath);
		var ms = new MemoryStream();
		await fetched.CopyToAsync(ms);
		ms.Seek(0, SeekOrigin.Begin);
		var fetchedBytes = ms.ToArray();

		Assert.True(bytes.SequenceEqual(fetchedBytes));
	}
}