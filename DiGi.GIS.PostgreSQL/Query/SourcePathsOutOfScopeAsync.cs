using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.IO.Classes;
using DiGi.GIS.IO.Enums;
using GeoAPI.CoordinateSystems.Transformations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Selects the source files of an open building release that cannot reach an import's scope, so the import can leave them unread (DiGi.GIS.PostgreSQL#170): a nationwide release read for a few counties otherwise spends most of its time on files that keep nothing.
        /// <para>Each file's own extent comes from <see cref="DiGi.GIS.IO.Query.SourceExtentAsync(string?, BuildingDataset, CancellationToken)"/> in the file's reference system. It is carried into EPSG:2180 through the same transformations the import uses - sampled along its four edges, because a box does not stay a box under a change of projection - and the envelope of the samples is compared with every scope box grown by <paramref name="margin"/>. A file is selected only when that envelope reaches none of them.</para>
        /// <para>Every doubt reads the file: no extent (CSV, PBF, a GeoPackage without a registered R-tree, a GeoParquet without a declared bbox), an unsupported reference system, a transformation that cannot be built or a sample that does not land on a finite point. Leaving a file out is therefore only ever a saving - the rows a run stores are the same either way - while the footprints of the files left out are counted nowhere, neither as out of scope nor as rejected.</para>
        /// </summary>
        /// <param name="paths">The source files of the release.</param>
        /// <param name="buildingDataset">The dataset the files belong to; it selects the format.</param>
        /// <param name="boundingBox2Ds_Scope">The scope in EPSG:2180: the envelope of every county part in scope, or the scope bounding box. A null envelope among them, a null or an empty scope selects nothing.</param>
        /// <param name="margin">The distance in metres a footprint filed under a file's extent may reach beyond it, plus the import's filing tolerance; a file whose extent comes this close to the scope is read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result is the files that cannot reach the scope, in the order given; empty when every file must be read.</returns>
        public static async Task<List<string>> SourcePathsOutOfScopeAsync(IEnumerable<string>? paths, BuildingDataset buildingDataset, IEnumerable<BoundingBox2D?>? boundingBox2Ds_Scope, double margin, CancellationToken cancellationToken = default)
        {
            List<string> result = [];

            if (paths is null || boundingBox2Ds_Scope is null || double.IsNaN(margin) || margin < 0)
            {
                return result;
            }

            List<BoundingBox2D> boundingBox2Ds = [];
            foreach (BoundingBox2D? boundingBox2D in boundingBox2Ds_Scope)
            {
                if (boundingBox2D is null)
                {
                    // A scope part without an envelope could be anywhere: nothing can be ruled out.
                    return result;
                }

                boundingBox2Ds.Add(boundingBox2D);
            }

            if (boundingBox2Ds.Count == 0)
            {
                return result;
            }

            // The samples per box edge. The tiles and regions of the releases span under a degree, where the
            // curvature between two samples is far below any margin worth giving.
            const int count_Sample = 16;

            ICoordinateTransformation? transformation_4326 = null;
            ICoordinateTransformation? transformation_3035 = null;

            foreach (string path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                SourceExtent? sourceExtent = await DiGi.GIS.IO.Query.SourceExtentAsync(path, buildingDataset, cancellationToken);
                if (sourceExtent?.BoundingBox2D is not BoundingBox2D boundingBox2D_Source)
                {
                    continue;
                }

                ICoordinateTransformation? coordinateTransformation;
                switch (sourceExtent.Srid)
                {
                    case 4326:
                        transformation_4326 ??= GIS.Create.EPSG2180TransformationFromEPSG4326();
                        coordinateTransformation = transformation_4326;
                        break;

                    case 3035:
                        transformation_3035 ??= GIS.Create.EPSG2180TransformationFromEPSG3035();
                        coordinateTransformation = transformation_3035;
                        break;

                    default:
                        coordinateTransformation = null;
                        break;
                }

                if (coordinateTransformation?.MathTransform is not IMathTransform mathTransform)
                {
                    continue;
                }

                double minX = double.MaxValue;
                double minY = double.MaxValue;
                double maxX = double.MinValue;
                double maxY = double.MinValue;
                bool isFinite = true;

                double width = boundingBox2D_Source.Max.X - boundingBox2D_Source.Min.X;
                double height = boundingBox2D_Source.Max.Y - boundingBox2D_Source.Min.Y;

                for (int i = 0; i <= count_Sample && isFinite; i++)
                {
                    double x = boundingBox2D_Source.Min.X + (width * i / count_Sample);
                    double y = boundingBox2D_Source.Min.Y + (height * i / count_Sample);

                    // The four edges: bottom and top at x, left and right at y.
                    double[][] samples =
                    [
                        [x, boundingBox2D_Source.Min.Y],
                        [x, boundingBox2D_Source.Max.Y],
                        [boundingBox2D_Source.Min.X, y],
                        [boundingBox2D_Source.Max.X, y],
                    ];

                    foreach (double[] sample in samples)
                    {
                        double[]? values;
                        try
                        {
                            values = mathTransform.Transform(sample);
                        }
                        catch (ArgumentException)
                        {
                            values = null;
                        }

                        if (values is null || values.Length < 2 || double.IsNaN(values[0]) || double.IsNaN(values[1]) || double.IsInfinity(values[0]) || double.IsInfinity(values[1]))
                        {
                            isFinite = false;
                            break;
                        }

                        minX = Math.Min(minX, values[0]);
                        minY = Math.Min(minY, values[1]);
                        maxX = Math.Max(maxX, values[0]);
                        maxY = Math.Max(maxY, values[1]);
                    }
                }

                if (!isFinite)
                {
                    continue;
                }

                BoundingBox2D boundingBox2D_2180 = new(new Point2D(minX, minY), new Point2D(maxX, maxY));

                bool isInScope = false;
                foreach (BoundingBox2D boundingBox2D in boundingBox2Ds)
                {
                    if (boundingBox2D.InRange(boundingBox2D_2180, margin))
                    {
                        isInScope = true;
                        break;
                    }
                }

                if (!isInScope)
                {
                    result.Add(path);
                }
            }

            return result;
        }
    }
}
