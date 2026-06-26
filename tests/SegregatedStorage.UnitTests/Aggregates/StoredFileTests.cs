using Newtonsoft.Json;

namespace SegregatedStorage.UnitTests.Aggregates;

public class StoredFileTests
{
	[Fact]
	public void DeserializedFromJson_NoFileHash_FileHashNotNull()
	{
		// Arrange
		var json = """
		           {
		             "Id" : "e9d6703c-da04-49b8-9d88-4133bed2c04c",
		             "FileName" : "hello.txt",
		             "MimeType" : "text/plain",
		           }
		           """;

		// Act
		var deserialized = JsonConvert.DeserializeObject<StoredFile>(json);

		// Assert
		Assert.NotNull(deserialized);
		Assert.NotNull(deserialized.FileHash);
	}

	[Fact]
	public void Uploaded_AlreadyUploaded_Throws()
	{
		// Arrange
		var file = StoredFile.Create(Guid.NewGuid(), "hello.txt", "text/plain");
		file = file.Uploaded([]);

		// Act && Assert
		var ex = Assert.Throws<InvalidOperationException>(() => file.Uploaded([]));

		Assert.StartsWith("StoredFile is not awaiting upload!.", ex.Message);
	}
}