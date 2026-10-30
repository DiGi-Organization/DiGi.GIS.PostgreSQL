using DiGi.Geometry.Planar.Classes;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Gets a comparison that orders building references along a Z-order (Morton) curve of their outlines, so that consecutive references lie close together (DiGi-Organization/DiGi.GIS.Analytical#4).
        /// <para>Each outline is placed in a square cell of <paramref name="cellSize"/> by the lower-left corner of its bounding box, and the cells are ordered by the interleaved bits of their column and row; a batch of consecutive references then covers a compact area rather than a strip across the county. References of equal cells are ordered by ordinal string comparison, and references with no outline come last, so the order is total and repeatable.</para>
        /// </summary>
        /// <param name="building2Ds_ByReference">The outlines, keyed by building reference. This value can be null.</param>
        /// <param name="cellSize">The side of a cell [m].</param>
        /// <returns>The comparison.</returns>
        public static Comparison<string> ZOrderComparison(IDictionary<string, GIS.Classes.Building2D>? building2Ds_ByReference, double cellSize = 100.0)
        {
            Dictionary<string, ulong> keys = [];

            if (building2Ds_ByReference is not null && building2Ds_ByReference.Count != 0 && cellSize > 0)
            {
                double x_Min = double.MaxValue;
                double y_Min = double.MaxValue;
                foreach (GIS.Classes.Building2D building2D in building2Ds_ByReference.Values)
                {
                    if (building2D?.PolygonalFace2D?.GetBoundingBox() is BoundingBox2D boundingBox2D)
                    {
                        x_Min = Math.Min(x_Min, boundingBox2D.Min.X);
                        y_Min = Math.Min(y_Min, boundingBox2D.Min.Y);
                    }
                }

                // The bits of the column and the row interleaved: 32 bits each, so a county of any size fits.
                static ulong Interleave(uint value)
                {
                    ulong result = value;
                    result = (result | (result << 16)) & 0x0000FFFF0000FFFFUL;
                    result = (result | (result << 8)) & 0x00FF00FF00FF00FFUL;
                    result = (result | (result << 4)) & 0x0F0F0F0F0F0F0F0FUL;
                    result = (result | (result << 2)) & 0x3333333333333333UL;
                    result = (result | (result << 1)) & 0x5555555555555555UL;
                    return result;
                }

                foreach (KeyValuePair<string, GIS.Classes.Building2D> keyValuePair in building2Ds_ByReference)
                {
                    if (keyValuePair.Value?.PolygonalFace2D?.GetBoundingBox() is not BoundingBox2D boundingBox2D)
                    {
                        continue;
                    }

                    uint column = (uint)Math.Max(0, Math.Floor((boundingBox2D.Min.X - x_Min) / cellSize));
                    uint row = (uint)Math.Max(0, Math.Floor((boundingBox2D.Min.Y - y_Min) / cellSize));
                    keys[keyValuePair.Key] = Interleave(column) | (Interleave(row) << 1);
                }
            }

            return (x, y) =>
            {
                bool hasKey_X = x is not null && keys.TryGetValue(x, out _);
                bool hasKey_Y = y is not null && keys.TryGetValue(y, out _);

                if (hasKey_X != hasKey_Y)
                {
                    return hasKey_X ? -1 : 1;
                }

                if (hasKey_X)
                {
                    int result = keys[x!].CompareTo(keys[y!]);
                    if (result != 0)
                    {
                        return result;
                    }
                }

                return string.CompareOrdinal(x, y);
            };
        }
    }
}
