using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace SegregatedStorage.Serializers;

internal class CosmosSerializer : Microsoft.Azure.Cosmos.CosmosSerializer
{
	private readonly JsonSerializer _serializer;

	public CosmosSerializer()
	{
		var settings = new JsonSerializerSettings
		{
			ContractResolver = new PrivateSetterCamelCasePropertyNamesContractResolver(),
			Converters = { new StringEnumConverter() },
			TypeNameHandling = TypeNameHandling.Auto
		};

		_serializer = JsonSerializer.Create(settings);
	}

	public override T FromStream<T>(Stream stream)
	{
		using var sr = new StreamReader(stream);
		using var jsonTextReader = new JsonTextReader(sr);

		return _serializer.Deserialize<T>(jsonTextReader)!;
	}

	public override Stream ToStream<T>(T input)
	{
		var stream = new MemoryStream();
		using (var sw = new StreamWriter(stream, leaveOpen: true))
		using (var jsonTextWriter = new JsonTextWriter(sw))
		{
			_serializer.Serialize(jsonTextWriter, input);
		}

		stream.Seek(0, SeekOrigin.Begin);

		return stream;
	}
}

file class PrivateSetterCamelCasePropertyNamesContractResolver : CamelCasePropertyNamesContractResolver
{
	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		var property = base.CreateProperty(member, memberSerialization).MakeWriteable(member);

		if (property.PropertyType?.IsClass == true && member is PropertyInfo propertyInfo)
		{
			if (propertyInfo.GetMethod?.IsFamily == true || propertyInfo.GetMethod?.IsFamilyOrAssembly == true)
			{
				property.Readable = true;
			}
		}

		return property;
	}
}

static file class Extensions
{
	public static JsonProperty MakeWriteable(this JsonProperty jProperty, MemberInfo member)
	{
		if (jProperty.Writable)
		{
			return jProperty;
		}

		jProperty.Writable = IsPropertyWithSetter(member);

		return jProperty;
	}

	private static bool IsPropertyWithSetter(MemberInfo member)
	{
		var property = member as PropertyInfo;

		return property?.SetMethod != null;
	}
}