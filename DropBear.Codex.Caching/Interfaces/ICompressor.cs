namespace DropBear.Codex.Caching.Interfaces;

public interface ICompressor
{
    byte[] Compress(byte[] data);
    byte[] Decompress(byte[] compressedData);
}