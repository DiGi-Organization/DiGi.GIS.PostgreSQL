#### [DiGi\.GIS\.PostgreSQL](DiGi.GIS.PostgreSQL.Overview.md 'DiGi\.GIS\.PostgreSQL\.Overview')

## DiGi\.GIS\.PostgreSQL\.Constants Namespace
### Classes

<a name='DiGi.GIS.PostgreSQL.Constants.FileName'></a>

## FileName Class

Provides constant values for PostgreSQL configuration file names\.

```csharp
public static class FileName
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → FileName
### Fields

<a name='DiGi.GIS.PostgreSQL.Constants.FileName.PostgreSQL_Main'></a>

## FileName\.PostgreSQL\_Main Field

The filename of the main PostgreSQL configuration file\.

```csharp
public const string PostgreSQL_Main = "GIS_PostgreSQL_Main.conf";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.FileName.PostgreSQL_Storage'></a>

## FileName\.PostgreSQL\_Storage Field

The filename of the PostgreSQL storage configuration file\.

```csharp
public const string PostgreSQL_Storage = "GIS_PostgreSQL_Storage.conf";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.PrimarySource'></a>

## PrimarySource Class

Provides the closed vocabulary of the `primary_source` column of the open building footprint tables \(DiGi\.GIS\.PostgreSQL\#125\)\.

Every stored row carries exactly one of these values, normalised from the row's per-object provenance by [PrimarySource\(JsonObject\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.PrimarySource(System.Text.Json.Nodes.JsonObject) 'DiGi\.GIS\.PostgreSQL\.Query\.PrimarySource\(System\.Text\.Json\.Nodes\.JsonObject\)'). The vocabulary feeds figure R1 (the provenance diagram), so the values are part of the published output and must not drift.

```csharp
public static class PrimarySource
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → PrimarySource
### Fields

<a name='DiGi.GIS.PostgreSQL.Constants.PrimarySource.Esri'></a>

## PrimarySource\.Esri Field

The geometry originates in Esri \(an Esri entry of the Overture `sources` array\)\.

```csharp
public const string Esri = "esri";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.PrimarySource.Microsoft'></a>

## PrimarySource\.Microsoft Field

The geometry originates in Microsoft \(the Microsoft reader's quadkey, or a Microsoft entry of the Overture `sources` array\)\.

```csharp
public const string Microsoft = "microsoft";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.PrimarySource.Osm'></a>

## PrimarySource\.Osm Field

The geometry originates in OpenStreetMap \(the OSM reader itself, an OSM\-sourced EUBUCCO or DBSM row, or an OSM entry of the Overture `sources` array\)\.

```csharp
public const string Osm = "osm";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.PrimarySource.Other'></a>

## PrimarySource\.Other Field

The origin does not fall into any of the named categories \(GHS\-OBAT model output, an ESA\-only DBSM row, an unrecognised source entry\), or the row carries no provenance at all\.

```csharp
public const string Other = "other";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.PrimarySource.Register'></a>

## PrimarySource\.Register Field

The geometry originates in an official register or government dataset \(EUBUCCO `gov-*` sources\)\.

```csharp
public const string Register = "register";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName'></a>

## TableName Class

Provides constant values for database table names used in the PostgreSQL GIS system\.

```csharp
public static class TableName
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → TableName
### Fields

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.AddressPoint'></a>

## TableName\.AddressPoint Field

The name of the address point table\. One row is one PRG address point, keyed by its PRG local identifier and filed under the county part it lies in, together with the `building_2d` reference it was matched to\.

```csharp
public const string AddressPoint = "address_point";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.AdministrativeAreal2D'></a>

## TableName\.AdministrativeAreal2D Field

The name of the administrative areal 2D table\.

```csharp
public const string AdministrativeAreal2D = "administrative_areal_2d";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.Building'></a>

## TableName\.Building Field

The name of the building table\.

```csharp
public const string Building = "building";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.Building2D'></a>

## TableName\.Building2D Field

The name of the building 2D table\.

```csharp
public const string Building2D = "building_2d";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.BuildingData'></a>

