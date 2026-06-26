using System.Security.Cryptography;

namespace SegregatedStorage.Configuration;

public class StorageServiceConfiguration
{
	/// <summary>
	///     Whether or not to add a HostedService for periodically deleting files marked for deletion.
	///     (Invoking DeleteAsync on IStorageService only marks files for deletion, the service takes care of the actual
	///     deletion.)
	///     Defaults to true
	/// </summary>
	public bool IncludeDeletionBackgroundService { get; set; } = true;

	/// <summary>
	///     Hashing algorithm to use for calculating FileHash stored onto StoredFiles.
	///     Defaults to MD5
	/// </summary>
	public HashAlgorithmName HashAlgorithm { get; set; } = HashAlgorithmName.MD5;
}