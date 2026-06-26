using System.Security.Cryptography;
using SegregatedStorage.Utilities;

namespace SegregatedStorage.UnitTests.Utilities;

public class HashingStreamTests
{
	private static byte[] ComputeMd5(byte[] data)
	{
		return MD5.HashData(data);
	}

	private static MemoryStream MakeStream(byte[] data)
	{
		return new MemoryStream(data);
	}

	// ── Correctness ────────────────────────────────────────────────────────────

	[Fact]
	public void Read_SmallPayload_ProducesCorrectMd5()
	{
		// Arrange
		var data = "hello world"u8.ToArray();
		using var inner = MakeStream(data);
		using var sut = new HashingStream(inner, HashAlgorithmName.MD5);

		// Act
		sut.CopyTo(Stream.Null);

		// Assert
		Assert.Equal(ComputeMd5(data), sut.GetHashAndReset());
	}

	[Fact]
	public async Task ReadAsyncMemory_FullPayload_ProducesCorrectMd5()
	{
		// Arrange
		var data = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
		using var inner = MakeStream(data);
		await using var sut = new HashingStream(inner, HashAlgorithmName.MD5);

		// Act
		await sut.CopyToAsync(Stream.Null);

		// Assert
		Assert.Equal(ComputeMd5(data), sut.GetHashAndReset());
	}

	[Fact]
	public async Task ReadAsyncByteArray_FullPayload_ProducesCorrectMd5()
	{
		// Arrange
		var data = "unit test payload"u8.ToArray();
		using var inner = MakeStream(data);
		using var sut = new HashingStream(inner, HashAlgorithmName.MD5);

		// Act
		var buffer = new byte[4];
		while (await sut.ReadAsync(buffer, 0, buffer.Length) > 0) { }

		// Assert
		Assert.Equal(ComputeMd5(data), sut.GetHashAndReset());
	}

	[Fact]
	public void Read_LargePayloadWithSmallBuffer_ProducesCorrectMd5()
	{
		// Arrange
		var data = new byte[10_000];
		Random.Shared.NextBytes(data);
		using var inner = MakeStream(data);
		using var sut = new HashingStream(inner, HashAlgorithmName.MD5);

		// Act
		var buffer = new byte[128];
		while (sut.Read(buffer, 0, buffer.Length) > 0) { }

		// Assert
		Assert.Equal(ComputeMd5(data), sut.GetHashAndReset());
	}

	[Fact]
	public void Read_EmptyStream_ProducesEmptyStreamMd5()
	{
		// Arrange
		using var inner = MakeStream([]);
		using var sut = new HashingStream(inner, HashAlgorithmName.MD5);

		// Act
		sut.CopyTo(Stream.Null);

		// Assert
		Assert.Equal(ComputeMd5([]), sut.GetHashAndReset());
	}

	// ── GetHashAndReset behaviour ──────────────────────────────────────────────

	[Fact]
	public void GetHashAndReset_CalledTwiceAfterRead_SecondCallReturnsEmptyStreamHash()
	{
		// Arrange
		var data = "some data"u8.ToArray();
		using var inner = MakeStream(data);
		using var sut = new HashingStream(inner, HashAlgorithmName.MD5);
		sut.CopyTo(Stream.Null);

		// Act
		sut.GetHashAndReset();
		var secondHash = sut.GetHashAndReset();

		// Assert
		Assert.Equal(ComputeMd5([]), secondHash);
	}

	// ── Stream capability flags ────────────────────────────────────────────────

	[Fact]
	public void CanRead_Always_IsTrue()
	{
		// Arrange
		using var sut = new HashingStream(MakeStream([]), HashAlgorithmName.MD5);

		// Act
		var result = sut.CanRead;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void CanSeek_Always_IsFalse()
	{
		// Arrange
		using var sut = new HashingStream(MakeStream([]), HashAlgorithmName.MD5);

		// Act
		var result = sut.CanSeek;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void CanWrite_Always_IsFalse()
	{
		// Arrange
		using var sut = new HashingStream(MakeStream([]), HashAlgorithmName.MD5);

		// Act
		var result = sut.CanWrite;

		// Assert
		Assert.False(result);
	}

	// ── Unsupported operations throw ───────────────────────────────────────────

	[Fact]
	public void Seek_Always_ThrowsNotSupportedException()
	{
		// Arrange
		using var sut = new HashingStream(MakeStream([]), HashAlgorithmName.MD5);

		// Act & Assert
		Assert.Throws<NotSupportedException>(() => sut.Seek(0, SeekOrigin.Begin));
	}

	[Fact]
	public void Write_Always_ThrowsNotSupportedException()
	{
		// Arrange
		using var sut = new HashingStream(MakeStream([]), HashAlgorithmName.MD5);

		// Act & Assert
		Assert.Throws<NotSupportedException>(() => sut.Write([1, 2, 3], 0, 3));
	}

	[Fact]
	public void SetLength_Always_ThrowsNotSupportedException()
	{
		// Arrange
		using var sut = new HashingStream(MakeStream([]), HashAlgorithmName.MD5);

		// Act & Assert
		Assert.Throws<NotSupportedException>(() => sut.SetLength(10));
	}

	[Fact]
	public void SetPosition_Always_ThrowsNotSupportedException()
	{
		// Arrange
		using var sut = new HashingStream(MakeStream([1, 2, 3]), HashAlgorithmName.MD5);

		// Act & Assert
		Assert.Throws<NotSupportedException>(() => sut.Position = 0);
	}

	// ── Works with other algorithms ────────────────────────────────────────────

	[Fact]
	public void Read_WithSha256Algorithm_ProducesCorrectSha256()
	{
		// Arrange
		var data = "sha test"u8.ToArray();
		using var inner = MakeStream(data);
		using var sut = new HashingStream(inner, HashAlgorithmName.SHA256);

		// Act
		sut.CopyTo(Stream.Null);

		// Assert
		Assert.Equal(SHA256.HashData(data), sut.GetHashAndReset());
	}
}