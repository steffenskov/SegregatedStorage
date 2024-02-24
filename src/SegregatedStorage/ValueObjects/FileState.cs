namespace SegregatedStorage.ValueObjects;

public enum FileState : byte
{
	AwaitingUpload = 0,
	Available = 1,
	Deleting = 2
}