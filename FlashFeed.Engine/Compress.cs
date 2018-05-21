using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FlashBot.Engine
{
    public class Compress
    {
        // https://www.dotnetperls.com/compress
        public static byte[] GZipCompress(string raw)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(raw);

            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                    CompressionMode.Compress, true))
                {
                    gzip.Write(bytes, 0, raw.Length);
                }

                return memory.ToArray();
            }
        }

        public static string GZipDecompress(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip),
                CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    byte[] byteReturn = memory.ToArray();

                    return Encoding.UTF8.GetString(byteReturn);
                }
            }
        }
    }
}
