#### [DiGi\.GIS\.PostgreSQL](DiGi.GIS.PostgreSQL.Overview.md 'DiGi\.GIS\.PostgreSQL\.Overview')

## DiGi\.GIS\.PostgreSQL\.Interfaces Namespace
### Interfaces

<a name='DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLConverter'></a>

## IGISPostgreSQLConverter Interface

Defines the contract for a GIS\-specific PostgreSQL converter\.

```csharp
public interface IGISPostgreSQLConverter : DiGi.PostgreSQL.Interfaces.IPostgreSQLConverter, DiGi.PostgreSQL.Interfaces.IPostgreSQLObject, DiGi.Core.Interfaces.IObject, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject
```

Derived  
↳ [AdministrativeAreal2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter')  
↳ [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')  
↳ [BuildingDataPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataPostgreSQLConverter')  
↳ [BuildingPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingPostgreSQLConverter')  
↳ [BuildingSolarRadiationPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingSolarRadiationPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingSolarRadiationPostgreSQLConverter')  
↳ [EPWFilePostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.EPWFilePostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.EPWFilePostgreSQLConverter')  
↳ [OrtoDatasPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter')  
↳ [ReferencedObjectPostgreSQLConverter&lt;TReferencedObject,TUniqueObject&gt;](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.ReferencedObjectPostgreSQLConverter_TReferencedObject,TUniqueObject_ 'DiGi\.GIS\.PostgreSQL\.Classes\.ReferencedObjectPostgreSQLConverter\<TReferencedObject,TUniqueObject\>')  
↳ [StatisticalDataCollectionPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.StatisticalDataCollectionPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.StatisticalDataCollectionPostgreSQLConverter')  
↳ [TerrainPointPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPointPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPointPostgreSQLConverter')  
↳ [UnitPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.UnitPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.UnitPostgreSQLConverter')  
↳ [IGISPostgreSQLConverter&lt;TTableObject&gt;](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLConverter_TTableObject_ 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLConverter\<TTableObject\>')

Implements [DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLConverter](https://learn.microsoft.com/en-us/dotnet/api/digi.postgresql.interfaces.ipostgresqlconverter 'DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLConverter'), [DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLObject](https://learn.microsoft.com/en-us/dotnet/api/digi.postgresql.interfaces.ipostgresqlobject 'DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLObject'), [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject'), [IGISPostgreSQLObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject')

<a name='DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLConverter_TTableObject_'></a>

## IGISPostgreSQLConverter\<TTableObject\> Interface

Defines the contract for a generic GIS\-specific PostgreSQL converter for a specific table object type\.

```csharp
public interface IGISPostgreSQLConverter<TTableObject> : DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLConverter, DiGi.PostgreSQL.Interfaces.IPostgreSQLConverter, DiGi.PostgreSQL.Interfaces.IPostgreSQLObject, DiGi.Core.Interfaces.IObject, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject
    where TTableObject : DiGi.GIS.PostgreSQL.Interfaces.ITableObject
```
#### Type parameters

<a name='DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLConverter_TTableObject_.TTableObject'></a>

`TTableObject`

The type of the table object that this converter handles\.

Derived  
↳ [AdministrativeAreal2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter')  
↳ [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')  
↳ [BuildingPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingPostgreSQLConverter')  
↳ [OrtoDatasPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter')  
↳ [ReferencedObjectPostgreSQLConverter&lt;TReferencedObject,TUniqueObject&gt;](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.ReferencedObjectPostgreSQLConverter_TReferencedObject,TUniqueObject_ 'DiGi\.GIS\.PostgreSQL\.Classes\.ReferencedObjectPostgreSQLConverter\<TReferencedObject,TUniqueObject\>')  
↳ [TerrainPointPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPointPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPointPostgreSQLConverter')

Implements [IGISPostgreSQLConverter](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLConverter'), [DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLConverter](https://learn.microsoft.com/en-us/dotnet/api/digi.postgresql.interfaces.ipostgresqlconverter 'DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLConverter'), [DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLObject](https://learn.microsoft.com/en-us/dotnet/api/digi.postgresql.interfaces.ipostgresqlobject 'DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLObject'), [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject'), [IGISPostgreSQLObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject')

<a name='DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject'></a>

## IGISPostgreSQLObject Interface

Defines the contract for a Geographic Information System \(GIS\) object that can be persisted in a PostgreSQL database\.

```csharp
public interface IGISPostgreSQLObject : DiGi.Core.Interfaces.IObject
```

Derived  
↳ [AdministrativeAreal2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter')  
↳ [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')  
↳ [BuildingDataPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataPostgreSQLConverter')  
↳ [BuildingPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingPostgreSQLConverter')  
↳ [BuildingSolarRadiationPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingSolarRadiationPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingSolarRadiationPostgreSQLConverter')  
↳ [EPWFilePostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.EPWFilePostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.EPWFilePostgreSQLConverter')  
↳ [OrtoDatasPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter')  
↳ [PostgreSQLAdministrativeAreal2DCreateDatabaseTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLAdministrativeAreal2DCreateDatabaseTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLAdministrativeAreal2DCreateDatabaseTask')  
↳ [PostgreSQLAdministrativeAreal2DCreateTableTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLAdministrativeAreal2DCreateTableTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLAdministrativeAreal2DCreateTableTask')  
↳ [PostgreSQLAdministrativeAreal2DRefreshTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLAdministrativeAreal2DRefreshTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLAdministrativeAreal2DRefreshTask')  
↳ [PostgreSQLBuilding2DCreateTableTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLBuilding2DCreateTableTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLBuilding2DCreateTableTask')  
↳ [PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask')  
↳ [PostgreSQLBuilding2DRefreshTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLBuilding2DRefreshTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLBuilding2DRefreshTask')  
↳ [PostgreSQLBuildingDataExternalComponentsUpdateTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLBuildingDataExternalComponentsUpdateTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLBuildingDataExternalComponentsUpdateTask')  
↳ [PostgreSQLBuildingDataUpdateTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLBuildingDataUpdateTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLBuildingDataUpdateTask')  
↳ [PostgreSQLOrtoDatasCreateDatabaseTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasCreateDatabaseTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLOrtoDatasCreateDatabaseTask')  
↳ [PostgreSQLOrtoDatasRefreshTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLOrtoDatasRefreshTask')  
↳ [PostgreSQLStatisticalDataCollectionCreateTableTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLStatisticalDataCollectionCreateTableTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLStatisticalDataCollectionCreateTableTask')  
↳ [PostgreSQLStatisticalDataCollectionPopulateTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLStatisticalDataCollectionPopulateTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLStatisticalDataCollectionPopulateTask')  
↳ [PostgreSQLTerrainPointCreateTableTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLTerrainPointCreateTableTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLTerrainPointCreateTableTask')  
↳ [PostgreSQLTerrainPointFillGapsTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLTerrainPointFillGapsTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLTerrainPointFillGapsTask')  
↳ [PostgreSQLUnitCreateTableTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLUnitCreateTableTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLUnitCreateTableTask')  
↳ [PostgreSQLUnitInsertFromFileTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLUnitInsertFromFileTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLUnitInsertFromFileTask')  
↳ [PostgreSQLUnitPopulateTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLUnitPopulateTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLUnitPopulateTask')  
↳ [PostgreSQLUpdateOccupancyTask](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLUpdateOccupancyTask 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLUpdateOccupancyTask')  
↳ [ReferencedObjectPostgreSQLConverter&lt;TReferencedObject,TUniqueObject&gt;](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.ReferencedObjectPostgreSQLConverter_TReferencedObject,TUniqueObject_ 'DiGi\.GIS\.PostgreSQL\.Classes\.ReferencedObjectPostgreSQLConverter\<TReferencedObject,TUniqueObject\>')  
↳ [StatisticalDataCollectionPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.StatisticalDataCollectionPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.StatisticalDataCollectionPostgreSQLConverter')  
↳ [TableSerializableObject&lt;TSerializableObject&gt;](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Classes\.TableSerializableObject\<TSerializableObject\>')  
↳ [TerrainPoint](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPoint 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPoint')  
↳ [TerrainPointPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPointPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPointPostgreSQLConverter')  
↳ [UnitPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.UnitPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.UnitPostgreSQLConverter')  
↳ [IGISPostgreSQLConverter](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLConverter')  
↳ [IGISPostgreSQLConverter&lt;TTableObject&gt;](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLConverter_TTableObject_ 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLConverter\<TTableObject\>')  
↳ [ITableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableObject')  
↳ [ITableSerializableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject')  
↳ [ITableSerializableObject&lt;TSerializableObject&gt;](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>')

Implements [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject')

<a name='DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLSerializableObject'></a>

## IGISPostgreSQLSerializableObject Interface

Defines the contract for objects that are serializable and compatible with PostgreSQL GIS storage\.

```csharp
public interface IGISPostgreSQLSerializableObject : DiGi.Core.Interfaces.ISerializableObject, DiGi.Core.Interfaces.ICloneableObject<DiGi.Core.Interfaces.ISerializableObject>, DiGi.Core.Interfaces.ICloneableObject, DiGi.Core.Interfaces.IObject
```

Derived  
↳ [AdministrativeAreal2DReferencePath](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReferencePath')  
↳ [Building2DCountyPartMismatchResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DCountyPartMismatchResult 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCountyPartMismatchResult')  
↳ [Building2DReferencedObjectCountyPartMismatchResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DReferencedObjectCountyPartMismatchResult 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DReferencedObjectCountyPartMismatchResult')  
↳ [Building2DReferencedObjectStrayResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DReferencedObjectStrayResult 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DReferencedObjectStrayResult')  
↳ [Building2DReferenceDuplicate](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DReferenceDuplicate 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DReferenceDuplicate')  
↳ [Building2DReferenceUniquenessSummary](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DReferenceUniquenessSummary 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DReferenceUniquenessSummary')  
↳ [BuildingDataCoverageResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingDataCoverageResult 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataCoverageResult')  
↳ [BuildingDataYearBuiltUpdateResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingDataYearBuiltUpdateResult 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataYearBuiltUpdateResult')  
↳ [ExternalComponentsAreaResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.ExternalComponentsAreaResult 'DiGi\.GIS\.PostgreSQL\.Classes\.ExternalComponentsAreaResult')  
↳ [OrtoDatasCountyResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasCountyResult 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasCountyResult')  
↳ [OrtoDatasCoverageResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasCoverageResult 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasCoverageResult')  
↳ [OrtoDatasQueueResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasQueueResult 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasQueueResult')  
↳ [OrtoDatasSubdivisionResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasSubdivisionResult 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasSubdivisionResult')  
↳ [PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult')  
↳ [PostgreSQLBuilding2DRefreshResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLBuilding2DRefreshResult 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLBuilding2DRefreshResult')  
↳ [PostgreSQLOrtoDatasRefreshResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshResult 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLOrtoDatasRefreshResult')  
↳ [PostgreSQLUpdateResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLUpdateResult 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLUpdateResult')  
↳ [PredictedYearBuiltRemoveResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PredictedYearBuiltRemoveResult 'DiGi\.GIS\.PostgreSQL\.Classes\.PredictedYearBuiltRemoveResult')  
↳ [PredictedYearBuiltRunResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PredictedYearBuiltRunResult 'DiGi\.GIS\.PostgreSQL\.Classes\.PredictedYearBuiltRunResult')  
↳ [Rejection](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Rejection 'DiGi\.GIS\.PostgreSQL\.Classes\.Rejection')  
↳ [TableSerializableObject&lt;TSerializableObject&gt;](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Classes\.TableSerializableObject\<TSerializableObject\>')  
↳ [TerrainPoint](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPoint 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPoint')  
↳ [TerrainPointCountyResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPointCountyResult 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPointCountyResult')  
↳ [TerrainPointCoverageResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPointCoverageResult 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPointCoverageResult')  
↳ [TerrainPointDensityResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPointDensityResult 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPointDensityResult')  
↳ [TerrainPointUpdateResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPointUpdateResult 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPointUpdateResult')  
↳ [UnitComplianceResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.UnitComplianceResult 'DiGi\.GIS\.PostgreSQL\.Classes\.UnitComplianceResult')  
↳ [UserYearBuiltRemoveResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.UserYearBuiltRemoveResult 'DiGi\.GIS\.PostgreSQL\.Classes\.UserYearBuiltRemoveResult')  
↳ [YearBuiltDataRemoveResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.YearBuiltDataRemoveResult 'DiGi\.GIS\.PostgreSQL\.Classes\.YearBuiltDataRemoveResult')  
↳ [ITableSerializableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject')  
↳ [ITableSerializableObject&lt;TSerializableObject&gt;](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>')

Implements [DiGi\.Core\.Interfaces\.ISerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iserializableobject 'DiGi\.Core\.Interfaces\.ISerializableObject'), [DiGi\.Core\.Interfaces\.ICloneableObject&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject-1 'DiGi\.Core\.Interfaces\.ICloneableObject\`1')[DiGi\.Core\.Interfaces\.ISerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iserializableobject 'DiGi\.Core\.Interfaces\.ISerializableObject')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject-1 'DiGi\.Core\.Interfaces\.ICloneableObject\`1'), [DiGi\.Core\.Interfaces\.ICloneableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject 'DiGi\.Core\.Interfaces\.ICloneableObject'), [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject')

<a name='DiGi.GIS.PostgreSQL.Interfaces.ITableObject'></a>

## ITableObject Interface

Defines the contract for an object that can be represented as a table record within a PostgreSQL GIS database\.

```csharp
public interface ITableObject : DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject, DiGi.Core.Interfaces.IObject
```

Derived  
↳ [TableSerializableObject&lt;TSerializableObject&gt;](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Classes\.TableSerializableObject\<TSerializableObject\>')  
↳ [TerrainPoint](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPoint 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPoint')  
↳ [ITableSerializableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject')  
↳ [ITableSerializableObject&lt;TSerializableObject&gt;](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>')

Implements [IGISPostgreSQLObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject'), [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject')

<a name='DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject'></a>

## ITableSerializableObject Interface

Defines an object that can be serialized as part of a database table\.

```csharp
public interface ITableSerializableObject : DiGi.GIS.PostgreSQL.Interfaces.ITableObject, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject, DiGi.Core.Interfaces.IObject, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLSerializableObject, DiGi.Core.Interfaces.ISerializableObject, DiGi.Core.Interfaces.ICloneableObject<DiGi.Core.Interfaces.ISerializableObject>, DiGi.Core.Interfaces.ICloneableObject
```

Derived  
↳ [TableSerializableObject&lt;TSerializableObject&gt;](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Classes\.TableSerializableObject\<TSerializableObject\>')  
↳ [ITableSerializableObject&lt;TSerializableObject&gt;](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>')

Implements [ITableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableObject'), [IGISPostgreSQLObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject'), [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject'), [IGISPostgreSQLSerializableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLSerializableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLSerializableObject'), [DiGi\.Core\.Interfaces\.ISerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iserializableobject 'DiGi\.Core\.Interfaces\.ISerializableObject'), [DiGi\.Core\.Interfaces\.ICloneableObject&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject-1 'DiGi\.Core\.Interfaces\.ICloneableObject\`1')[DiGi\.Core\.Interfaces\.ISerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iserializableobject 'DiGi\.Core\.Interfaces\.ISerializableObject')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject-1 'DiGi\.Core\.Interfaces\.ICloneableObject\`1'), [DiGi\.Core\.Interfaces\.ICloneableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject 'DiGi\.Core\.Interfaces\.ICloneableObject')
### Properties

<a name='DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject.Object'></a>

## ITableSerializableObject\.Object Property

Gets or sets the JSON representation of the object\.

```csharp
System.Text.Json.Nodes.JsonObject? Object { get; set; }
```

#### Property Value
[System\.Text\.Json\.Nodes\.JsonObject](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonobject 'System\.Text\.Json\.Nodes\.JsonObject')

<a name='DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_'></a>

## ITableSerializableObject\<TSerializableObject\> Interface

Defines a generic object that can be serialized as part of a database table and converted to a specific serializable object type\.

```csharp
public interface ITableSerializableObject<TSerializableObject> : DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject, DiGi.GIS.PostgreSQL.Interfaces.ITableObject, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject, DiGi.Core.Interfaces.IObject, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLSerializableObject, DiGi.Core.Interfaces.ISerializableObject, DiGi.Core.Interfaces.ICloneableObject<DiGi.Core.Interfaces.ISerializableObject>, DiGi.Core.Interfaces.ICloneableObject
    where TSerializableObject : DiGi.Core.Interfaces.ISerializableObject
```
#### Type parameters

<a name='DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_.TSerializableObject'></a>

`TSerializableObject`

The type of the serializable object this interface converts to\.

Derived  
↳ [TableSerializableObject&lt;TSerializableObject&gt;](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Classes\.TableSerializableObject\<TSerializableObject\>')

Implements [ITableSerializableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject'), [ITableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableObject'), [IGISPostgreSQLObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject'), [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject'), [IGISPostgreSQLSerializableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLSerializableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLSerializableObject'), [DiGi\.Core\.Interfaces\.ISerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iserializableobject 'DiGi\.Core\.Interfaces\.ISerializableObject'), [DiGi\.Core\.Interfaces\.ICloneableObject&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject-1 'DiGi\.Core\.Interfaces\.ICloneableObject\`1')[DiGi\.Core\.Interfaces\.ISerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iserializableobject 'DiGi\.Core\.Interfaces\.ISerializableObject')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject-1 'DiGi\.Core\.Interfaces\.ICloneableObject\`1'), [DiGi\.Core\.Interfaces\.ICloneableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject 'DiGi\.Core\.Interfaces\.ICloneableObject')
### Methods

<a name='DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_.ToDiGi()'></a>

## ITableSerializableObject\<TSerializableObject\>\.ToDiGi\(\) Method

Converts the current instance to a DiGi serializable object\.

```csharp
TSerializableObject? ToDiGi();
```

#### Returns
[TSerializableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_.TSerializableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>\.TSerializableObject')  
A [TSerializableObject](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_.TSerializableObject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>\.TSerializableObject') representation of the object, or null if conversion fails\.