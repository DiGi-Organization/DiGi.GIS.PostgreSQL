using DiGi.Analytical.Building.Interfaces;
using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.GIS.Classes;
using System.Collections.Generic;
using System.Globalization;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Creates a reference chain for the specified building model, optionally anchored to a county administrative division and a specific building element.
        /// <para>The reference chain is ordered from the root of the containment hierarchy inwards: <see cref="AdministrativeDivision"/> (if a county identifier is provided), <see cref="DiGi.Analytical.Building.Classes.BuildingModel"/> (by its <see cref="Analytical.Enums.BuildingModelParameter.Reference"/> parameter value or a <see cref="GuidReference"/> fallback), and the optional <see cref="IBuildingGuidObject"/> element. If the chain contains only a single entry, that entry is returned directly instead of wrapped in a <see cref="ComplexReference"/>.</para>
        /// </summary>
        /// <param name="buildingModel">The analytical building model to create the reference for.</param>
        /// <param name="buildingGuidObject">An optional specific building element (e.g. component, space) to include as the innermost reference in the chain.</param>
        /// <param name="countyId">An optional county identifier that anchors the reference to an <see cref="AdministrativeDivision"/> at the outermost level of the chain.</param>
        /// <returns>An <see cref="IReference"/> representing the containment chain, or <see langword="null"/> if <paramref name="buildingModel"/> is <see langword="null"/>.</returns>
        public static IReference? Reference(DiGi.Analytical.Building.Classes.BuildingModel buildingModel, IBuildingGuidObject? buildingGuidObject = null, int? countyId = null)
        {
            if (buildingModel is null)
            {
                return null;
            }

            List<ISerializableReference> serializableRefrences = [];

            // Ordered from the root of the containment chain inwards - administrative area, then building, then
            // building element - so a ComplexReference reads AdministrativeArea -> Building -> element.
            if (countyId is not null)
            {
                serializableRefrences.Add(new UniqueIdReference(new TypeReference(typeof(AdministrativeDivision)), countyId.Value.ToString(CultureInfo.CurrentCulture)));
            }

            string? reference = buildingModel.GetValue<string>(Analytical.Enums.BuildingModelParameter.Reference);
            if (!string.IsNullOrWhiteSpace(reference))
            {
                serializableRefrences.Add(new UniqueIdReference(new TypeReference(typeof(DiGi.Analytical.Building.Classes.BuildingModel)), reference));
            }
            else
            {
                serializableRefrences.Add(new GuidReference(buildingModel));
            }

            if (buildingGuidObject is not null)
            {
                serializableRefrences.Add(new GuidReference(buildingGuidObject));
            }

            if (serializableRefrences.Count == 1)
            {
                return serializableRefrences[0];
            }

            return new ComplexReference(serializableRefrences);
        }
    }
}