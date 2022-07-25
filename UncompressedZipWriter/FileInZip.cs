using System.Text;

record FileInZip(string Name, Stream Stream, long Size, DateTime LastModified)
{
    public uint Offset { get; set; } = 0;
    public ushort TimeBits { get; set; } = 0;
    public ushort DateBits { get; set; } = 0;
    public uint CrcBits { get; set; } = 0;
    public byte[] NameAsBytes => Encoding.UTF8.GetBytes(Path.GetFileName(Name));
}
