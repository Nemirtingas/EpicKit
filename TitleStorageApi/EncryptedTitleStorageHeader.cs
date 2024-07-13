using System.Runtime.InteropServices;

namespace EpicKit.TitleStorageApi;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EncryptedTitleStorageHeader
{
    public const ushort MagicValue = 0x789C;
    public const int TagSize = 16;
    public const int EncryptedHeaderSize = 42;

    public ushort Magic;
    public ushort Version;
    public ushort HeaderSize;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public byte[] IV;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] Unknown2;
    public ulong FileSize;
}

