using System.Security.Cryptography;

namespace SegregatedStorage.Utilities;

public sealed class HashingStream : Stream
{
	private readonly IncrementalHash _hash;
	private readonly Stream _inner;

	public HashingStream(Stream inner, HashAlgorithmName algorithm)
	{
		_inner = inner;
		_hash = IncrementalHash.CreateHash(algorithm);
	}

	// Passthroughs
	public override bool CanRead => _inner.CanRead;
	public override bool CanSeek => false;
	public override bool CanWrite => false;
	public override long Length => _inner.Length;
	public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }

	public byte[] GetHashAndReset()
	{
		return _hash.GetHashAndReset();
	}

	// The key override — hash bytes as they're read
	public override int Read(byte[] buffer, int offset, int count)
	{
		var bytesRead = _inner.Read(buffer, offset, count);
		if (bytesRead > 0)
		{
			_hash.AppendData(buffer, offset, bytesRead);
		}

		return bytesRead;
	}

	public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
	{
		var bytesRead = await _inner.ReadAsync(buffer, offset, count, ct);
		if (bytesRead > 0)
		{
			_hash.AppendData(buffer, offset, bytesRead);
		}

		return bytesRead;
	}

	public async override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
	{
		var bytesRead = await _inner.ReadAsync(buffer, ct);
		if (bytesRead > 0)
		{
			_hash.AppendData(buffer.Span[..bytesRead]);
		}

		return bytesRead;
	}

	public override void Flush()
	{
		_inner.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_inner.Dispose();
			_hash.Dispose();
		}

		base.Dispose(disposing);
	}
}