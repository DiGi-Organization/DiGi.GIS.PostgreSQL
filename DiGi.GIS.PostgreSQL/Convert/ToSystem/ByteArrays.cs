using System.Buffers.Binary;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Convert
    {
        /// <summary>
        /// Unpacks the <c>shading_factors</c> value of the <see cref="Constants.TableName.BuildingSolarRadiation"/> table into the shading factors of each surface, the inverse of <see cref="ToSystem_Bytes(IEnumerable{byte[]})"/>.
        /// </summary>
        /// <param name="bytes">The packed value: a little-endian <c>uint32</c> hour count <c>n</c>, then <c>n</c> bytes per surface.</param>
        /// <returns>One array of <c>n</c> bytes per surface, in stored order, or null when <paramref name="bytes"/> is null, shorter than its header, has a zero hour count, or its length is not a whole number of surfaces.</returns>
        public static List<byte[]>? ToSystem_ByteArrays(this byte[]? bytes)
        {
            if (bytes is null || bytes.Length < sizeof(uint))
            {
                return null;
            }

            uint count = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
            long length = bytes.Length - sizeof(uint);
            if (count == 0 || length % count != 0)
            {
                return null;
            }

            List<byte[]> result = [];
            for (long offset = sizeof(uint); offset < bytes.Length; offset += count)
            {
                byte[] shadingFactors = new byte[count];
                System.Buffer.BlockCopy(bytes, (int)offset, shadingFactors, 0, (int)count);
                result.Add(shadingFactors);
            }

            return result;
        }
    }
}
