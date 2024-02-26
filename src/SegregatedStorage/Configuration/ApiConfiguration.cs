using System.Text.RegularExpressions;

namespace SegregatedStorage.Configuration;

public partial class ApiConfiguration
{
	private string _endpointPrefix = "file";
	private static readonly Regex _endpointPrefixRegex = WordRegex();

	/// <summary>
	///     Whether or not to disable the built-in (MVC) Anti-Forgery Token functionality.
	/// </summary>
	public bool DisableAntiForgery { get; set; }

	/// <summary>
	///     Prefix for API Endpoints, defaults to "file"
	/// </summary>
	public string EndpointPrefix
	{
		get => _endpointPrefix;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new ArgumentException($"{nameof(EndpointPrefix)} cannot be null or empty!", nameof(value));
			
			if (!_endpointPrefixRegex.IsMatch(value))
				throw new ArgumentException($"{nameof(EndpointPrefix)} can only contain alpha-numeric characters!", nameof(value));

			_endpointPrefix = value;
		}
	}

    [GeneratedRegex(@"^\w+$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline, "en-DK")]
    private static partial Regex WordRegex();
}