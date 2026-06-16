
namespace EpicKit.ManifestApi;

[Flags]
public enum ManifestStoreFlags : byte
{
    None = 0x00,
    Deflate = 0x01,
    Encrypted = 0x02,
}
