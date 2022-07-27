static public class ByteHelpers
{
    static public FileStream WriteBytes(this FileStream stream, params byte[] bytes)
    {
        stream.Write(bytes);
        return stream;
    }

    static public FileStream Write_8(this FileStream stream, params byte[] bytes)
    {
        stream.Write(bytes);
        return stream;
    }

    static public FileStream Write16(this FileStream stream, params UInt16[] values)
    {
        foreach (var value in values)
        {
            stream.WriteByte((byte)(value >> 0));
            stream.WriteByte((byte)(value >> 8));
        }
        return stream;
    }

    static public FileStream Write32(this FileStream stream, params UInt32[] values)
    {
        foreach (var value in values)
        {
            stream.WriteByte((byte)(value >> 0));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }
        return stream;
    }


    static public FileStream Write64(this FileStream stream, params UInt64[] values)
    {
        foreach (var value in values)
        {
            stream.WriteByte((byte)(value >> 0));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 32));
            stream.WriteByte((byte)(value >> 40));
            stream.WriteByte((byte)(value >> 48));
            stream.WriteByte((byte)(value >> 56));
        }
        return stream;
    }
}