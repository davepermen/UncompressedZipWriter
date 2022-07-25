static public class ByteHelpers
{
    static public FileStream WriteBytes(this FileStream stream, params byte[] bytes)
    {
        stream.Write(bytes);
        return stream;
    }
    static public FileStream WriteInt16AsBytes(this FileStream stream, UInt16 value)
    {
        stream.WriteByte((byte)(value >> 0));
        stream.WriteByte((byte)(value >> 8));
        return stream;
    }
    static public FileStream WriteInt32AsBytes(this FileStream stream, UInt32 value)
    {
        stream.WriteByte((byte)(value >> 0));
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 24));
        return stream;
    }
    static public FileStream WriteInt64AsBytes(this FileStream stream, UInt64 value)
    {
        stream.WriteByte((byte)(value >> 0));
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 24));
        stream.WriteByte((byte)(value >> 32));
        stream.WriteByte((byte)(value >> 40));
        stream.WriteByte((byte)(value >> 48));
        stream.WriteByte((byte)(value >> 56));
        return stream;
    }
    static public FileStream WriteStreamAsBytes(this FileStream stream, Stream other)
    {
        other.CopyTo(stream);
        return stream;
    }
}