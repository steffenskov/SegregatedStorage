namespace SegregatedStorage.Configuration;

public class StorageServiceConfiguration
{
	/// <summary>
	/// Whether or not to add a HostedService for periodically deleting files marked for deletion.
	/// (Invoking DeleteAsync on IStorageService only marks files for deletion, the service takes care of the actual deletion.)
	/// Defaults to true
	/// </summary>
	public bool IncludeDeletionBackgroundService { get; set; } = true;
}