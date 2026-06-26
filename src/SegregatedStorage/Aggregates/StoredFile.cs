namespace SegregatedStorage.Aggregates;

public record StoredFile
{
	public Guid Id { get; private init; }
	public string FileName { get; private init; } = default!;
	public string MimeType { get; private init; } = default!;
	public FileState State { get; private init; }

	public string FileHash
	{
		get => field ?? ""; // Backwards compatibility for persisted StoredFiles where Hash didn't exist
		private init;
	} = "";

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

	public StoredFile Uploaded(byte[] fileHash)
	{
		if (State != FileState.AwaitingUpload)
		{
			throw new InvalidOperationException($"StoredFile is not awaiting upload!. Aggregate: {this}");
		}

		return this with
		{
			State = FileState.Available,
			FileHash = Convert.ToHexString(fileHash)
		};
	}

	public StoredFile Rename(string filename)
	{
		if (string.IsNullOrWhiteSpace(filename))
		{
			throw new ArgumentException("filename cannot be null or whitespace", nameof(filename));
		}

		return this with
		{
			FileName = filename
		};
	}
}