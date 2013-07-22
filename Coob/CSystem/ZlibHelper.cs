using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Coob
{
    class ZlibHelper
    {
        public static byte[] UncompressBuffer(byte[] buffer, out ushort key, out uint keyend)
        {
            byte[] toDecompress = new byte[buffer.Length - 2];
            Array.Copy(buffer, 2, toDecompress, 0, toDecompress.Length);

            key = BitConverter.ToUInt16(buffer, 0);
            keyend = BitConverter.ToUInt32(buffer, buffer.Length - 4);
            return DeflateStream.UncompressBuffer(toDecompress);
        }

        public static byte[] CompressBuffer(byte[] buffer, ushort key, uint keyend)
        {
            byte[] compressed;
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
            {
                compressor.Write(buffer, 0, buffer.Length);
                compressor.Close();
                compressed = compressStream.ToArray();
            }
            byte[] returnbytes = new byte[compressed.Length + 6];
            byte[] kbyte = BitConverter.GetBytes(key);
            byte[] kebyte = BitConverter.GetBytes(keyend);
            Array.Copy(kbyte, 0, returnbytes, 0, kbyte.Length);
            Array.Copy(compressed, 0, returnbytes, 2, compressed.Length);
            Array.Copy(kebyte, 0, returnbytes, returnbytes.Length - 4, kebyte.Length);
            return returnbytes;
        }
    }
}
