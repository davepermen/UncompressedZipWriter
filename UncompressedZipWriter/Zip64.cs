/// https://pkware.cachefly.net/webdocs/casestudies/APPNOTE.TXT
/// https://rzymek.github.io/post/excel-zip64/

static public class Zip64
{
    static FileStream WriteStreamAndComputeCrc(this FileStream output, Stream input, Action<uint> calculatedCrc)
    {
        byte[] buff = new byte[1024];

        int len = input.Read(buff, 0, buff.Length);
        uint crc = Force.Crc32.Crc32Algorithm.Compute(buff, 0, len);
        output.Write(buff, 0, len);

        while ((len = input.Read(buff, 0, buff.Length)) > 0)
        {
            crc = Force.Crc32.Crc32Algorithm.Append(crc, buff, 0, len);
            output.Write(buff, 0, len);
        }
        calculatedCrc(crc);
        return output;
    }

    static FileStream WriteFileEntry(this FileStream zip, FileInZip file)
    {
        file.Offset = (uint)zip.Position;

        return zip
            .Write_8(0x50, 0x4b, 0x03, 0x04)                                  /// header [local file header]
            .Write16(45, 0b00001000, 0, file.TimeBits, file.DateBits)         // version (45 = ZIP64) | general purpose bitflag (bit 3 for Data Descriptor at End) | compression method (0 = store) | time | date
            .Write32(0, 0, 0)                                                 // CRC bits | compressed size | uncompressed size => 0 each for data descriptor
            .Write16((ushort)file.NameAsBytes.Length, 0)                      // filename length | extrafield size
            .Write_8(file.NameAsBytes)                                        // filename

            .WriteStreamAndComputeCrc(file.Stream, crc => file.CrcBits = crc) /// write the actual data and calculate crc

            .Write_8(0x50, 0x4b, 0x07, 0x08)                                  /// header [data descriptor]
            .Write32(file.CrcBits)                                            // CRC bits
            .Write64((ulong)file.Size, (ulong)file.Size)                      // compressed size: ZIP64 extra | uncompressed size: ZIP64 extra
        ;
    }

    static FileStream WriteCentralDirectoryEntry(this FileStream zip, FileInZip file)
    {
        return zip
            .Write_8(0x50, 0x4b, 0x01, 0x02)                              /// header [central directory header]
            .Write16(45, 45, 0b00001000, 0, file.TimeBits, file.DateBits) // version (ZIP64) | min version to extract (ZIP64) | general purpose bitflag (bit 3 for Data Descriptor at End) | compression method (0 = store) | time | date
            .Write32(file.CrcBits, 0xFFFFFFFF, 0xFFFFFFFF)                // CRC bits | compressed size | uncompressed size => FFFFFFFF for ZIP64
            .Write16((ushort)file.NameAsBytes.Length)                     // filename length
            .Write16(20, 0, 0, 0)                                         // extrafield length | file comment length | disk number | internal file attributes
            .Write32(0, file.Offset)                                      // external file attributes, offset of file
            .Write_8(file.NameAsBytes)                                    // filename

            .Write_8(0x01, 0x00)                                          /// extrafield header
            .Write16(16)                                                  // size of extrafield (below)
            .Write64((ulong)file.Size, (ulong)file.Size)                  // compressed size: ZIP64 extra | uncompressed size: ZIP64 extra
        ;
    }

    static FileStream WriteEndOfCentralDirectory(this FileStream zip, ulong count, ulong offset, ulong length)
    {
        return zip
            .Write_8(0x50, 0x4b, 0x06, 0x06)       /// header [zip64 end of central directory record]
            .Write64(44)                           // size of remaining record is 56 bytes
            .Write16(45, 45)                       // version (ZIP64) | min version to extract (ZIP64)
            .Write32(0, 0)                         // number of this disk | number of the disk with the start of the central directory
            .Write64(count, count, length, offset) // total number of entries in the central directory on this disk | total number of entries in the central directory | size of central directory | offset of start of central directory with respect to the starting disk number

            .Write_8(0x50, 0x4b, 0x06, 0x07)       /// header [zip64 end of central directory locator]
            .Write32(0)                            // number of the disk with the start of the zip64 end of central directory
            .Write64(offset)                       // relative offset of the zip64 end of central directory record
            .Write32(1)                            // total number of disks

            .Write_8(0x50, 0x4b, 0x05, 0x06)       /// header [end of central directory record]
            .Write16(0, 0, 0xFFFF, 0xFFFF)         // disk number | starting disk | central directory number | central directory amount
            .Write32(0xFFFFFFFF, 0xFFFFFFFF)       // central directory size | central directory offset
            .Write16(0)
        ;
    }

    static public void ZipToFiles(string targetFile, string[] filesToZip)
    {
        var zip = File.Create(targetFile);

        var files = filesToZip.Select(f => new FileInZip(Path.GetFileName(f), File.OpenRead(f), new FileInfo(f).Length, new FileInfo(f).LastWriteTime)).ToArray();

        foreach (var file in files)
        {
            zip.WriteFileEntry(file);
        }

        var centralDirectoryStart = (ulong)zip.Position;

        /// [central directory header N]
        foreach (var file in files)
        {
            zip.WriteCentralDirectoryEntry(file);
        }

        var centralDirectoryEnd = (ulong)zip.Position;

        var centralDirectorySize = centralDirectoryEnd - centralDirectoryStart;
        var fileCount = (ulong)files.Length;

        zip.WriteEndOfCentralDirectory(fileCount, centralDirectoryStart, centralDirectorySize);

        zip.Close();
    }
}
