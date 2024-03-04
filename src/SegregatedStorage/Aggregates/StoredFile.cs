namespace SegregatedStorage.Aggregates;

public record StoredFile
{
	public Guid Id { get; private init; }
	public string FileName { get; private init; } = default!;
	public string MimeType { get; private init; } = default!;
	public FileState State { get; private init; }

	public static StoredFile Create(Guid id, string filename, string mimeType)
	{
		return new StoredFile
		{
			Id = id,
			FileName = filename,
			MimeType = mimeType,
			State = FileState.AwaitingUpload
		};
	}

	public StoredFile Delete()
	{
		return this with
		{
			State = FileState.Deleting
		};
	}

	public StoredFile Uploaded()
	{
		return this with
		{
			State = FileState.Available
		};
	}
}