## TableName\.BuildingData Field

The name of the building data table\.

```csharp
public const string BuildingData = "building_data";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.BuildingDataset'></a>

## TableName\.BuildingDataset Field

The name of the open building dataset release register: one row per release of one open building dataset, carrying its version, download date, licence and SHA\-256 \(DiGi\.GIS\.PostgreSQL\#125\)\. On the Main database, beside [Building2D](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.Building2D 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.Building2D')\.

```csharp
public const string BuildingDataset = "building_dataset";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.BuildingModel'></a>

## TableName\.BuildingModel Field

The name of the building model table\.

```csharp
public const string BuildingModel = "building_model";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.BuildingSolarRadiation'></a>

## TableName\.BuildingSolarRadiation Field

The name of the building solar radiation table\. One row is the stored solar radiation of one building model for one neighbour radius and solver version, partitioned by county part\.

```csharp
public const string BuildingSolarRadiation = "building_solar_radiation";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.DBSM_Building2D'></a>

## TableName\.DBSM\_Building2D Field

The name of the DBSM footprint table: one row per footprint of one DBSM release, partitioned by county part \(DiGi\.GIS\.PostgreSQL\#125\)\. On the Main database, beside [Building2D](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.Building2D 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.Building2D')\.

```csharp
public const string DBSM_Building2D = "dbsm_building_2d";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.EPWFile'></a>

## TableName\.EPWFile Field

The name of the EPW file table\.

```csharp
public const string EPWFile = "epw_file";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.EUBUCCO_Building2D'></a>

## TableName\.EUBUCCO\_Building2D Field

The name of the EUBUCCO footprint table: one row per footprint of one EUBUCCO release, partitioned by county part \(DiGi\.GIS\.PostgreSQL\#125\)\. On the Main database, beside [Building2D](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.Building2D 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.Building2D')\.

```csharp
public const string EUBUCCO_Building2D = "eubucco_building_2d";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.GHS_OBAT_Building'></a>

## TableName\.GHS\_OBAT\_Building Field

The name of the GHS\-OBAT centroid table: one row per centroid of one GHS\-OBAT release, keyed by the Overture identifier it links through and not partitioned \(DiGi\.GIS\.PostgreSQL\#125\)\. On the Main database, beside [Building2D](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.Building2D 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.Building2D')\.

```csharp
public const string GHS_OBAT_Building = "ghs_obat_building";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.Microsoft_Building2D'></a>

## TableName\.Microsoft\_Building2D Field

The name of the Microsoft footprint table: one row per footprint of one Microsoft release, partitioned by county part \(DiGi\.GIS\.PostgreSQL\#125\)\. On the Main database, beside [Building2D](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.Building2D 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.Building2D')\.

```csharp
public const string Microsoft_Building2D = "microsoft_building_2d";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OccupancyData_AdministrativeAreal2D'></a>

## TableName\.OccupancyData\_AdministrativeAreal2D Field

The name of the occupancy data administrative areal 2D table\.

```csharp
public const string OccupancyData_AdministrativeAreal2D = "occupancy_data_administrative_areal_2d";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OccupancyData_Building2D'></a>

## TableName\.OccupancyData\_Building2D Field

The name of the occupancy data building 2D table\.

```csharp
public const string OccupancyData_Building2D = "occupancy_data_building_2d";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OrtoArea'></a>

## TableName\.OrtoArea Field

The name of the full\-interpretation test area table: one row per test area, carrying its bounding box envelope, sampling grid parameters and the sampled cells inside `object` \(DiGi\.GIS\.PostgreSQL\#129\)\. On the Main database\.

```csharp
public const string OrtoArea = "orto_area";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OrtoAreaImage'></a>

## TableName\.OrtoAreaImage Field

The name of the test area image table: one materialised area tile per test area and imagery epoch, negative rows of missing or failed imagery included \(DiGi\.GIS\.PostgreSQL\#129\)\. On the Storage database beside [OrtoDatas](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.OrtoDatas 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.OrtoDatas')\.

```csharp
public const string OrtoAreaImage = "orto_area_image";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OrtoAreaLabel'></a>

## TableName\.OrtoAreaLabel Field

The name of the test area label table: one row per \(area, object identity, epoch, interpreter\), where the object identity is exactly one of the footprint pair or the drawn\-object identifier, unique under a `NULLS NOT DISTINCT` index \(DiGi\.GIS\.PostgreSQL\#129\)\. On the Main database\.

```csharp
public const string OrtoAreaLabel = "orto_area_label";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OrtoAreaObject'></a>

## TableName\.OrtoAreaObject Field

The name of the test area object table: one row per polygon drawn in a test area, keyed by its server\-assigned identifier and filed under `area_key` \(DiGi\.GIS\.PostgreSQL\#129\)\. On the Main database\.

```csharp
public const string OrtoAreaObject = "orto_area_object";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OrtoDataAcquisition'></a>

## TableName\.OrtoDataAcquisition Field

The name of the orthophoto acquisition table: one row per stored image of [OrtoDatas](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.OrtoDatas 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.OrtoDatas'), keyed by county part, building reference and requested year, holding the date, sheet and content hash read from [OrtoSheet](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.OrtoSheet 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.OrtoSheet') \(DiGi\.GIS\.PostgreSQL\#123\)\. On the Storage database, partitioned by county part like [OrtoDatas](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.OrtoDatas 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.OrtoDatas')\.

```csharp
public const string OrtoDataAcquisition = "orto_data_acquisition";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OrtoDatas'></a>

## TableName\.OrtoDatas Field

The name of the ortho\-datas table\.

```csharp
public const string OrtoDatas = "orto_datas";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OrtoDatas_Building2DReference_Update'></a>

## TableName\.OrtoDatas\_Building2DReference\_Update Field

The name of the ortho\-datas building 2D reference update table\.

```csharp
public const string OrtoDatas_Building2DReference_Update = "orto_datas_building_2d_reference_update";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OrtoSheet'></a>

## TableName\.OrtoSheet Field

The name of the orthophoto sheet index table: one row per sheet image of the GUGiK index, on the Storage database beside [OrtoDatas](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.OrtoDatas 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.OrtoDatas')\.

```csharp
public const string OrtoSheet = "orto_sheet";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.OSM_Building2D'></a>

## TableName\.OSM\_Building2D Field

The name of the OpenStreetMap footprint table: one row per footprint of one Geofabrik extract release, partitioned by county part \(DiGi\.GIS\.PostgreSQL\#125\)\. On the Main database, beside [Building2D](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.Building2D 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.Building2D')\.

```csharp
public const string OSM_Building2D = "osm_building_2d";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.Overture_Building2D'></a>

## TableName\.Overture\_Building2D Field

The name of the Overture footprint table: one row per footprint of one Overture release, partitioned by county part \(DiGi\.GIS\.PostgreSQL\#125\)\. On the Main database, beside [Building2D](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.Building2D 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.Building2D')\.

```csharp
public const string Overture_Building2D = "overture_building_2d";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.StatisticalDataCollection'></a>

## TableName\.StatisticalDataCollection Field

The name of the statistical data collection table\.

```csharp
public const string StatisticalDataCollection = "statistical_data_collection";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.TerrainPoint'></a>

## TableName\.TerrainPoint Field

The name of the terrain point table\. One row is a single terrain elevation point\.

```csharp
public const string TerrainPoint = "terrain_point";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.TypologyModel'></a>

## TableName\.TypologyModel Field

The name of the typology model table\.

```csharp
public const string TypologyModel = "typology_model";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.Unit'></a>

## TableName\.Unit Field

The name of the territorial unit table\.

```csharp
public const string Unit = "unit";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.YearBuiltData'></a>

## TableName\.YearBuiltData Field

The name of the year built data table\.

```csharp
public const string YearBuiltData = "year_built_data";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')