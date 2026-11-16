using DiGi.EPW.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.Solar.Classes
{
    /// <summary>
    /// Holds one building and radius of a batch solar radiation solve: the receiving building, its receiver normals, the neighbour radius and the weather it is solved with (DiGi.GIS.PostgreSQL#139).
    /// <para>The building model is shared by the jobs of its other radii and may be read by several solves at once; a solve only reads it.</para>
    /// </summary>
    public class BuildingSolarRadiationSolveJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingSolarRadiationSolveJob"/> class.
        /// </summary>
        /// <param name="buildingModel">The receiving building model, stamped with its building information.</param>
        /// <param name="reference">The reference of the building.</param>
        /// <param name="normals">The outward normal of each receiving surface, by the surface's reference.</param>
        /// <param name="radius">The neighbour radius, in m, exactly as it is stored; 0 is the building alone.</param>
        /// <param name="countyId">The county part the result is filed under.</param>
        /// <param name="ePWFileId">The id of the weather file.</param>
        /// <param name="ePWFile">The weather file.</param>
        public BuildingSolarRadiationSolveJob(DiGi.Analytical.Building.Classes.BuildingModel buildingModel, string reference, Dictionary<string, Vector3D> normals, double radius, int countyId, int ePWFileId, EPWFile ePWFile)
        {
            BuildingModel = buildingModel;
            Reference = reference;
            Normals = normals;
            Radius = radius;
            CountyId = countyId;
            EPWFileId = ePWFileId;
            EPWFile = ePWFile;
        }

        /// <summary>
        /// Gets the receiving building model.
        /// </summary>
        public DiGi.Analytical.Building.Classes.BuildingModel BuildingModel { get; }

        /// <summary>
        /// Gets the county part the result is filed under.
        /// </summary>
        public int CountyId { get; }

        /// <summary>
        /// Gets the weather file.
        /// </summary>
        public EPWFile EPWFile { get; }

        /// <summary>
        /// Gets the id of the weather file.
        /// </summary>
        public int EPWFileId { get; }

        /// <summary>
        /// Gets the outward normal of each receiving surface, by the surface's reference.
        /// </summary>
        public Dictionary<string, Vector3D> Normals { get; }

        /// <summary>
        /// Gets the neighbour radius, in m, exactly as it is stored.
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// Gets the reference of the building.
        /// </summary>
        public string Reference { get; }
    }
}
