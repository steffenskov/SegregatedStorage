using SegregatedStorage.Configuration;

namespace SegregatedStorage.UnitTests.Configuration;

public class ApiConfigurationTests
{
	[Theory]
	[InlineData("foo/")]
	[InlineData("bar$")]
	[InlineData("foo#bar")]
	public void EndpointPrefix_WithNonAlphaLetters_Throws(string prefix)
	{
		// Arrange
		var config = new ApiConfiguration();
		
		// Act && Assert
		Assert.Throws<ArgumentException>(() => config.EndpointPrefix = prefix);
	}
	
	[Theory]
	[InlineData("foo42")]
	[InlineData("bar")]
	public void EndpointPrefix_Valid_Works(string prefix)
	{
		// Arrange
		var config = new ApiConfiguration();
		
		// Act
		var ex= Record.Exception(() => config.EndpointPrefix = prefix);
		
		// Assert
		Assert.Null(ex);
	}
}