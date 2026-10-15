using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Convert
    {
        /// <summary>
        /// Packs the shading factors of every surface of a stored solar radiation result into the single <c>shading_factors</c> value of the <see cref="Constants.TableName.BuildingSolarRadiation"/> table.
        /// <para>The layout is a little-endian <c>uint32</c> hour count <c>n</c>, then <c>n</c> bytes per surface in the order given (the order of the surfaces in the stored object). Keeping the count in the value makes it self-describing, so a later time step still unpacks with <see cref="ToSystem_ByteArrays(byte[])"/>.</para>
        /// </summary>
        /// <param name="shadingFactors">The quantized shading factors of each surface, in surface order.</param>
        /// <returns>The packed value, or null when <paramref name="shadingFactors"/> is null or empty, an element is null or empty, or the elements differ in length.</returns>
        public static byte[]? ToSystem_Bytes(this IEnumerable<byte[]?>? shadingFactors)
        {
            if (shadingFactors is null)
            {
                return null;
            }

            List<byte[]> shadingFactors_List = [];
            foreach (byte[]? shadingFactors_Surface in shadingFactors)
            {
                if (shadingFactors_Surface is null || shadingFactors_Surface.Length == 0 || (shadingFactors_List.Count != 0 && shadingFactors_Surface.Length != shadingFactors_List[0].Length))
                {
                    return null;
                }

                shadingFactors_List.Add(shadingFactors_Surface);
            }

            if (shadingFactors_List.Count == 0)
            {
                return null;
            }

            int count = shadingFactors_List[0].Length;

            byte[] result = new byte[sizeof(uint) + (count * shadingFactors_List.Count)];
            BinaryPrimitives.WriteUInt32LittleEndian(result, (uint)count);
            for (int i = 0; i < shadingFactors_List.Count; i++)
            {
                System.Buffer.BlockCopy(shadingFactors_List[i], 0, result, sizeof(uint) + (i * count), count);
            }

            return result;
        }

        /// <summary>
        /// Converts the stored form of a <c>byte[]</c> member back to bytes.
        /// <para>The DiGi serializer writes a <c>byte[]</c> as a JSON array of numbers (<c>Core.Create.JsonNode</c> treats it as an enumerable), so that is the shape a projection such as <c>v-&gt;'Bytes'</c> reads out of a stored object. A base64 string - the form System.Text.Json gives a <c>byte[]</c> - is accepted as well, so a row written that way decodes rather than fails. Any other shape answers null.</para>
        /// </summary>
        /// <param name="jsonNode">The <see cref="JsonNode"/> holding the member. This value can be null.</param>
        /// <returns>The bytes, or null when <paramref name="jsonNode"/> is null, an element is not a byte, or the node is neither an array nor a base64 string.</returns>
        public static byte[]? ToSystem_Bytes(this JsonNode? jsonNode)
        {
            if (jsonNode is JsonArray jsonArray)
            {
                byte[] result = new byte[jsonArray.Count];
                for (int i = 0; i < result.Length; i++)
                {
                    if (jsonArray[i] is not JsonValue jsonValue || !jsonValue.TryGetValue(out byte @byte))
                    {
                        return null;
                    }

                    result[i] = @byte;
                }

                return result;
            }

            if (jsonNode is JsonValue jsonValue_String && jsonValue_String.TryGetValue(out string? @string) && @string is not null)
            {
                try
                {
                    return System.Convert.FromBase64String(@string);
                }
                catch (System.FormatException)
                {
                    return null;
                }
            }

            return null;
        }
    }
}
