using System.Text;
using System.Security.Cryptography;


/// https://pkware.cachefly.net/webdocs/casestudies/APPNOTE.TXT

static public class Zip64
{
    static uint ComputeCrc(Stream stream)
    {
        uint crc = 0;
        byte[] buff = new byte[1024];
        int len = stream.Read(buff, 0, buff.Length);
        crc = Force.Crc32.Crc32Algorithm.Compute(buff, 0, len);
        while ((len = stream.Read(buff, 0, buff.Length)) > 0)
        {
            crc = Force.Crc32.Crc32Algorithm.Append(crc, buff, 0, len);
        }
        stream.Position = 0;
        return crc;
    }

    static public void ZipToFiles(string targetFile, string[] filesToZip)
    {
        var crc32 = new UncompressedZipWriter.Crc32();

        var zip = File.Create(targetFile);

        var timestamp = DateTime.Parse("2022-04-17") + TimeSpan.FromHours(18) + TimeSpan.FromMinutes(22) + TimeSpan.FromSeconds(30);

        var files = filesToZip.Select(f => new FileInZip(Path.GetFileName(f), File.OpenRead(f), new FileInfo(f).Length, timestamp)).ToArray();

        /// [local file header N]
        foreach (var file in files)
        {
            var lm = file.LastModified;

            file.Offset = (uint)zip.Position;
            file.TimeBits = (ushort)((lm.Second / 2) | lm.Minute << 5 | lm.Hour << 11);
            file.DateBits = (ushort)(lm.Day | lm.Month << 5 | (lm.Year - 1980) << 9);

            file.CrcBits = ComputeCrc(file.Stream);

            zip
                .WriteBytes(
                    0x50, 0x4b, 0x03, 0x04,                         // header
                    0x2D, 0x00,                                     // version
                    0x00, 0x00,                                     // general purpose bitflag
                    0x00, 0x00                                      // compression method (0 = store)
                )
                .WriteInt16AsBytes(file.TimeBits)
                .WriteInt16AsBytes(file.DateBits)
                .WriteInt32AsBytes(file.CrcBits)
                .WriteBytes(
                    0xFF, 0xFF, 0xFF, 0xFF,                         // compressed size: FFFFFFFF for ZIP64
                    0xFF, 0xFF, 0xFF, 0xFF                          // uncompressed size: FFFFFFFF for ZIP64
                )
                .WriteInt16AsBytes((ushort)file.NameAsBytes.Length) // filename length
                .WriteBytes(
                    0x14, 0x00                                      // extrafield length
                )
                .WriteBytes(file.NameAsBytes)
                .WriteBytes(
                    0x01, 0x00, 0x10, 0x00                          // extrafield header
                )
                .WriteInt64AsBytes((ulong)file.Size)                // compressed size: ZIP64 extra
                .WriteInt64AsBytes((ulong)file.Size)                // uncompressed size: ZIP64 extra
                .WriteStreamAsBytes(file.Stream)                    // write the actual data
            ;
        }

        var directoryOffset = zip.Position;

        /// [central directory header N]
        foreach (var file in files)
        {
            zip
                .WriteBytes(
                    0x50, 0x4b, 0x01, 0x02,                         // header
                    0x3F, 0x00,                                     // version
                    0x2D, 0x00,                                     // min version to extract
                    0x00, 0x00,                                     // general purpose bitflag
                    0x00, 0x00                                      // compression method (0 = store)
                )
                .WriteInt16AsBytes(file.TimeBits)
                .WriteInt16AsBytes(file.DateBits)
                .WriteInt32AsBytes(file.CrcBits)
                .WriteBytes(
                    0xFF, 0xFF, 0xFF, 0xFF,                         // compressed size: FFFFFFFF for ZIP64
                    0xFF, 0xFF, 0xFF, 0xFF                          // uncompressed size: FFFFFFFF for ZIP64
                )
                .WriteInt16AsBytes((ushort)file.NameAsBytes.Length) // filename length
                .WriteBytes(
                    0x14, 0x00,                                     // extrafield length
                    0x00, 0x00,                                     // file comment length
                    0x00, 0x00,                                     // disk number
                    0x00, 0x00,                                     // internal file attributes
                    0x00, 0x00, 0x00, 0x00                          // external file attributes
                )
                .WriteInt32AsBytes(file.Offset)                     // offset of file
                .WriteBytes(file.NameAsBytes)
                .WriteBytes(
                    0x01, 0x00, 0x10, 0x00                          // extrafield header
                )
                .WriteInt64AsBytes((ulong)file.Size)                // compressed size: ZIP64 extra
                .WriteInt64AsBytes((ulong)file.Size)                // uncompressed size: ZIP64 extra
            ;
        }

        var endOffset = zip.Position;

        var zip64endofcentraldirectoryrecordOffset = zip.Position;
        /// [zip64 end of central directory record]
        zip
            .WriteBytes(
                0x50, 0x4b, 0x06, 0x06                              // header
            )
            // TODO
        ;

        var zip64endofcentraldirectorylocatorOffset = zip.Position;
        /// [zip64 end of central directory locator]
        zip
            .WriteBytes(
                0x50, 0x4b, 0x06, 0x07,                             // header
                0x00, 0x00                                          // current disk number
            )
            .WriteInt64AsBytes(0
                /* relative offset to zip64endofcentraldirectoryrecordOffset */
            )
            .WriteBytes(
                0x00, 0x00, 0x00, 0x00                              // number of disks
            )
            // TODO
              ///   4.3.14  Zip64 end of central directory record

              //  zip64 end of central dir
              //  signature                       4 bytes(0x06064b50)
              //  size of zip64 end of central
              //  directory record                8 bytes
              //  version made by                 2 bytes
              //  version needed to extract       2 bytes
              //  number of this disk             4 bytes
              //  number of the disk with the
              //  start of the central directory  4 bytes
              //  total number of entries in the
              //  central directory on this disk  8 bytes
              //  total number of entries in the
              //  central directory               8 bytes
              //  size of the central directory   8 bytes
              //  offset of start of central
              //  directory with respect to
              //  the starting disk number        8 bytes
              //  zip64 extensible data sector(variable size)

              //4.3.14.1 The value stored into the "size of zip64 end of central
              //directory record" SHOULD be the size of the remaining
              //record and SHOULD NOT include the leading 12 bytes.

              //Size = SizeOfFixedFields + SizeOfVariableData - 12.
        ;

        var endofcentraldirectoryrecordOffset = zip.Position;
        /// [end of central directory record]
        zip
            .WriteBytes(
                0x50, 0x4b, 0x05, 0x06,                             // header
                0x00, 0x00,                                         // disk number: FFFF for ZIP64
                0x00, 0x00                                          // starting disk: FFFF for ZIP64
            )
            .WriteInt16AsBytes((ushort)files.Length)                // central directory number: FFFF for ZIP64
            .WriteInt16AsBytes((ushort)files.Length)                // central directory amount: FFFF for ZIP64
            .WriteInt32AsBytes((uint)(endOffset - directoryOffset)) // central directory size: FFFFFFFF for ZIP64
            .WriteInt32AsBytes((uint)directoryOffset)               // central directory offset: FFFFFFFF for ZIP64
            .WriteBytes(0x00, 0x00)                                 // comment length
        ;

        zip.Close();
    }
}
