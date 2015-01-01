using System;
using System.Collections.Generic;
#if !NET20
using System.Linq;
#endif
using System.Text;
using System.IO;


namespace LittleUmph
{
    /// <summary>
    /// Stream assist class.
    /// </summary>
    public class Strm
    {
        #region [ Read Bytes ]
        /// <summary>
        /// Reads the bytes (meant to read small number of byte count <1KB).
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="byteCount">The byte count.</param>
        /// <returns></returns>
        public static byte[] ReadBytes(Stream stream, int byteCount)
        {
            List<byte> data = new List<byte>();

            for (int i = 0; i < byteCount; i++)
            {
                var b = stream.ReadByte();
                if (b > -1)
                {
                    data.Add((byte)b);
                }
            }

            return data.ToArray();
        }
        #endregion

        #region [ Read Integer ]
        /// <summary>
        /// Reads two bytes and convert it to uint16.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static UInt16 ReadUInt16(Stream stream, long offset = -1)
        {
            if (offset > -1)
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }
            byte[] bytes = ReadBytes(stream, 2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Reads two bytes and convert it to int16.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static Int16 ReadInt16(Stream stream)
        {
            byte[] bytes = ReadBytes(stream, 2);
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Reads the int16.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static Int16 ReadInt16(Stream stream, long offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            return ReadInt16(stream);
        }

        /// <summary>
        /// Reads four bytes and convert it to uint32.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static UInt32 ReadUInt32(Stream stream, long offset = -1)
        {
            if (offset > -1)
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }
            byte[] bytes = ReadBytes(stream, 2);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Reads four bytes and convert it to int32.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static Int32 ReadInt32(Stream stream)
        {
            byte[] bytes = ReadBytes(stream, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Reads the int32.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static Int32 ReadInt32(Stream stream, long offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            return ReadInt32(stream);
        }

        /// <summary>
        /// Reads eight bytes and convert it to uint64.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static UInt64 ReadUInt64(Stream stream, long offset = -1)
        {
            if (offset > -1)
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }
            byte[] bytes = ReadBytes(stream, 2);
            return BitConverter.ToUInt64(bytes, 0);
        }

        /// <summary>
        /// Reads eight bytes and convert it to int64.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static Int64 ReadInt64(Stream stream)
        {
            byte[] bytes = ReadBytes(stream, 8);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// Reads the int64.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static Int64 ReadInt64(Stream stream, long offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            return ReadInt64(stream);
        }
        #endregion

        #region [ Stream Conversion ]
        /// <summary>
        /// Take a string and put it in a memory stream.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static MemoryStream FromString(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            MemoryStream ms = new MemoryStream(bytes);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Read the stream and convert it to string (remember to set position to zero).
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static string ToString(Stream stream)
        {
            byte[] buffer = new byte[4096];
            StringBuilder sb = new StringBuilder();
            while (stream.Read(buffer, 0, buffer.Length) > 0)
            {
                sb.Append(Encoding.UTF8.GetString(buffer));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Streams to bytes (remember to set position to zero).
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static byte[] ToBytes(Stream stream)
        {
            List<byte> result = new List<byte>();
            byte[] buffer = new byte[4096];
            int len = 0;
            while ((len = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (len == buffer.Length)
                {
                    result.AddRange(buffer);
                }
                else
                {
                    byte[] fit = new byte[len];
                    Array.Copy(buffer, fit, len);
                    result.AddRange(fit);
                    fit = null;
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Save streams to a file (remember to set position to zero).
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="targetPath">The target path.</param>
        public static void ToFile(Stream stream, string targetPath)
        {
            using (var fs = File.OpenWrite(targetPath))
            {
                byte[] buffer = new byte[4096];
                int len = 0;
                while ((len = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, len);
                }

                fs.Flush();
            }
        }

        /// <summary>
        /// Convert a stream into a memory stream (remember to set position to zero).
        /// </summary>
        /// <param name="stream">The stream.</param>
        public static MemoryStream ToMemoryStream(Stream stream)
        {
            var bytes = ToBytes(stream);
            return new MemoryStream(bytes);
        }
        #endregion
    }
}
