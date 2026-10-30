using DiGi.Analytical.Building.Interfaces;
using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.IO;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.Geometry.Spatial.Classes;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Modify
    {
        /// <summary>
        /// Classifies the components of the stored <see cref="DiGi.Analytical.Building.Classes.BuildingModel"/>s into the 35 “External Components Area” columns of the given table, keyed by county identifier and reference.
        /// <para>A wall is filed under the sector of its outward normal’s azimuth, a roof under its tilt band (flat below 5°, then [5°, 20°], (20°, 45°] and above 45°) crossed with the same sectors, and a floor under the floor column; the total column is the sum of the 34 breakdowns. A building that arrives with a stored model gets a row in which every empty bucket is 0, so a zero row and an absent row stay distinguishable.</para>
        /// <para>The wall outward and roof upward normals are the normals of the faces of the model’s external envelope, built by <see cref="DiGi.Analytical.Building.Classes.BuildingModel.GetExternalShell(DiGi.Geometry.Core.Enums.Side?, DiGi.Geometry.Core.Enums.Orientation?, DiGi.Geometry.Core.Enums.Orientation?, double)"/> with <see cref="DiGi.Geometry.Core.Enums.Side.External"/>, so each face direction is resolved once over the envelope instead of being guessed from the component’s stored geometry. That method states the selection rule for every consumer: a component bounding exactly one space is external, one bounding two is an internal partition, one bounding none is part of no envelope. Every envelope face carries the <see cref="DiGi.Core.Interfaces.IUniqueReference"/> of the component it was built from, which is how a face is matched back to its component; a component the envelope carries is classified from its face, one it does not carry is excluded from the external area and counted in the result when it bounds two spaces, and is a defect in the model’s space structure otherwise - a component bounding one space that the envelope still leaves out has no polygonal face.</para>
        /// <para>A model that carries components but no envelope at all - fewer than the four external faces a closed solid needs, typically a sliver footprint with walls and no roof or floor - is degenerate: nothing gives its components an outward normal, so it gets no row and its reference is listed in <see cref="ExternalComponentsAreaResult.DegenerateReferences"/>. A model refused as a defect gets no row either and is listed with its reason in <see cref="ExternalComponentsAreaResult.FailedReferences"/>; neither stops the classification of the other models, and neither leaves a partial row, because a row is only built once its model is classified.</para>
        /// <para>The finest tolerance at which the envelope edge-pairs into a closed surface is recorded beside the areas - the closing tolerance column, null when the envelope closes at no rung of the ladder 1e-6 to 0.2 m or the model carries no external components. Ray parity is sound only for a closed face set, so a null closing tolerance is the signal that the row’s sector and tilt values may rest on an arbitrary face side; such a model is counted in the result, not failed.</para>
        /// <para>A component the method cannot classify - a wall whose normal is vertical, so its azimuth is undefined - is skipped and counted in the result.</para>
        /// <para>A reference can arrive several times (several stored versions of the model); the first record of a given county and reference is the one that is written and the rest are stepped over, so the collection has to reach this method in the caller’s order of preference - the converter returns the newest record first.</para>
        /// </summary>
        /// <param name="table">The table to fill with the classification.</param>
        /// <param name="buildingModels">The envelopes of stored building models, most preferred record first.</param>
        /// <returns>The outcome of the classification: the number of components skipped because they bound two spaces or their target bucket is undefined, the number of models whose external envelope does not close on the tolerance ladder, and the references of the degenerate and the refused models - the counts 0 and the lists empty when every component was classified over a closed envelope. The tallies cover the written rows only.</returns>
        public static ExternalComponentsAreaResult Update_ExternalComponentsArea(this Table? table, IEnumerable<BuildingModel>? buildingModels)
        {
            return Update_ExternalComponentsArea(table, buildingModels, null);
        }

        /// <summary>
        /// Classifies the components of the stored building models into the 35 “External Components Area” columns, as <see cref="Update_ExternalComponentsArea(Table?, IEnumerable{BuildingModel}?)"/> does, and files the wall area each building shares with adjacent buildings into the 9 party wall area columns (DiGi-Organization/DiGi.GIS.Analytical#4).
        /// <para>The party walls come from <see cref="DiGi.Analytical.Building.Query.PartyWalls"/> and are matched to the walls by the identifier of the wall component. The shared area of a wall is filed under the sector of that wall's envelope face - the sector the wall itself is filed under - and is capped at the wall's area, so a wall shared with two overlapping neighbours never counts more than itself. The external wall columns stay gross: they include the party walls, and the net external area of a sector is its external column minus its party column.</para>
        /// <para>With <paramref name="partyWallResults"/> null the party wall columns are not touched; with an empty collection they are written as 0, so a building with no party wall and a building not examined stay distinguishable.</para>
        /// </summary>
        /// <param name="table">The table to fill with the classification.</param>
        /// <param name="buildingModels">The envelopes of stored building models, most preferred record first.</param>
        /// <param name="partyWallResults">The party walls of the models, or null to leave the party wall columns alone.</param>
        /// <returns>The outcome of the classification, as for <see cref="Update_ExternalComponentsArea(Table?, IEnumerable{BuildingModel}?)"/>.</returns>
        public static ExternalComponentsAreaResult Update_ExternalComponentsArea(this Table? table, IEnumerable<BuildingModel>? buildingModels, IEnumerable<DiGi.Analytical.Building.Classes.PartyWallResult?>? partyWallResults)
        {
            // Must stay in sync with the DiGi.GIS.IO column descriptions: flat is strictly below 5°,
            // the first tilted band is [5°, 20°], the second is (20°, 45°], the third above 45°.
            const double tilt_FlatDegrees = 5.0;
            const double tilt_UpTo20Degrees = 20.0;
            const double tilt_Between45Degrees = 45.0;

            double Area(string reference, int countyId, PolygonalFace3D face, string componentKind)
            {
                double area = face.GetArea();
                if (double.IsNaN(area) || area <= 0)
                {
                    throw new InvalidOperationException($"Building {reference} of county {countyId}: the {componentKind} face has no usable area, so it cannot be classified.");
                }

                return area;
            }

            Vector3D? NormalUnit(string reference, int countyId, DiGi.Analytical.Classes.Face face_Shell, string componentKind)
            {
                Vector3D? normal = face_Shell.Plane?.Normal;
                if (normal is null || normal.Length <= 0)
                {
                    throw new InvalidOperationException($"Building {reference} of county {countyId}: the {componentKind} face has no usable normal, so it cannot be classified.");
                }

                Vector3D? normal_Unit = normal.Unit;
                if (normal_Unit is null)
                {
                    throw new InvalidOperationException($"Building {reference} of county {countyId}: the {componentKind} face has no usable normal, so it cannot be classified.");
                }

                return normal_Unit;
            }

            // The number of spaces a component bounds - the same count the selection rule of GetExternalShell reads.
            // Only reached for a component the envelope does not carry, so the clone GetRelation returns is paid a few times per model at most.
            int SpaceCount(DiGi.Analytical.Building.Classes.BuildingModel buildingModel, IComponent component)
            {
                DiGi.Analytical.Building.Classes.SpaceRelation? spaceRelation = buildingModel.GetRelation<DiGi.Analytical.Building.Classes.SpaceRelation>(component);

                // The getter clones the reference list, so it is read once.
                List<DiGi.Core.Interfaces.IUniqueReference>? uniqueReferences = spaceRelation?.UniqueReferences_To;

                return uniqueReferences?.Count ?? 0;
            }

            // Wall sectors, in the order of the columns: north, northeast, east, southeast, south, southwest, west, northwest.
            int SectorIndex(DiGi.GIS.Enums.CardinalDirection direction)
            {
                int index = direction switch
                {
                    DiGi.GIS.Enums.CardinalDirection.North => 0,
                    DiGi.GIS.Enums.CardinalDirection.NorthEast => 1,
                    DiGi.GIS.Enums.CardinalDirection.East => 2,
                    DiGi.GIS.Enums.CardinalDirection.SouthEast => 3,
                    DiGi.GIS.Enums.CardinalDirection.South => 4,
                    DiGi.GIS.Enums.CardinalDirection.SouthWest => 5,
                    DiGi.GIS.Enums.CardinalDirection.West => 6,
                    DiGi.GIS.Enums.CardinalDirection.NorthWest => 7,
                    _ => -1
                };

                return index;
            }

            long skippedComponentCount = 0;
            long openEnvelopeCount = 0;
            List<string> references_Degenerate = [];
            List<string> references_Failed = [];

            if (table is null || buildingModels is null)
            {
                return new ExternalComponentsAreaResult(skippedComponentCount, openEnvelopeCount);
            }

            Column? column_CountyId = table.UpdateColumn<Column>(IO.Constants.Column.CountyId);
            Column? column_Reference = table.UpdateColumn<Column>(IO.Constants.Column.Reference);

            // The base-typed accessor keeps this project free of a DiGi.Unit.IO reference: the field behind it is a UnitColumn,
            // and referencing that assembly would make the DiGi.Unit namespace shadow DiGi.BDL.Classes.Unit everywhere here.
            Column? column_ClosingTolerance = table.UpdateColumn<Column>(IO.Create.Column_ClosingTolerance());
            if (column_CountyId is null || column_Reference is null)
            {
                return new ExternalComponentsAreaResult(skippedComponentCount, openEnvelopeCount);
            }

            List<Column> columns_External = [];
            foreach (Column column in IO.Create.Columns_ExternalComponentsArea())
            {
                Column? column_Added = table.UpdateColumn<Column>(column);
                if (column_Added is not null)
                {
                    columns_External.Add(column_Added);
                }
            }

            // The party wall columns: 8 sectors in the order of the wall columns, and the total last. Each wall's shared area is summed over its neighbours.
            List<Column>? columns_Party = null;
            Dictionary<Guid, double> areas_Party_ByWallGuid = [];
            if (partyWallResults is not null)
            {
                columns_Party = [];
                foreach (Column column in IO.Create.Columns_PartyWallArea())
                {
                    Column? column_Added = table.UpdateColumn<Column>(column);
                    if (column_Added is not null)
                    {
                        columns_Party.Add(column_Added);
                    }
                }

                foreach (DiGi.Analytical.Building.Classes.PartyWallResult? partyWallResult in partyWallResults)
                {
                    if (partyWallResult?.Component is not DiGi.Core.Classes.GuidReference guidReference_Wall || !(partyWallResult.Area > 0))
                    {
                        continue;
                    }

                    areas_Party_ByWallGuid[guidReference_Wall.Guid] = (areas_Party_ByWallGuid.TryGetValue(guidReference_Wall.Guid, out double area_Party) ? area_Party : 0) + partyWallResult.Area;
                }
            }

            // The 34 breakdown columns come first and the total column last, in the order Columns_ExternalComponentsArea declares them.
            int count_Breakdown = columns_External.Count - 1;
            if (count_Breakdown < 1)
            {
                return new ExternalComponentsAreaResult(skippedComponentCount, openEnvelopeCount);
            }

            // The closing-tolerance ladder of the #84 verification (IMPLEMENTATION_PLAN_issue84.md §8): the canonical
            // Distance and MacroDistance rungs where they exist, and the measured candidate tolerances between and above them.
            double[] tolerances_Closing = [DiGi.Core.Constants.Tolerance.Distance, 1e-5, 1e-4, DiGi.Core.Constants.Tolerance.MacroDistance, 0.01, 0.02, 0.05, 0.1, 0.2];

            // Column indexes, in that declared order: 8 wall sectors, then the flat roof at 8,
            // then the 24 tilted-roof bands (8 sectors x 3 bands), then the floor last among the breakdowns.
            int index_FlatRoof = 8;
            int index_TiltedRoof = 9;
            int index_Floor = count_Breakdown - 1;

            Dictionary<(int, string), Row> dictionary_Row = Query.RowsByCountyIdAndReference(table, column_CountyId, column_Reference);
            HashSet<(int, string)> written = [];

            foreach (BuildingModel? buildingModel_Envelope in buildingModels)
            {
                if (buildingModel_Envelope is null)
                {
                    continue;
                }

                string? reference = buildingModel_Envelope.Reference;
                if (string.IsNullOrWhiteSpace(reference))
                {
                    continue;
                }

                if (buildingModel_Envelope.CountyId is not int countyId)
                {
                    continue;
                }

                if (!written.Add((countyId, reference)))
                {
                    continue;
                }

                // A model refused as a defect costs that model alone: it is recorded with its reason and gets no row,
                // while the rest of the collection is still classified. Nothing is written to the table before the
                // classification of the model completes, so a refused model leaves no partial row behind.
                // The tallies are rolled back for a refused model, so they describe the written rows only.
                long skippedComponentCount_Before = skippedComponentCount;
                long openEnvelopeCount_Before = openEnvelopeCount;

                double[] areas = new double[count_Breakdown];
                double[] areas_Party = new double[8];
                double? closingTolerance = null;
                try
                {
                    if (!Classify(buildingModel_Envelope, reference, countyId, areas, areas_Party, out closingTolerance))
                    {
                        references_Degenerate.Add(reference);
                        continue;
                    }
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    skippedComponentCount = skippedComponentCount_Before;
                    openEnvelopeCount = openEnvelopeCount_Before;
                    references_Failed.Add($"{reference}: {invalidOperationException.Message}");
                    continue;
                }

                double totalArea = 0;
                foreach (double area in areas)
                {
                    totalArea += area;
                }

                Row? row = null;
                if (!dictionary_Row.TryGetValue((countyId, reference), out row) || row is null)
                {
                    row = table.AddRow();
                    IO.Modify.SetValue(row, column_CountyId, countyId);
                    IO.Modify.SetValue(row, column_Reference, reference);
                    dictionary_Row[(countyId, reference)] = row;
                }

                for (int i = 0; i < count_Breakdown; i++)
                {
                    IO.Modify.SetValue(row, columns_External[i], areas[i] > 0 ? (float)areas[i] : 0f);
                }

                IO.Modify.SetValue(row, columns_External[count_Breakdown], (float)totalArea);

                if (columns_Party is not null && columns_Party.Count == areas_Party.Length + 1)
                {
                    double totalArea_Party = 0;
                    for (int i = 0; i < areas_Party.Length; i++)
                    {
                        IO.Modify.SetValue(row, columns_Party[i], areas_Party[i] > 0 ? (float)areas_Party[i] : 0f);
                        totalArea_Party += areas_Party[i];
                    }

                    IO.Modify.SetValue(row, columns_Party[areas_Party.Length], (float)totalArea_Party);
                }

                // Null is the signal - the open envelope or the absent one - so nothing is written for it: a cell left
                // alone is null, and a sentinel would read as a tolerance.
                float? value_ClosingTolerance = closingTolerance is null ? null : (float)closingTolerance.Value;
                IO.Modify.SetValue(row, column_ClosingTolerance, value_ClosingTolerance);

                table.AddRow(row, false);
            }

            return new ExternalComponentsAreaResult(skippedComponentCount, openEnvelopeCount, references_Degenerate, references_Failed);

            // Classifies the components of one model into the given area buckets, and the shared part of each wall into the party buckets. False when the model carries components
            // but no external envelope (a degenerate model: nothing to classify by, and no row is written); throws
            // InvalidOperationException for a defect in the model's space structure.
            bool Classify(BuildingModel buildingModel_Envelope, string reference, int countyId, double[] areas, double[] areas_Party, out double? closingTolerance)
            {
                DiGi.Analytical.Building.Classes.BuildingModel? buildingModel = buildingModel_Envelope.ToDiGi();
                if (buildingModel is null)
                {
                    throw new InvalidOperationException($"Building {reference} of county {countyId}: the stored model does not rehydrate into an analytical BuildingModel, so its components cannot be classified.");
                }

                // GetComponents hands out clones, so each list is fetched exactly once and reused.
                List<IWall>? walls = buildingModel.GetComponents<IWall>();
                List<IRoof>? roofs = buildingModel.GetComponents<IRoof>();
                List<IFloor>? floors = buildingModel.GetComponents<IFloor>();

                // The open-envelope signal of the row: ray parity is sound only for a closed face set, so the finest
                // tolerance at which the envelope edge-pairs shut is what separates a trustworthy row from an
                // arbitrary-side one. The manifold criterion stays at its default - parity needs the face set welded
                // shut, not manifold - and a model whose envelope closes at no rung of the ladder is counted in the
                // result rather than failed: its row is written with a null closing tolerance.
                closingTolerance = null;

                bool hasComponent = (walls is not null && walls.Count > 0) || (roofs is not null && roofs.Count > 0) || (floors is not null && floors.Count > 0);
                if (hasComponent)
                {
                    // The external envelope of the whole model: every component bounding exactly one space, oriented once over the envelope by ray parity,
                    // so a stored normal that points into the building is flipped by topology rather than by guesswork (GetExternalShell states the selection rule).
                    // Null when the model has no space relation at all or fewer than four external faces - a degenerate model, typically a sliver
                    // footprint with walls and no roof or floor: no component has an outward normal to be classified by, so the model gets no row.
                    DiGi.Analytical.Classes.Shell? shell = buildingModel.GetExternalShell(DiGi.Geometry.Core.Enums.Side.External);
                    if (shell is null)
                    {
                        return false;
                    }

                    closingTolerance = DiGi.Geometry.Spatial.Query.ClosingTolerance(shell, tolerances_Closing);
                    if (closingTolerance is null)
                    {
                        openEnvelopeCount++;
                    }

                    // Every envelope face carries the GuidReference of the component it was built from; a component appears at most once.
                    Dictionary<Guid, DiGi.Analytical.Classes.Face> face_ByComponentGuid = [];

                    // The getter clones on access, so the list is read once.
                    List<DiGi.Analytical.Classes.Face>? faces = shell?.PolygonalFaces;
                    if (faces is not null)
                    {
                        foreach (DiGi.Analytical.Classes.Face face in faces)
                        {
                            // Face.UniqueReference is a fresh clone on every call - read it once, and match on the Guid the reference carries.
                            DiGi.Core.Interfaces.IUniqueReference? uniqueReference_Face = face.UniqueReference;
                            if (uniqueReference_Face is not DiGi.Core.Classes.GuidReference guidReference_Face)
                            {
                                continue;
                            }

                            if (!face_ByComponentGuid.TryAdd(guidReference_Face.Guid, face))
                            {
                                throw new InvalidOperationException($"Building {reference} of county {countyId}: the component {guidReference_Face.Guid} appears twice in the external envelope, so the model's space structure is defective and its components cannot be classified.");
                            }
                        }
                    }

                    if (walls is not null)
                    {
                        foreach (IWall? wall in walls)
                        {
                            if (wall is null)
                            {
                                continue;
                            }

                            Guid guid_Component = wall.Guid;
                            if (!face_ByComponentGuid.TryGetValue(guid_Component, out DiGi.Analytical.Classes.Face? face_Shell))
                            {
                                int spaceCount = SpaceCount(buildingModel, wall);
                                if (spaceCount == 2)
                                {
                                    // Bounds two spaces: an internal partition, not an external component.
                                    skippedComponentCount++;
                                    continue;
                                }

                                throw new InvalidOperationException($"Building {reference} of county {countyId}: the wall component {guid_Component} bounds {spaceCount} space(s) and is not part of the external envelope, so its outward normal is not available and it cannot be classified.");
                            }

                            double area = Area(reference, countyId, face_Shell, "wall");
                            Vector3D normal_Unit = NormalUnit(reference, countyId, face_Shell, "wall")!;

                            double azimuth = (Math.Atan2(normal_Unit.X, normal_Unit.Y) * 180.0 / Math.PI + 360.0) % 360.0;

                            int index = SectorIndex(DiGi.GIS.Query.CardinalDirection(azimuth));
                            if (index < 0)
                            {
                                // CardinalDirection answered Undefined: the normal is vertical, so the sector is not guessed at.
                                skippedComponentCount++;
                                continue;
                            }

                            areas[index] += area;

                            if (areas_Party_ByWallGuid.TryGetValue(guid_Component, out double area_Party))
                            {
                                // A wall shared with two overlapping neighbours cannot share more than itself.
                                areas_Party[index] += Math.Min(area_Party, area);
                            }
                        }
                    }

                    if (roofs is not null)
                    {
                        foreach (IRoof? roof in roofs)
                        {
                            if (roof is null)
                            {
                                continue;
                            }

                            Guid guid_Component = roof.Guid;
                            if (!face_ByComponentGuid.TryGetValue(guid_Component, out DiGi.Analytical.Classes.Face? face_Shell))
                            {
                                int spaceCount = SpaceCount(buildingModel, roof);
                                if (spaceCount == 2)
                                {
                                    // Bounds two spaces: an internal partition, not an external component.
                                    skippedComponentCount++;
                                    continue;
                                }

                                throw new InvalidOperationException($"Building {reference} of county {countyId}: the roof component {guid_Component} bounds {spaceCount} space(s) and is not part of the external envelope, so its upward normal is not available and it cannot be classified.");
                            }

                            double area = Area(reference, countyId, face_Shell, "roof");
                            Vector3D normal_Unit = NormalUnit(reference, countyId, face_Shell, "roof")!;

                            // The shell normal is the roof's upward direction; no flip is applied.
                            double tilt = normal_Unit.Angle(DiGi.Geometry.Spatial.Constants.Vector3D.WorldZ) * 180.0 / Math.PI;
                            if (tilt < tilt_FlatDegrees)
                            {
                                areas[index_FlatRoof] += area;
                                continue;
                            }

                            double azimuth = (Math.Atan2(normal_Unit.X, normal_Unit.Y) * 180.0 / Math.PI + 360.0) % 360.0;

                            int index = SectorIndex(DiGi.GIS.Query.CardinalDirection(azimuth));
                            if (index < 0)
                            {
                                // CardinalDirection answered Undefined: the normal is vertical, so the sector is not guessed at.
                                skippedComponentCount++;
                                continue;
                            }

                            int band = tilt <= tilt_UpTo20Degrees ? 0 : tilt <= tilt_Between45Degrees ? 1 : 2;
                            areas[index_TiltedRoof + 8 * band + index] += area;
                        }
                    }

                    if (floors is not null)
                    {
                        foreach (IFloor? floor_Temp in floors)
                        {
                            if (floor_Temp is null)
                            {
                                continue;
                            }

                            Guid guid_Component = floor_Temp.Guid;
                            if (!face_ByComponentGuid.TryGetValue(guid_Component, out DiGi.Analytical.Classes.Face? face_Shell))
                            {
                                int spaceCount = SpaceCount(buildingModel, floor_Temp);
                                if (spaceCount == 2)
                                {
                                    // Bounds two spaces: an internal partition, not an external component.
                                    skippedComponentCount++;
                                    continue;
                                }

                                throw new InvalidOperationException($"Building {reference} of county {countyId}: the floor component {guid_Component} bounds {spaceCount} space(s) and is not part of the external envelope, so its envelope face is not available and it cannot be classified.");
                            }

                            areas[index_Floor] += Area(reference, countyId, face_Shell, "floor");
                        }
                    }
                }

                return true;
            }
        }
    }
}
