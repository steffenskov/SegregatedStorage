namespace SegregatedStorage.Aggregates;

public record FileAggregate
{
	public Guid Id { get; private init; }
	public string FileName { get; private init; } = default!;
	public string MimeType { get; private init; } = default!;
	public FileState State { get; private init; }

	public static FileAggregate Create(Guid id, string filename, string mimeType)
	{
		return new FileAggregate
		{
			Id = id,
			FileName = filename,
			MimeType = mimeType,
			State = FileState.AwaitingUpload
		};
	}

	public FileAggregate Delete()
	{
		return this with
		{
			State = FileState.Deleting
		};
	}

	public FileAggregate Uploaded()
	{
		return this with
		{
			State = FileState.Available
		};
	}
}