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
        public static byte[] UncompressBuffer(byte[] buffer)
        {
            byte[] toDecompress = new byte[buffer.Length - 2];
            Array.Copy(buffer, 2, toDecompress, 0, toDecompress.Length);

            return DeflateStream.UncompressBuffer(toDecompress);
        }

        public static byte[] CompressBuffer(byte[] buffer)
        {
            byte[] compressed;
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
            {
                compressor.Write(buffer, 0, buffer.Length);
                compressor.Close();
                compressed = compressStream.ToArray();
            }
            byte[] returnbytes = new byte[compressed.Length + 2];
            Array.Copy(compressed, 0, returnbytes, 2, compressed.Length);
            return returnbytes;
        }
    }
}
