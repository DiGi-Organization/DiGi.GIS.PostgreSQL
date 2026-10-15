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

<a name='DiGi.GIS.PostgreSQL.Constants.TableName'></a>

## TableName Class

Provides constant values for database table names used in the PostgreSQL GIS system\.

```csharp
public static class TableName
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → TableName
### Fields

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

<a name='DiGi.GIS.PostgreSQL.Constants.TableName.EPWFile'></a>

## TableName\.EPWFile Field

The name of the EPW file table\.

```csharp
public const string EPWFile = "epw_file";
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