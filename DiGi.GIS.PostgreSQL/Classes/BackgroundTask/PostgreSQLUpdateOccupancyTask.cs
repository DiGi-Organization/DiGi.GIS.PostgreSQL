using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task responsible for updating occupancy data within a PostgreSQL GIS database.
    /// <para>This class leverages the <see cref="GISPostgreSQLConverterManager"/> to execute the update process based on the provided <see cref="PostgreSQLUpdateOccupancyOptions"/>.</para>
    /// <para><b>Administrative side.</b> Every subdivision keeps its own stored figure - <see langword="null"/> when the source carries none, never a zero standing in for it. The levels above are sums: a municipality over its subdivisions, a county over its municipalities, and so on. Where the subdivision layer nests - a city, its districts and their neighbourhoods are all subdivisions of the one municipality - only the <b>top-level</b> subdivisions are summed (<see cref="Query.ContainerIds(System.Collections.Generic.IReadOnlyDictionary{int, Geometry.Planar.Classes.PolygonalFace2D}, double)"/>), so a city counts once rather than once per level; summing every row wrote Warsaw's municipality as 4.6 million against a city of 1 622 594 (<see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/77">DiGi.GIS.PostgreSQL#77</see>).</para>
    /// <para><b>Building side.</b> Each building is attributed to exactly one subdivision - the one its <c>subdivision_id</c> names, which is the smallest subdivision containing it - and takes its share from the figure of the <b>smallest figured subdivision containing it</b> (<see cref="Query.OccupancySubdivisionId(int, System.Collections.Generic.IReadOnlyDictionary{int, System.Collections.Generic.List{int}}, System.Collections.Generic.IReadOnlyDictionary{int, uint?})"/>): its own when the row carries one, otherwise the nearest container's. A figured subdivision's figure is spread by floor area over <b>every</b> building inside it - its own and its descendants' alike - and each building keeps the share of the smallest figured subdivision it sits in; so a district's figure gives a district density to the buildings outside its figured neighbourhoods, rather than the whole district figure landing on the few buildings filed directly under it (Białołęka's 123 668 over 76 buildings) and rather than nothing at all for a neighbourhood without a figure (Jelonki's 2 237 buildings). Figures are read off the <c>administrative_areal_2d</c> rows, never off the occupancy table this task writes. A subdivision with no figure anywhere up its chain writes nothing for its buildings and is counted in <see cref="MissingOccupancySubdivisionCount"/>; one that borrowed a container's figure is counted in <see cref="BorrowedOccupancySubdivisionCount"/>. An explicit zero is a figure and is distributed as one. The building side can be limited to county polygon parts with <see cref="PostgreSQLUpdateOccupancyOptions.CountyIds"/>.</para>
    /// </summary>
    public class PostgreSQLUpdateOccupancyTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// Gets the GIS PostgreSQL converter manager used to refresh the data.
        /// </summary>
        private readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        /// <summary>
        /// Gets or sets the options used to configure the PostgreSQL occupancy update process.
        /// </summary>
        public PostgreSQLUpdateOccupancyOptions PostgreSQLUpdateOccupancyOptions { get; set; } = new PostgreSQLUpdateOccupancyOptions();

        /// <summary>
        /// Gets the number of subdivisions that held buildings but carried no occupancy figure, and had no container carrying one either, during the last run - so their buildings were left unwritten.
        /// <para>Not a failure of the run but a gap in the source: the figure is absent on the subdivision row and on every subdivision around it. The same subdivisions come up again next time until the source is completed. Logged one warning each, naming the subdivision and its building count.</para>
        /// </summary>
        public long MissingOccupancySubdivisionCount { get; private set; }

        /// <summary>
        /// Gets the number of subdivisions that held buildings but carried no occupancy figure of their own during the last run, so their buildings took the share of the smallest figured subdivision containing them.
        /// <para>Logged one line each, naming the subdivision, the one whose figure applied and the building count.</para>
        /// </summary>
        public long BorrowedOccupancySubdivisionCount { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLUpdateOccupancyTask"/> class.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The <see cref="GISPostgreSQLConverterManager"/> used to refresh the occupancy data.</param>
        public PostgreSQLUpdateOccupancyTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager;
        }

        /// <summary>
        /// Executes the background task to update occupancy data for administrative areal units and buildings.
        /// </summary>
        /// <param name="progress">A progress reporter for reporting the number of processed items.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true if the update was successful; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            if (gISPostgreSQLConverterManager is null)
            {
                return false;
            }

            PostgreSQLUpdateOccupancyOptions ??= new PostgreSQLUpdateOccupancyOptions();

            MissingOccupancySubdivisionCount = 0;
            BorrowedOccupancySubdivisionCount = 0;

            bool includeBuilding2Ds = PostgreSQLUpdateOccupancyOptions.IncludeBuilding2Ds;
            bool includeAdministrativeAreal2Ds = PostgreSQLUpdateOccupancyOptions.IncludeAdministrativeAreal2Ds;
            bool clear = PostgreSQLUpdateOccupancyOptions.Clear;
            HashSet<int>? countyIds_Scope = PostgreSQLUpdateOccupancyOptions.CountyIds;

            // Bulk reads/writes over hundreds of thousands of records exceed the 30s default; allow up to 10 minutes per statement.
            const int commandTimeout = 600;

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            if (administrativeAreal2DPostgreSQLConverter is null)
            {
                return false;
            }

            AdministrativeAreal2DOccupancyDataPostgreSQLConverter? administrativeAreal2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DOccupancyDataPostgreSQLConverter>();
            if (administrativeAreal2DOccupancyDataPostgreSQLConverter is null)
            {
                return false;
            }

            // One counter per side: the progress report runs over both, the finish line of each side names its own rows.
            long totalUpdated = 0;
            long updatedCount_AdministrativeAreal2Ds = 0;
            long updatedCount_Building2Ds = 0;

            if (includeAdministrativeAreal2Ds)
            {
                if (clear)
                {
                    await administrativeAreal2DOccupancyDataPostgreSQLConverter.ClearAsync(commandTimeout, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Subdivisions = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Subdivision, cancellationToken: cancellationToken, commandTimeout: commandTimeout);
                List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Municipalities = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Municipality, cancellationToken: cancellationToken, commandTimeout: commandTimeout);
                List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Counties = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, cancellationToken: cancellationToken, commandTimeout: commandTimeout);
                List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Voivodeships = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Voivodeship, cancellationToken: cancellationToken, commandTimeout: commandTimeout);
                List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Countries = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Country, cancellationToken: cancellationToken, commandTimeout: commandTimeout);

                ILookup<int?, AdministrativeAreal2DReference>? subdivisionsByMunicipalityId = administrativeAreal2DReferences_Subdivisions?.Where(s => s != null && s.MunicipalityId.HasValue).ToLookup(s => s.MunicipalityId);
                ILookup<int?, AdministrativeAreal2DReference>? directSubdivisionsByCountyId = administrativeAreal2DReferences_Subdivisions?.Where(s => s != null && s.MunicipalityId == null && s.CountyId.HasValue).ToLookup(s => s.CountyId);
                ILookup<int?, AdministrativeAreal2DReference>? municipalitiesByCountyId = administrativeAreal2DReferences_Municipalities?.Where(m => m != null && m.CountyId.HasValue).ToLookup(m => m.CountyId);
                ILookup<int?, AdministrativeAreal2DReference>? countiesByVoivodeshipId = administrativeAreal2DReferences_Counties?.Where(c => c != null && c.VoivodeshipId.HasValue).ToLookup(c => c.VoivodeshipId);
                ILookup<int?, AdministrativeAreal2DReference>? voivodeshipsByCountryId = administrativeAreal2DReferences_Voivodeships?.Where(v => v != null && v.CountryId.HasValue).ToLookup(v => v.CountryId);

                Dictionary<int, OccupancyData> occupancyDatas_ById = [];

                // The subdivisions that lie inside another subdivision of their county. A nested layer - a city, its
                // districts, their neighbourhoods - carries the same people on every level, so the roll-up above
                // it sums the top level only; summing every row counted Warsaw three times over.
                HashSet<int> subdivisionIds_Nested = [];

                HashSet<int> countyIds_ForSubdivisions = [.. administrativeAreal2DReferences_Counties?.Select(c => c.Id) ?? []];
                if (administrativeAreal2DReferences_Subdivisions is not null)
                {
                    foreach (AdministrativeAreal2DReference administrativeAreal2DReference_Subdivision in administrativeAreal2DReferences_Subdivisions)
                    {
                        if (administrativeAreal2DReference_Subdivision.CountyId.HasValue)
                        {
                            countyIds_ForSubdivisions.Add(administrativeAreal2DReference_Subdivision.CountyId.Value);
                        }
                    }
                }

                foreach (int countyId in countyIds_ForSubdivisions)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    List<AdministrativeAreal2D>? countySubdivisions = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(AdministrativeArealType.Subdivision, countyId, cancellationToken: cancellationToken);
                    if (countySubdivisions is null || countySubdivisions.Count == 0)
                    {
                        continue;
                    }

                    Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = [];

                    foreach (AdministrativeAreal2D countySubdivision in countySubdivisions)
                    {
                        GIS.Classes.AdministrativeAreal2D? administrativeAreal2D = countySubdivision.ToDiGi();
                        if (administrativeAreal2D is AdministrativeSubdivision administrativeSubdivision)
                        {
                            // Read into a local: the property hands back a clone per access.
                            PolygonalFace2D? polygonalFace2D = administrativeSubdivision.PolygonalFace2D;
                            if (polygonalFace2D is not null)
                            {
                                polygonalFace2Ds_ById[countySubdivision.Id] = polygonalFace2D;
                            }

                            // A missing figure stays missing. Writing a zero in its place would hand the building
                            // side a figure to distribute, and it would distribute nothing to every building.
                            occupancyDatas_ById[countySubdivision.Id] = new OccupancyData(countySubdivision.Reference, polygonalFace2D?.GetArea() ?? 0, administrativeSubdivision.Occupancy);
                        }
                    }

                    foreach (KeyValuePair<int, List<int>> keyValuePair in polygonalFace2Ds_ById.ContainerIds())
                    {
                        if (keyValuePair.Value.Count != 0)
                        {
                            subdivisionIds_Nested.Add(keyValuePair.Key);
                        }
                    }
                }

                if (administrativeAreal2DReferences_Municipalities is not null && administrativeAreal2DReferences_Municipalities.Count != 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    List<AdministrativeAreal2D>? municipalities = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(AdministrativeArealType.Municipality, cancellationToken: cancellationToken);
                    if (municipalities is not null)
                    {
                        foreach (AdministrativeAreal2D municipality in municipalities)
                        {
                            GIS.Classes.AdministrativeAreal2D? administrativeAreal2D = municipality.ToDiGi();
                            uint occupancy = 0;

                            if (subdivisionsByMunicipalityId is not null)
                            {
                                foreach (AdministrativeAreal2DReference administrativeAreal2DReference_Subdivision in subdivisionsByMunicipalityId[municipality.Id])
                                {
                                    if (subdivisionIds_Nested.Contains(administrativeAreal2DReference_Subdivision.Id))
                                    {
                                        continue;
                                    }

                                    if (occupancyDatas_ById.TryGetValue(administrativeAreal2DReference_Subdivision.Id, out OccupancyData? occupancyData) && occupancyData?.Occupancy is not null)
                                    {
                                        occupancy += occupancyData.Occupancy.Value;
                                    }
                                }
                            }

                            occupancyDatas_ById[municipality.Id] = new OccupancyData(municipality.Reference, administrativeAreal2D?.PolygonalFace2D?.GetArea() ?? 0, occupancy);
                        }
                    }
                }

                if (administrativeAreal2DReferences_Counties is not null && administrativeAreal2DReferences_Counties.Count != 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    List<AdministrativeAreal2D>? counties = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(AdministrativeArealType.County, cancellationToken: cancellationToken);
                    if (counties is not null)
                    {
                        foreach (AdministrativeAreal2D county in counties)
                        {
                            GIS.Classes.AdministrativeAreal2D? administrativeAreal2D = county.ToDiGi();
                            uint occupancy = 0;

                            if (municipalitiesByCountyId is not null)
                            {
                                foreach (AdministrativeAreal2DReference administrativeAreal2DReference_Municipality in municipalitiesByCountyId[county.Id])
                                {
                                    if (occupancyDatas_ById.TryGetValue(administrativeAreal2DReference_Municipality.Id, out OccupancyData? occupancyData) && occupancyData?.Occupancy is not null)
                                    {
                                        occupancy += occupancyData.Occupancy.Value;
                                    }
                                }
                            }

                            if (directSubdivisionsByCountyId is not null)
                            {
                                foreach (AdministrativeAreal2DReference administrativeAreal2DReference_DirectSubdivision in directSubdivisionsByCountyId[county.Id])
                                {
                                    if (subdivisionIds_Nested.Contains(administrativeAreal2DReference_DirectSubdivision.Id))
                                    {
                                        continue;
                                    }

                                    if (occupancyDatas_ById.TryGetValue(administrativeAreal2DReference_DirectSubdivision.Id, out OccupancyData? occupancyData) && occupancyData?.Occupancy is not null)
                                    {
                                        occupancy += occupancyData.Occupancy.Value;
                                    }
                                }
                            }

                            occupancyDatas_ById[county.Id] = new OccupancyData(county.Reference, administrativeAreal2D?.PolygonalFace2D?.GetArea() ?? 0, occupancy);
                        }
                    }
                }

                if (administrativeAreal2DReferences_Voivodeships is not null && administrativeAreal2DReferences_Voivodeships.Count != 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    List<AdministrativeAreal2D>? voivodeships = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(AdministrativeArealType.Voivodeship, cancellationToken: cancellationToken);
                    if (voivodeships is not null)
                    {
                        foreach (AdministrativeAreal2D voivodeship in voivodeships)
                        {
                            GIS.Classes.AdministrativeAreal2D? administrativeAreal2D = voivodeship.ToDiGi();
                            uint occupancy = 0;

                            if (countiesByVoivodeshipId is not null)
                            {
                                foreach (AdministrativeAreal2DReference administrativeAreal2DReference_County in countiesByVoivodeshipId[voivodeship.Id])
                                {
                                    if (occupancyDatas_ById.TryGetValue(administrativeAreal2DReference_County.Id, out OccupancyData? occupancyData) && occupancyData?.Occupancy is not null)
                                    {
                                        occupancy += occupancyData.Occupancy.Value;
                                    }
                                }
                            }

                            occupancyDatas_ById[voivodeship.Id] = new OccupancyData(voivodeship.Reference, administrativeAreal2D?.PolygonalFace2D?.GetArea() ?? 0, occupancy);
                        }
                    }
                }

                if (administrativeAreal2DReferences_Countries is not null && administrativeAreal2DReferences_Countries.Count != 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    List<AdministrativeAreal2D>? countries = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(AdministrativeArealType.Country, cancellationToken: cancellationToken);
                    if (countries is not null)
                    {
                        foreach (AdministrativeAreal2D country in countries)
                        {
                            GIS.Classes.AdministrativeAreal2D? administrativeAreal2D = country.ToDiGi();
                            uint occupancy = 0;

                            if (voivodeshipsByCountryId is not null)
                            {
                                foreach (AdministrativeAreal2DReference administrativeAreal2DReference_Voivodeship in voivodeshipsByCountryId[country.Id])
                                {
                                    if (occupancyDatas_ById.TryGetValue(administrativeAreal2DReference_Voivodeship.Id, out OccupancyData? occupancyData) && occupancyData?.Occupancy is not null)
                                    {
                                        occupancy += occupancyData.Occupancy.Value;
                                    }
                                }
                            }

                            occupancyDatas_ById[country.Id] = new OccupancyData(country.Reference, administrativeAreal2D?.PolygonalFace2D?.GetArea() ?? 0, occupancy);
                        }
                    }
                }

                if (occupancyDatas_ById.Count != 0)
                {
                    List<AdministrativeAreal2DOccupancyData> administrativeAreal2DOccupancyDatas = [];
                    foreach (OccupancyData occupancyData in occupancyDatas_ById.Values)
                    {
                        if (occupancyData.ToPostgreSQL() is AdministrativeAreal2DOccupancyData administrativeAreal2DOccupancyData)
                        {
                            administrativeAreal2DOccupancyDatas.Add(administrativeAreal2DOccupancyData);
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    await administrativeAreal2DOccupancyDataPostgreSQLConverter.UpdateAsync(administrativeAreal2DOccupancyDatas, commandTimeout);

                    updatedCount_AdministrativeAreal2Ds += administrativeAreal2DOccupancyDatas.Count;
                    totalUpdated += administrativeAreal2DOccupancyDatas.Count;
                    progress.Report(totalUpdated);
                }

                Serilog.Modify.Log("{Type}: administrative side finished, {UpdatedCount} rows written", nameof(PostgreSQLUpdateOccupancyTask), updatedCount_AdministrativeAreal2Ds);
            }

            if (includeBuilding2Ds)
            {
                Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
                if (building2DOccupancyDataPostgreSQLConverter is null)
                {
                    return false;
                }

                Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
                if (building2DPostgreSQLConverter is null)
                {
                    return false;
                }

                // A scoped run clears per county below, once its buildings are known, rather than truncating the
                // rows of every county it is not going to rewrite.
                if (clear && countyIds_Scope is null)
                {
                    await building2DOccupancyDataPostgreSQLConverter.ClearAsync(commandTimeout, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, cancellationToken: cancellationToken, commandTimeout: commandTimeout);
                List<AdministrativeAreal2DReference>? subdivisionReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Subdivision, cancellationToken: cancellationToken, commandTimeout: commandTimeout);

                if (subdivisionReferences is null || subdivisionReferences.Count == 0)
                {
                    return true;
                }

                // A building and the subdivision holding its occupancy are not guaranteed to be filed under the same
                // county polygon part, so the subdivision side of the pairing is widened to every part sharing the
                // parent's code - the same rule the building data update scopes its runs by. Keying the pairing on the
                // part alone left every building of a multi-part county whose subdivisions sit under a sibling part
                // without a stored occupancy record, and calculated_occupancy unwritten.
                Dictionary<int, HashSet<int>> siblingCountyGroups = countyReferences.SiblingCountyGroups();
                Dictionary<int, HashSet<int>> inScopeSubdivisionIds_ByCountyId = Query.InScopeSubdivisionIds(subdivisionReferences, siblingCountyGroups);

                Dictionary<int, AdministrativeAreal2DReference> subdivisionReferences_ById = [];
                foreach (AdministrativeAreal2DReference subdivisionReference in subdivisionReferences)
                {
                    if (subdivisionReference is not null)
                    {
                        subdivisionReferences_ById[subdivisionReference.Id] = subdivisionReference;
                    }
                }

                HashSet<int> countyIds = [.. countyReferences?.Select(c => c.Id) ?? []];
                foreach (AdministrativeAreal2DReference subdivisionReference in subdivisionReferences)
                {
                    if (subdivisionReference.CountyId.HasValue)
                    {
                        countyIds.Add(subdivisionReference.CountyId.Value);
                    }
                }

                if (countyIds_Scope is not null)
                {
                    countyIds.IntersectWith(countyIds_Scope);
                }

                Serilog.Modify.Log("{Type}: building side starting over {CountyCount} counties, scope {CountyScope}", nameof(PostgreSQLUpdateOccupancyTask), countyIds.Count, countyIds_Scope is null ? "all" : string.Join(", ", countyIds_Scope.OrderBy(x => x)));

                foreach (int countyId in countyIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    List<AdministrativeAreal2DReference> countySubdivisions = [];
                    if (inScopeSubdivisionIds_ByCountyId.TryGetValue(countyId, out HashSet<int>? inScopeSubdivisionIds))
                    {
                        foreach (int inScopeSubdivisionId in inScopeSubdivisionIds)
                        {
                            if (subdivisionReferences_ById.TryGetValue(inScopeSubdivisionId, out AdministrativeAreal2DReference? subdivisionReference_InScope))
                            {
                                countySubdivisions.Add(subdivisionReference_InScope);
                            }
                        }
                    }

                    if (countySubdivisions.Count == 0)
                    {
                        continue;
                    }

                    List<Building2D>? countyBuildings = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(countyId, subdivisionId: null, excludedReferences: null, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                    if (countyBuildings is null || countyBuildings.Count == 0)
                    {
                        continue;
                    }

                    if (clear && countyIds_Scope is not null)
                    {
                        // The scoped clear: exactly the rows of this county's buildings, so a building whose
                        // subdivision carries no figure does not keep the share an earlier run gave it.
                        List<string> countyBuildingReferences = [.. countyBuildings.Where(x => !string.IsNullOrWhiteSpace(x?.Reference)).Select(x => x.Reference!)];

                        const int batchSize = 1000;
                        for (int i = 0; i < countyBuildingReferences.Count; i += batchSize)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            await building2DOccupancyDataPostgreSQLConverter.RemoveAsync(countyBuildingReferences.Skip(i).Take(batchSize), countyId, commandTimeout, cancellationToken);
                        }
                    }

                    // The figure comes off the subdivision's own row - the source - not off the administrative
                    // occupancy table, which is a copy this task writes: read from the copy, a missing figure is
                    // indistinguishable from a zero an older build stored in its place, and the answer would
                    // depend on whether the administrative side had run first.
                    Dictionary<int, uint?> occupancies_BySubdivisionId = [];
                    Dictionary<int, PolygonalFace2D> polygonalFace2Ds_BySubdivisionId = [];

                    List<AdministrativeAreal2D>? countySubdivisions_Rows = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByIdsAsync(countySubdivisions.Select(x => x.Id), commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                    if (countySubdivisions_Rows is not null)
                    {
                        foreach (AdministrativeAreal2D countySubdivision_Row in countySubdivisions_Rows)
                        {
                            if (countySubdivision_Row?.ToDiGi() is AdministrativeSubdivision administrativeSubdivision)
                            {
                                occupancies_BySubdivisionId[countySubdivision_Row.Id] = administrativeSubdivision.Occupancy;

                                // Read into a local: the property hands back a clone per access.
                                PolygonalFace2D? polygonalFace2D = administrativeSubdivision.PolygonalFace2D;
                                if (polygonalFace2D is not null)
                                {
                                    polygonalFace2Ds_BySubdivisionId[countySubdivision_Row.Id] = polygonalFace2D;
                                }
                            }
                        }
                    }

                    // The nesting of the layer, containers smallest first: it names the figured subdivision a
                    // building's share comes from, and the buildings a figured subdivision's figure is spread over.
                    Dictionary<int, List<int>> containerIds_BySubdivisionId = polygonalFace2Ds_BySubdivisionId.ContainerIds();

                    ILookup<int?, Building2D> buildingsBySubdivisionId = countyBuildings.Where(b => b?.SubdivisionId != null).ToLookup(b => b.SubdivisionId);

                    // Two collections per figured subdivision: every building inside it (its own and its
                    // descendants'), which its figure is spread over; and the buildings that take their share
                    // from it, because it is the smallest figured subdivision they sit in. A figured leaf has the
                    // two equal; a district has its figure spread over the whole district but hands a share only
                    // to the buildings outside its figured neighbourhoods.
                    Dictionary<int, List<Building2D>> buildingsInside_ByFiguredSubdivisionId = [];
                    Dictionary<int, HashSet<string>> references_ByFiguredSubdivisionId = [];

                    foreach (AdministrativeAreal2DReference subdivisionReference in countySubdivisions)
                    {
                        List<Building2D> subdivisionBuildings = [.. buildingsBySubdivisionId[subdivisionReference.Id]];
                        if (subdivisionBuildings.Count == 0)
                        {
                            continue;
                        }

                        int? occupancySubdivisionId = Query.OccupancySubdivisionId(subdivisionReference.Id, containerIds_BySubdivisionId, occupancies_BySubdivisionId);

                        // No figure on the subdivision nor on any container: nothing to distribute. Writing a zero
                        // would read back as a measurement, so the gap is counted instead.
                        if (occupancySubdivisionId is not int figuredSubdivisionId)
                        {
                            MissingOccupancySubdivisionCount++;
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Occupancy not distributed - county {CountyId}, subdivision {SubdivisionId} {SubdivisionName} carries no occupancy figure and neither does any subdivision containing it, its {BuildingCount} buildings left unwritten", countyId, subdivisionReference.Id, subdivisionReference.Name ?? string.Empty, subdivisionBuildings.Count);
                            continue;
                        }

                        if (figuredSubdivisionId != subdivisionReference.Id)
                        {
                            BorrowedOccupancySubdivisionCount++;
                            Serilog.Modify.Log("Occupancy borrowed - county {CountyId}, subdivision {SubdivisionId} {SubdivisionName} carries no occupancy figure, its {BuildingCount} buildings take the share of subdivision {FiguredSubdivisionId}", countyId, subdivisionReference.Id, subdivisionReference.Name ?? string.Empty, subdivisionBuildings.Count, figuredSubdivisionId);
                        }

                        if (!references_ByFiguredSubdivisionId.TryGetValue(figuredSubdivisionId, out HashSet<string>? references))
                        {
                            references = [];
                            references_ByFiguredSubdivisionId[figuredSubdivisionId] = references;
                        }

                        foreach (Building2D subdivisionBuilding in subdivisionBuildings)
                        {
                            if (subdivisionBuilding?.Reference is string reference)
                            {
                                references.Add(reference);
                            }
                        }

                        // The figure is spread over the buildings of every figured container as well, up the chain.
                        List<int> subdivisionIds_Spread = [subdivisionReference.Id];
                        if (containerIds_BySubdivisionId.TryGetValue(subdivisionReference.Id, out List<int>? containerIds))
                        {
                            subdivisionIds_Spread.AddRange(containerIds);
                        }

                        foreach (int subdivisionId_Spread in subdivisionIds_Spread)
                        {
                            if (!occupancies_BySubdivisionId.TryGetValue(subdivisionId_Spread, out uint? occupancy_Spread) || occupancy_Spread is null)
                            {
                                continue;
                            }

                            if (!buildingsInside_ByFiguredSubdivisionId.TryGetValue(subdivisionId_Spread, out List<Building2D>? buildingsInside))
                            {
                                buildingsInside = [];
                                buildingsInside_ByFiguredSubdivisionId[subdivisionId_Spread] = buildingsInside;
                            }

                            buildingsInside.AddRange(subdivisionBuildings);
                        }
                    }

                    List<Building2DOccupancyData> countyBuilding2DOccupancyDatas = [];

                    foreach (KeyValuePair<int, HashSet<string>> keyValuePair in references_ByFiguredSubdivisionId)
                    {
                        if (!occupancies_BySubdivisionId.TryGetValue(keyValuePair.Key, out uint? occupancy_Figured) || occupancy_Figured is not uint occupancy || !buildingsInside_ByFiguredSubdivisionId.TryGetValue(keyValuePair.Key, out List<Building2D>? buildingsInside))
                        {
                            continue;
                        }

                        foreach (Building2DOccupancyData building2DOccupancyData in CalculateBuilding2DOccupancyDatas(countyId, buildingsInside, occupancy))
                        {
                            if (building2DOccupancyData?.Reference is string reference && keyValuePair.Value.Contains(reference))
                            {
                                countyBuilding2DOccupancyDatas.Add(building2DOccupancyData);
                            }
                        }
                    }

                    if (countyBuilding2DOccupancyDatas.Count > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await building2DOccupancyDataPostgreSQLConverter.UpdateAsync(countyBuilding2DOccupancyDatas, commandTimeout, cancellationToken);

                        updatedCount_Building2Ds += countyBuilding2DOccupancyDatas.Count;
                        totalUpdated += countyBuilding2DOccupancyDatas.Count;
                        progress.Report(totalUpdated);
                    }
                }

                Serilog.Modify.Log(
                    MissingOccupancySubdivisionCount == 0 ? Serilog.Enums.LogEventLevel.Information : Serilog.Enums.LogEventLevel.Warning,
                    "{Type}: building side finished, {UpdatedCount} rows written, {BorrowedOccupancySubdivisionCount} subdivisions without a figure of their own took a container's, {MissingOccupancySubdivisionCount} subdivisions with buildings but no occupancy figure anywhere up their chain left unwritten",
                    nameof(PostgreSQLUpdateOccupancyTask), updatedCount_Building2Ds, BorrowedOccupancySubdivisionCount, MissingOccupancySubdivisionCount);
            }

            return true;

            static List<Building2DOccupancyData> CalculateBuilding2DOccupancyDatas(int countyId, List<Building2D> subdivisionBuildings, uint occupancy)
            {
                List<GIS.Classes.Building2D> building2Ds = [];
                List<double> occupancyAreas = [];

                foreach (Building2D building2D_Raw in subdivisionBuildings)
                {
                    if (building2D_Raw?.ToDiGi() is not GIS.Classes.Building2D building2D || GIS.Query.OccupancyArea(building2D) is not double occupancyArea)
                    {
                        continue;
                    }

                    building2Ds.Add(building2D);
                    occupancyAreas.Add(occupancyArea);
                }

                List<uint> occupancies = GIS.Query.Occupancies(occupancyAreas, occupancy);
                if (occupancies.Count != building2Ds.Count)
                {
                    return [];
                }

                List<Building2DOccupancyData> result = [];
                for (int i = 0; i < building2Ds.Count; i++)
                {
                    if (new OccupancyData(building2Ds[i].Reference, occupancyAreas[i], occupancies[i]).ToPostgreSQL(countyId) is Building2DOccupancyData building2DOccupancyData)
                    {
                        result.Add(building2DOccupancyData);
                    }
                }

                return result;
            }
        }
    }
}