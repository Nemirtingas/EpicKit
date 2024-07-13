using System;
using System.Security.Cryptography;

namespace EpicKit.TitleStorageApi;

public class EncryptedFile : IDisposable
{
    private bool _disposed = false;
    private byte[] _buffer;

    public EncryptedTitleStorageHeader? Header { get; internal set; }
    public ReadOnlySpan<byte> Tag => new ReadOnlySpan<byte>(_buffer, (int)Header.Value.FileSize, EncryptedTitleStorageHeader.TagSize);
    public ReadOnlySpan<byte> Content => new ReadOnlySpan<byte>(_buffer, 0, (int)Header.Value.FileSize);

    private void _CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EncryptedFile));
    }

    private void _ReadHeader(Stream stream)
    {
        if (stream.Length < EncryptedTitleStorageHeader.EncryptedHeaderSize)
            throw new InvalidDataException();

        var binaryReader = new BinaryReader(stream);
        Header = Shared.FromBinaryReader<EncryptedTitleStorageHeader>(binaryReader);

        if (Header.Value.Magic != EncryptedTitleStorageHeader.MagicValue || Header.Value.Version > 1 || Header.Value.HeaderSize != EncryptedTitleStorageHeader.EncryptedHeaderSize)
            throw new InvalidDataException();
    }

    public void ReadFile(Stream stream)
    {
        _CheckDisposed();

        var binaryReader = new BinaryReader(stream);
        _ReadHeader(stream);

        var fileBytesLeft = (int)(stream.Length - stream.Position);
        if (_buffer == null || _buffer.Length < fileBytesLeft)
            _buffer = new byte[fileBytesLeft];

        binaryReader.Read(_buffer, 0, fileBytesLeft);
    }

    public byte[] Decipher(byte[] key)
    {
        _CheckDisposed();

        var plaintextBytes = new byte[Content.Length];
        using (var aesGcm = new AesGcm(key, Tag.Length))
        {
            aesGcm.Decrypt((ReadOnlySpan<byte>)Header.Value.IV, Content, Tag, plaintextBytes);
        }

        return plaintextBytes;
    }

    public byte[] ReadFileAndDecipher(Stream stream, byte[] key)
    {
        ReadFile(stream);
        return Decipher(key);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _buffer = null;
        Header = null;
    }
}
