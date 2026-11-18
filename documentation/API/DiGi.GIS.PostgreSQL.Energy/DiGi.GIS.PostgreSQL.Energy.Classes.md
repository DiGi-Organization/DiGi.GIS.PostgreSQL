#### [DiGi\.GIS\.PostgreSQL\.Energy](DiGi.GIS.PostgreSQL.Energy.Overview.md 'DiGi\.GIS\.PostgreSQL\.Energy\.Overview')

## DiGi\.GIS\.PostgreSQL\.Energy\.Classes Namespace
### Classes

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult'></a>

## BuildingHeatDemandCountyResult Class

Represents what one county part holds in [BuildingHeatDemand](DiGi.GIS.PostgreSQL.Energy.Constants.md#DiGi.GIS.PostgreSQL.Energy.Constants.TableName.BuildingHeatDemand 'DiGi\.GIS\.PostgreSQL\.Energy\.Constants\.TableName\.BuildingHeatDemand') for one method version \(DiGi\.GIS\.PostgreSQL\#143\): how many buildings, their total and mean energy need for space heating, and when they were written\.

```csharp
public class BuildingHeatDemandCountyResult : DiGi.Core.Classes.SerializableResult, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLSerializableObject, DiGi.Core.Interfaces.ISerializableObject, DiGi.Core.Interfaces.ICloneableObject<DiGi.Core.Interfaces.ISerializableObject>, DiGi.Core.Interfaces.ICloneableObject, DiGi.Core.Interfaces.IObject
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → [DiGi\.Core\.Classes\.Object](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.object 'DiGi\.Core\.Classes\.Object') → [DiGi\.Core\.Classes\.SerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.serializableobject 'DiGi\.Core\.Classes\.SerializableObject') → [DiGi\.Core\.Classes\.SerializableResult](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.serializableresult 'DiGi\.Core\.Classes\.SerializableResult') → BuildingHeatDemandCountyResult

Implements [DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLSerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.interfaces.igispostgresqlserializableobject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLSerializableObject'), [DiGi\.Core\.Interfaces\.ISerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iserializableobject 'DiGi\.Core\.Interfaces\.ISerializableObject'), [DiGi\.Core\.Interfaces\.ICloneableObject&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject-1 'DiGi\.Core\.Interfaces\.ICloneableObject\`1')[DiGi\.Core\.Interfaces\.ISerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iserializableobject 'DiGi\.Core\.Interfaces\.ISerializableObject')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject-1 'DiGi\.Core\.Interfaces\.ICloneableObject\`1'), [DiGi\.Core\.Interfaces\.ICloneableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.icloneableobject 'DiGi\.Core\.Interfaces\.ICloneableObject'), [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject')
### Constructors

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult)'></a>

## BuildingHeatDemandCountyResult\(BuildingHeatDemandCountyResult\) Constructor

Initializes a new instance of the [BuildingHeatDemandCountyResult](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandCountyResult') class by copying an existing instance\.

```csharp
public BuildingHeatDemandCountyResult(DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult? buildingHeatDemandCountyResult);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult).buildingHeatDemandCountyResult'></a>

`buildingHeatDemandCountyResult` [BuildingHeatDemandCountyResult](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandCountyResult')

The [BuildingHeatDemandCountyResult](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandCountyResult') to copy\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(int,int,long,double,double,System.Nullable_System.DateTimeOffset_,System.Nullable_System.DateTimeOffset_)'></a>

## BuildingHeatDemandCountyResult\(int, int, long, double, double, Nullable\<DateTimeOffset\>, Nullable\<DateTimeOffset\>\) Constructor

Initializes a new instance of the [BuildingHeatDemandCountyResult](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandCountyResult') class\.

```csharp
public BuildingHeatDemandCountyResult(int countyId, int methodVersion, long buildingCount, double heatingEnergyNeed, double heatingEnergyNeedPerArea, System.Nullable<System.DateTimeOffset> firstCreatedAt, System.Nullable<System.DateTimeOffset> lastCreatedAt);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(int,int,long,double,double,System.Nullable_System.DateTimeOffset_,System.Nullable_System.DateTimeOffset_).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(int,int,long,double,double,System.Nullable_System.DateTimeOffset_,System.Nullable_System.DateTimeOffset_).methodVersion'></a>

`methodVersion` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The method version of the results\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(int,int,long,double,double,System.Nullable_System.DateTimeOffset_,System.Nullable_System.DateTimeOffset_).buildingCount'></a>

`buildingCount` [System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

The number of buildings with a stored result\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(int,int,long,double,double,System.Nullable_System.DateTimeOffset_,System.Nullable_System.DateTimeOffset_).heatingEnergyNeed'></a>

`heatingEnergyNeed` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The total annual energy need for space heating of those buildings \[kWh/a\]\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(int,int,long,double,double,System.Nullable_System.DateTimeOffset_,System.Nullable_System.DateTimeOffset_).heatingEnergyNeedPerArea'></a>

`heatingEnergyNeedPerArea` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The mean annual energy need for space heating per reference floor area of those buildings \[kWh/\(m2a\)\]\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(int,int,long,double,double,System.Nullable_System.DateTimeOffset_,System.Nullable_System.DateTimeOffset_).firstCreatedAt'></a>

`firstCreatedAt` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.DateTimeOffset](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset 'System\.DateTimeOffset')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

When the first of the results was written\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(int,int,long,double,double,System.Nullable_System.DateTimeOffset_,System.Nullable_System.DateTimeOffset_).lastCreatedAt'></a>

`lastCreatedAt` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.DateTimeOffset](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset 'System\.DateTimeOffset')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

When the last of the results was written\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(System.Text.Json.Nodes.JsonObject)'></a>

## BuildingHeatDemandCountyResult\(JsonObject\) Constructor

Initializes a new instance of the [BuildingHeatDemandCountyResult](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandCountyResult') class from a JSON object\.

```csharp
public BuildingHeatDemandCountyResult(System.Text.Json.Nodes.JsonObject? jsonObject);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingHeatDemandCountyResult(System.Text.Json.Nodes.JsonObject).jsonObject'></a>

`jsonObject` [System\.Text\.Json\.Nodes\.JsonObject](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonobject 'System\.Text\.Json\.Nodes\.JsonObject')

The JSON object containing the data\. This value can be null\.
### Properties

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.BuildingCount'></a>

## BuildingHeatDemandCountyResult\.BuildingCount Property

Gets the number of buildings with a stored result\.

```csharp
public long BuildingCount { get; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.CountyId'></a>

## BuildingHeatDemandCountyResult\.CountyId Property

Gets the id of the county part\.

```csharp
public int CountyId { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.FirstCreatedAt'></a>

## BuildingHeatDemandCountyResult\.FirstCreatedAt Property

Gets when the first of the results was written\.

```csharp
public System.Nullable<System.DateTimeOffset> FirstCreatedAt { get; }
```

#### Property Value
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.DateTimeOffset](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset 'System\.DateTimeOffset')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.HeatingEnergyNeed'></a>

## BuildingHeatDemandCountyResult\.HeatingEnergyNeed Property

Gets the total annual energy need for space heating of the buildings \[kWh/a\]\.

```csharp
public double HeatingEnergyNeed { get; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.HeatingEnergyNeedPerArea'></a>

## BuildingHeatDemandCountyResult\.HeatingEnergyNeedPerArea Property

Gets the mean annual energy need for space heating per reference floor area of the buildings \[kWh/\(m2a\)\]\.

```csharp
public double HeatingEnergyNeedPerArea { get; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.LastCreatedAt'></a>

## BuildingHeatDemandCountyResult\.LastCreatedAt Property

Gets when the last of the results was written\.

```csharp
public System.Nullable<System.DateTimeOffset> LastCreatedAt { get; }
```

#### Property Value
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.DateTimeOffset](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset 'System\.DateTimeOffset')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult.MethodVersion'></a>

## BuildingHeatDemandCountyResult\.MethodVersion Property

Gets the method version of the results\.

```csharp
public int MethodVersion { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter'></a>

## BuildingHeatDemandPostgreSQLConverter Class

Stores and reads [DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.energy.classes.buildingheatdemandresult 'DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult') objects in the [BuildingHeatDemand](DiGi.GIS.PostgreSQL.Energy.Constants.md#DiGi.GIS.PostgreSQL.Energy.Constants.TableName.BuildingHeatDemand 'DiGi\.GIS\.PostgreSQL\.Energy\.Constants\.TableName\.BuildingHeatDemand') table: one row per building model and method version, partitioned by county part, on the Storage database beside `building_solar_radiation` \(DiGi\.GIS\.PostgreSQL\#143\)\.

The converter lives in `DiGi.GIS.PostgreSQL.Energy`, which `DiGi.GIS.PostgreSQL` cannot reference, so `Create.GISPostgreSQLConverterManager()` does not register it: [AddBuildingHeatDemandPostgreSQLConverter\(this GISPostgreSQLConverterManager\)](DiGi.GIS.PostgreSQL.Energy.md#DiGi.GIS.PostgreSQL.Energy.Modify.AddBuildingHeatDemandPostgreSQLConverter(thisDiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager) 'DiGi\.GIS\.PostgreSQL\.Energy\.Modify\.AddBuildingHeatDemandPostgreSQLConverter\(this DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager\)') does, on the Storage connection.

```csharp
public class BuildingHeatDemandPostgreSQLConverter : DiGi.PostgreSQL.Classes.PostgreSQLConverter<DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult>, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLConverter, DiGi.PostgreSQL.Interfaces.IPostgreSQLConverter, DiGi.PostgreSQL.Interfaces.IPostgreSQLObject, DiGi.Core.Interfaces.IObject, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → [DiGi\.PostgreSQL\.Classes\.PostgreSQLConverter&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.postgresql.classes.postgresqlconverter-1 'DiGi\.PostgreSQL\.Classes\.PostgreSQLConverter\`1')[DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.energy.classes.buildingheatdemandresult 'DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.postgresql.classes.postgresqlconverter-1 'DiGi\.PostgreSQL\.Classes\.PostgreSQLConverter\`1') → BuildingHeatDemandPostgreSQLConverter

Implements [DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLConverter](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.interfaces.igispostgresqlconverter 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLConverter'), [DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLConverter](https://learn.microsoft.com/en-us/dotnet/api/digi.postgresql.interfaces.ipostgresqlconverter 'DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLConverter'), [DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLObject](https://learn.microsoft.com/en-us/dotnet/api/digi.postgresql.interfaces.ipostgresqlobject 'DiGi\.PostgreSQL\.Interfaces\.IPostgreSQLObject'), [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject'), [DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.interfaces.igispostgresqlobject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject')
### Constructors

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.BuildingHeatDemandPostgreSQLConverter(DiGi.PostgreSQL.Classes.ConnectionData)'></a>

## BuildingHeatDemandPostgreSQLConverter\(ConnectionData\) Constructor

Initializes a new instance of the [BuildingHeatDemandPostgreSQLConverter](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandPostgreSQLConverter') class\.

```csharp
public BuildingHeatDemandPostgreSQLConverter(DiGi.PostgreSQL.Classes.ConnectionData? connectionData);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.BuildingHeatDemandPostgreSQLConverter(DiGi.PostgreSQL.Classes.ConnectionData).connectionData'></a>

`connectionData` [DiGi\.PostgreSQL\.Classes\.ConnectionData](https://learn.microsoft.com/en-us/dotnet/api/digi.postgresql.classes.connectiondata 'DiGi\.PostgreSQL\.Classes\.ConnectionData')

The [DiGi\.PostgreSQL\.Classes\.ConnectionData](https://learn.microsoft.com/en-us/dotnet/api/digi.postgresql.classes.connectiondata 'DiGi\.PostgreSQL\.Classes\.ConnectionData') of the Storage database\. This value can be null\.
### Fields

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.batchSize'></a>

## BuildingHeatDemandPostgreSQLConverter\.batchSize Field

The number of references or results one statement carries\.

```csharp
private const int batchSize = 1000;
```

#### Field Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')
### Properties

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.TableName'></a>

## BuildingHeatDemandPostgreSQLConverter\.TableName Property

Gets the name of the database table associated with stored building heat demand\.

```csharp
public static string TableName { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')
### Methods

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandPostgreSQLConverter\.GetItemsByReferencesAsync\(NpgsqlConnection, IEnumerable\<string\>, int, Nullable\<int\>, bool, int, CancellationToken\) Method

Asynchronously reads the stored heat demand of each of the given references\.

The read is scoped to [countyId](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).countyId 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandPostgreSQLConverter\.GetItemsByReferencesAsync\(Npgsql\.NpgsqlConnection, System\.Collections\.Generic\.IEnumerable\<string\>, int, System\.Nullable\<int\>, bool, int, System\.Threading\.CancellationToken\)\.countyId') first. With [fallbackByReference](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).fallbackByReference 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandPostgreSQLConverter\.GetItemsByReferencesAsync\(Npgsql\.NpgsqlConnection, System\.Collections\.Generic\.IEnumerable\<string\>, int, System\.Nullable\<int\>, bool, int, System\.Threading\.CancellationToken\)\.fallbackByReference'), a reference not found there is looked for under every county part, because a building of a multi-part county may be filed under a sibling part (`Coding - GIS Administrative Data.md`); the scoped and the unscoped read are separate statements, so the scoped one prunes to its partition. Of several results of one reference the highest method version wins, a tie broken by the newest. References are read 1000 at a time.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult>?> GetItemsByReferencesAsync(Npgsql.NpgsqlConnection? npgsqlConnection, System.Collections.Generic.IEnumerable<string>? references, int countyId, System.Nullable<int> methodVersion=null, bool fallbackByReference=false, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The open [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') of the Storage database\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The references to read\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The county part to scope the read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).methodVersion'></a>

`methodVersion` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The method version to read; null for the highest stored\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).fallbackByReference'></a>

`fallbackByReference` [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

True to look for a reference not found under [countyId](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).countyId 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandPostgreSQLConverter\.GetItemsByReferencesAsync\(Npgsql\.NpgsqlConnection, System\.Collections\.Generic\.IEnumerable\<string\>, int, System\.Nullable\<int\>, bool, int, System\.Threading\.CancellationToken\)\.countyId') under every county part\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.energy.classes.buildingheatdemandresult 'DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds one result per reference found, or null when the connection is null or the table is missing\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandPostgreSQLConverter\.GetItemsByReferencesAsync\(IEnumerable\<string\>, int, Nullable\<int\>, bool, int, CancellationToken\) Method

Asynchronously reads the stored heat demand of each of the given references, automatically managing the connection\.

```csharp
public System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult>?> GetItemsByReferencesAsync(System.Collections.Generic.IEnumerable<string>? references, int countyId, System.Nullable<int> methodVersion=null, bool fallbackByReference=false, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The references to read\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The county part to scope the read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).methodVersion'></a>

`methodVersion` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The method version to read; null for the highest stored\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).fallbackByReference'></a>

`fallbackByReference` [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

True to look for a reference not found under [countyId](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).countyId 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandPostgreSQLConverter\.GetItemsByReferencesAsync\(System\.Collections\.Generic\.IEnumerable\<string\>, int, System\.Nullable\<int\>, bool, int, System\.Threading\.CancellationToken\)\.countyId') under every county part\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetItemsByReferencesAsync(System.Collections.Generic.IEnumerable_string_,int,System.Nullable_int_,bool,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.energy.classes.buildingheatdemandresult 'DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds one result per reference found, or null when the connection cannot be made or the table is missing\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(int,System.Nullable_int_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandPostgreSQLConverter\.GetReferencesAsync\(int, Nullable\<int\>, int, CancellationToken\) Method

Asynchronously reads the references that carry a stored heat demand in the given county part, automatically managing the connection\.

```csharp
public System.Threading.Tasks.Task<System.Collections.Generic.HashSet<string>?> GetReferencesAsync(int countyId, System.Nullable<int> methodVersion=null, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(int,System.Nullable_int_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The county part to scope the read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(int,System.Nullable_int_,int,System.Threading.CancellationToken).methodVersion'></a>

`methodVersion` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The method version; null for any version\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(int,System.Nullable_int_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(int,System.Nullable_int_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the references; empty when the table is missing, or null when the connection cannot be made\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(Npgsql.NpgsqlConnection,int,System.Nullable_int_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandPostgreSQLConverter\.GetReferencesAsync\(NpgsqlConnection, int, Nullable\<int\>, int, CancellationToken\) Method

Asynchronously reads the references that carry a stored heat demand in the given county part \- what a resumed run steps over\.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.HashSet<string>?> GetReferencesAsync(Npgsql.NpgsqlConnection? npgsqlConnection, int countyId, System.Nullable<int> methodVersion=null, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(Npgsql.NpgsqlConnection,int,System.Nullable_int_,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The open [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') of the Storage database\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(Npgsql.NpgsqlConnection,int,System.Nullable_int_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The county part to scope the read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(Npgsql.NpgsqlConnection,int,System.Nullable_int_,int,System.Threading.CancellationToken).methodVersion'></a>

`methodVersion` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The method version; null for any version\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(Npgsql.NpgsqlConnection,int,System.Nullable_int_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetReferencesAsync(Npgsql.NpgsqlConnection,int,System.Nullable_int_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the references; empty when the table is missing, or null when the connection is null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetSummariesByCountyIdsAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandPostgreSQLConverter\.GetSummariesByCountyIdsAsync\(NpgsqlConnection, IEnumerable\<int\>, int, CancellationToken\) Method

Asynchronously summarises what each of the given county parts holds per method version: the number of buildings, their total and mean energy need for space heating, and when they were written\.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult>?> GetSummariesByCountyIdsAsync(Npgsql.NpgsqlConnection? npgsqlConnection, System.Collections.Generic.IEnumerable<int>? countyIds, int commandTimeout=600, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetSummariesByCountyIdsAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The open [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') of the Storage database\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetSummariesByCountyIdsAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The county parts to summarise; null for every part that holds results\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetSummariesByCountyIdsAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetSummariesByCountyIdsAsync(Npgsql.NpgsqlConnection,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[BuildingHeatDemandCountyResult](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandCountyResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds one summary per county part and method version, ordered by both; empty when the table is missing, or null when the connection is null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetSummariesByCountyIdsAsync(System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandPostgreSQLConverter\.GetSummariesByCountyIdsAsync\(IEnumerable\<int\>, int, CancellationToken\) Method

Asynchronously summarises what each of the given county parts holds per method version, automatically managing the connection\.

```csharp
public System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult>?> GetSummariesByCountyIdsAsync(System.Collections.Generic.IEnumerable<int>? countyIds, int commandTimeout=600, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetSummariesByCountyIdsAsync(System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The county parts to summarise; null for every part that holds results\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetSummariesByCountyIdsAsync(System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetSummariesByCountyIdsAsync(System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[BuildingHeatDemandCountyResult](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandCountyResult 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandCountyResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds one summary per county part and method version, or null when the connection cannot be made\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandPostgreSQLConverter\.UpdateAsync\(int, IEnumerable\<BuildingHeatDemandResult\>, int, CancellationToken\) Method

Asynchronously stores the heat demand of several buildings of one county part, automatically managing the connection\.

```csharp
public System.Threading.Tasks.Task<System.Nullable<int>> UpdateAsync(int countyId, System.Collections.Generic.IEnumerable<DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult?>? buildingHeatDemandResults, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The county part the buildings are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).buildingHeatDemandResults'></a>

`buildingHeatDemandResults` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.energy.classes.buildingheatdemandresult 'DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The results to store\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result is the number of rows written, or null when nothing could be written\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection,int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandPostgreSQLConverter\.UpdateAsync\(NpgsqlConnection, int, IEnumerable\<BuildingHeatDemandResult\>, int, CancellationToken\) Method

Asynchronously stores the heat demand of several buildings of one county part, replacing the rows stored for the same keys\.

The key is `(county_id, building_model_id, method_version)`; a write for an existing key replaces every other column. The results are written 1000 at a time, one statement per batch, inside one transaction: a batch that fails rolls the whole write back. A result without a reference is stepped over; of two results with one key, the last wins. [countyId](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection,int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).countyId 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandPostgreSQLConverter\.UpdateAsync\(Npgsql\.NpgsqlConnection, int, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult\>, int, System\.Threading\.CancellationToken\)\.countyId') is written as given whatever the result's own county says. The table and the partition of [countyId](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection,int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).countyId 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandPostgreSQLConverter\.UpdateAsync\(Npgsql\.NpgsqlConnection, int, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult\>, int, System\.Threading\.CancellationToken\)\.countyId') are created when missing.

```csharp
public static System.Threading.Tasks.Task<System.Nullable<int>> UpdateAsync(Npgsql.NpgsqlConnection? npgsqlConnection, int countyId, System.Collections.Generic.IEnumerable<DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult?>? buildingHeatDemandResults, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection,int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The open [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') of the Storage database\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection,int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The county part the buildings are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection,int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).buildingHeatDemandResults'></a>

`buildingHeatDemandResults` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.energy.classes.buildingheatdemandresult 'DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The results to store\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection,int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection,int,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result is the number of rows written, 0 when there was nothing to write, or null when the connection is null or the table or partition could not be created\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask'></a>

## BuildingHeatDemandSolveTask Class

Represents a background task that calculates the monthly heat demand of every building of a county part and stores it, whatever the inputs are read from and the results written to \(DiGi\.GIS\.PostgreSQL\#143, for DiGi\-Organization/DiGi\.Analytical\#10\)\.

Every read and the write is a protected hook: [PostgreSQLBuildingHeatDemandSolveTask](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveTask') implements them with the database converters, a test or a client implements them otherwise. The walk, the skip of stored results, the tiling, the batching, the parallel calculation, the counting and the logging are this class's alone, as in `BuildingSolarRadiationSolveTask`.

A county part is walked by its `id` - every part of a county on its own, never deduplicated (`Coding - GIS Administrative Data.md`) - in square tiles of [TileSize](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.TileSize 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveOptions\.TileSize') by the centroid of each building (`Solar.Query.SolarSolveTiles`), at most [BatchSize](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.BatchSize 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveOptions\.BatchSize') buildings at a time. A batch reads its models, 2D buildings, year built, occupancy, address points and stored solar results, and every neighbour within [NeighbourRadius](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.NeighbourRadius 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveOptions\.NeighbourRadius') of any of them in one read; it calculates its buildings in parallel and writes their results in one call.

<b>Per building</b>, `DiGi.GIS.Analytical.Energy.Create.BuildingHeatDemandResult` splits the party walls, places the openings, assigns the internal conditions, assembles the monthly climate and runs the engine. The climate zone comes from the climate-zone map read once per run; the weather is the EPW of the building's stored solar result, else of the nearest station; the solar result is the one of [SolarRadius](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.SolarRadius 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveOptions\.SolarRadius'), else the latest of any radius, else none (the irradiation is then unshaded and the building counted in [UnshadedBuildingCount](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.UnshadedBuildingCount 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandSolveTask\.UnshadedBuildingCount')). The construction library and the opening-size library are read once per run.

A building that cannot be calculated - no stored model, no 2D building, no location, no weather, or a null result (no opening-size set for it, a month without a valid temperature, no heated space) - is counted in [NotSolvableCount](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.NotSolvableCount 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandSolveTask\.NotSolvableCount') with a reason sample and stepped over. A building whose calculation throws is counted in [FailedCount](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.FailedCount 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandSolveTask\.FailedCount'). A county part whose reads or write fail is counted in [FailedCountyCount](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.FailedCountyCount 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandSolveTask\.FailedCountyCount'), logged with the exception, and the run goes on with the other parts; such a run reports itself as not successful. A requested cancellation is rethrown, never counted.

```csharp
public abstract class BuildingHeatDemandSolveTask : DiGi.Core.Classes.ReportableBackgroundTask<long>
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → [DiGi\.Core\.Classes\.BackgroundTask](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.backgroundtask 'DiGi\.Core\.Classes\.BackgroundTask') → [DiGi\.Core\.Classes\.CancelableBackgroundTask](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.cancelablebackgroundtask 'DiGi\.Core\.Classes\.CancelableBackgroundTask') → [DiGi\.Core\.Classes\.ReportableBackgroundTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.reportablebackgroundtask-1 'DiGi\.Core\.Classes\.ReportableBackgroundTask\`1')[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.reportablebackgroundtask-1 'DiGi\.Core\.Classes\.ReportableBackgroundTask\`1') → BuildingHeatDemandSolveTask

Derived  
↳ [PostgreSQLBuildingHeatDemandSolveTask](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveTask')
### Fields

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.failureSampleCountMax'></a>

## BuildingHeatDemandSolveTask\.failureSampleCountMax Field

The largest number of failure descriptions kept in [FailureSamples](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.FailureSamples 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandSolveTask\.FailureSamples'); the counts are never capped\.

```csharp
private const int failureSampleCountMax = 50;
```

#### Field Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')
### Properties

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.FailedCount'></a>

## BuildingHeatDemandSolveTask\.FailedCount Property

Gets the number of buildings whose calculation threw during the last run\.

```csharp
public long FailedCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.FailedCountyCount'></a>

## BuildingHeatDemandSolveTask\.FailedCountyCount Property

Gets the number of county parts that failed outright and were stepped over during the last run\.

```csharp
public long FailedCountyCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.FailureSamples'></a>

## BuildingHeatDemandSolveTask\.FailureSamples Property

Gets a bounded sample of the failures and of the buildings that could not be calculated during the last run, at most 50 descriptions; the counts are the full counts\.

```csharp
public System.Collections.Generic.IReadOnlyList<string> FailureSamples { get; }
```

#### Property Value
[System\.Collections\.Generic\.IReadOnlyList&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1 'System\.Collections\.Generic\.IReadOnlyList\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1 'System\.Collections\.Generic\.IReadOnlyList\`1')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.NotSolvableCount'></a>

## BuildingHeatDemandSolveTask\.NotSolvableCount Property

Gets the number of buildings that could not be calculated during the last run: no stored model, no 2D building, no location, no weather, or a null result\.

```csharp
public long NotSolvableCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.PostgreSQLBuildingHeatDemandSolveOptions'></a>

## BuildingHeatDemandSolveTask\.PostgreSQLBuildingHeatDemandSolveOptions Property

Gets or sets the options used to configure the run\.

```csharp
public DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions PostgreSQLBuildingHeatDemandSolveOptions { get; set; }
```

#### Property Value
[PostgreSQLBuildingHeatDemandSolveOptions](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveOptions')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ProcessedBuildingCount'></a>

## BuildingHeatDemandSolveTask\.ProcessedBuildingCount Property

Gets the number of buildings considered during the last run, whatever happened to them; the progress reported\.

```csharp
public long ProcessedBuildingCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ProcessedCountyCount'></a>

## BuildingHeatDemandSolveTask\.ProcessedCountyCount Property

Gets the number of county parts processed during the last run\.

```csharp
public long ProcessedCountyCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.SkippedCompletedCount'></a>

## BuildingHeatDemandSolveTask\.SkippedCompletedCount Property

Gets the number of buildings stepped over during the last run because a result of the current method version was already stored \([SkipCompleted](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.SkipCompleted 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveOptions\.SkipCompleted')\)\.

```csharp
public long SkippedCompletedCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.SolvedCount'></a>

## BuildingHeatDemandSolveTask\.SolvedCount Property

Gets the number of buildings calculated and stored during the last run\.

```csharp
public long SolvedCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.UnshadedBuildingCount'></a>

## BuildingHeatDemandSolveTask\.UnshadedBuildingCount Property

Gets the number of stored buildings of the last run whose irradiation is unshaded, because they had no stored solar result\.

```csharp
public long UnshadedBuildingCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')
### Methods

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ExecuteAsync(System.IProgress_long_,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ExecuteAsync\(IProgress\<long\>, CancellationToken\) Method

Executes the background task that calculates and stores the monthly heat demand of every building of the county parts in scope\.

```csharp
protected override System.Threading.Tasks.Task<bool> ExecuteAsync(System.IProgress<long> progress, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ExecuteAsync(System.IProgress_long_,System.Threading.CancellationToken).progress'></a>

`progress` [System\.IProgress&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1 'System\.IProgress\`1')[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1 'System\.IProgress\`1')

A progress reporter for reporting the number of buildings considered\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ExecuteAsync(System.IProgress_long_,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task representing the asynchronous operation\. Returns true when every county part in scope was processed without error; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadAddressPointsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadAddressPointsAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the address points matched to the given references of one county part\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string,DiGi.Geometry.Planar.Classes.Point2D>?> ReadAddressPointsAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadAddressPointsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadAddressPointsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The references to read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadAddressPointsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadAddressPointsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[DiGi\.Geometry\.Planar\.Classes\.Point2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.point2d 'DiGi\.Geometry\.Planar\.Classes\.Point2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the address point of each reference that has one, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadBuilding2DCentroidsAsync\(int, int, CancellationToken\) Method

Reads the centroids of the buildings of one county part, the walk of the part\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Classes.Building2DCentroid>?> ReadBuilding2DCentroidsAsync(int countyId, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.building2dcentroid 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the centroid and reference of every building of the part, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuilding2DsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadBuilding2DsAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the 2D buildings of the given references of one county part\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.Classes.Building2D?>?> ReadBuilding2DsAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuilding2DsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuilding2DsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The `building_2d` references of the batch\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuilding2DsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuilding2DsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Classes\.Building2D](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.building2d 'DiGi\.GIS\.Classes\.Building2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the 2D building of each reference found, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadBuildingModelsAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the stored building models of the given references of one county part\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadBuildingModelsAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The `building_2d` references of the batch\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the latest stored model of each reference found, stamped with its reference, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadBuildingSolarRadiationResultsAsync\(int, List\<string\>, Nullable\<double\>, int, CancellationToken\) Method

Reads the stored solar results of the given references of one county part, with their shading factors\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.Classes.BuildingSolarRadiationResult>?> ReadBuildingSolarRadiationResultsAsync(int countyId, System.Collections.Generic.List<string> references, System.Nullable<double> radius, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The references to read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken).radius'></a>

`radius` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The neighbour radius, in m, exactly as stored; null for the latest result of any radius\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds at most one result per reference, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadCompletedReferencesAsync(int,int,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadCompletedReferencesAsync\(int, int, int, CancellationToken\) Method

Reads the references of one county part that already hold a result of the given method version\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.HashSet<string>?> ReadCompletedReferencesAsync(int countyId, int methodVersion, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadCompletedReferencesAsync(int,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadCompletedReferencesAsync(int,int,int,System.Threading.CancellationToken).methodVersion'></a>

`methodVersion` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The method version\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadCompletedReferencesAsync(int,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadCompletedReferencesAsync(int,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the references, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadCountyReferencesAsync\(int, CancellationToken\) Method

Reads the county parts the run may walk\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference>?> ReadCountyReferencesAsync(int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.administrativeareal2dreference 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds one reference per county part, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadEPWFileAsync\(int, int, CancellationToken\) Method

Reads one EPW file\.

```csharp
protected abstract System.Threading.Tasks.Task<DiGi.EPW.Classes.EPWFile?> ReadEPWFileAsync(int id, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).id'></a>

`id` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the EPW file\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[DiGi\.EPW\.Classes\.EPWFile](https://learn.microsoft.com/en-us/dotnet/api/digi.epw.classes.epwfile 'DiGi\.EPW\.Classes\.EPWFile')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result is the EPW file, or null when it cannot be read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadEPWFileLocationsAsync\(int, CancellationToken\) Method

Reads the location of every EPW station, in the coordinates of the building models\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<(int Id,double X,double Y)>?> ReadEPWFileLocationsAsync(int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the id and position of every station, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadNeighbourBuildingModelsAsync\(Circle2D, int, CancellationToken\) Method

Reads every building model within a circle, the neighbours of a batch\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D circle2D, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).circle2D'></a>

`circle2D` [DiGi\.Geometry\.Planar\.Classes\.Circle2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.circle2d 'DiGi\.Geometry\.Planar\.Classes\.Circle2D')

The circle\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the models, stamped with their references, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadOccupancyDatasAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadOccupancyDatasAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the occupancy estimate of the given references of one county part\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string,DiGi.GIS.Classes.OccupancyData>?> ReadOccupancyDatasAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadOccupancyDatasAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadOccupancyDatasAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The references to read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadOccupancyDatasAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadOccupancyDatasAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[DiGi\.GIS\.Classes\.OccupancyData](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.occupancydata 'DiGi\.GIS\.Classes\.OccupancyData')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the occupancy of each reference that has one, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadYearBuiltsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.ReadYearBuiltsAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the year built of the given references of one county part, with its source: the user's year when there is one, else the predicted year\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string,(System.Nullable<short> YearBuilt,System.Nullable<DiGi.GIS.Enums.YearBuiltSource> YearBuiltSource)>?> ReadYearBuiltsAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadYearBuiltsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadYearBuiltsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The references to read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadYearBuiltsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.ReadYearBuiltsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int16](https://learn.microsoft.com/en-us/dotnet/api/system.int16 'System\.Int16')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[DiGi\.GIS\.Enums\.YearBuiltSource](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.enums.yearbuiltsource 'DiGi\.GIS\.Enums\.YearBuiltSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the year built of each reference that has one, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.TryInitialize()'></a>

## BuildingHeatDemandSolveTask\.TryInitialize\(\) Method

Prepares the transport before the run: resolves its converters or clients\. A refusal logs its own reason\.

```csharp
protected virtual bool TryInitialize();
```

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True when the run can go on; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken)'></a>

## BuildingHeatDemandSolveTask\.WriteAsync\(int, List\<BuildingHeatDemandResult\>, int, CancellationToken\) Method

Writes the results of one batch: the rows of `building_heat_demand` and the heat\-demand columns of `building_data`\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Nullable<int>> WriteAsync(int countyId, System.Collections.Generic.List<DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult> buildingHeatDemandResults, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the buildings are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).buildingHeatDemandResults'></a>

`buildingHeatDemandResults` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.energy.classes.buildingheatdemandresult 'DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The results of the batch\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the write; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result is the number of results written, or null when the write failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions'></a>

## PostgreSQLBuildingHeatDemandSolveOptions Class

Provides the options of the county heat\-demand solve, [BuildingHeatDemandSolveTask](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandSolveTask') \(DiGi\.GIS\.PostgreSQL\#143\)\.

```csharp
public class PostgreSQLBuildingHeatDemandSolveOptions : DiGi.Core.Classes.SerializableOptions
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → [DiGi\.Core\.Classes\.Object](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.object 'DiGi\.Core\.Classes\.Object') → [DiGi\.Core\.Classes\.SerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.serializableobject 'DiGi\.Core\.Classes\.SerializableObject') → [DiGi\.Core\.Classes\.SerializableOptions](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.serializableoptions 'DiGi\.Core\.Classes\.SerializableOptions') → PostgreSQLBuildingHeatDemandSolveOptions
### Constructors

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.PostgreSQLBuildingHeatDemandSolveOptions()'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\(\) Constructor

Initializes a new instance of the [PostgreSQLBuildingHeatDemandSolveOptions](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveOptions') class with the defaults\.

```csharp
public PostgreSQLBuildingHeatDemandSolveOptions();
```

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.PostgreSQLBuildingHeatDemandSolveOptions(DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions)'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\(PostgreSQLBuildingHeatDemandSolveOptions\) Constructor

Initializes a new instance of the [PostgreSQLBuildingHeatDemandSolveOptions](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveOptions') class by copying an existing options instance\.

```csharp
public PostgreSQLBuildingHeatDemandSolveOptions(DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions? postgreSQLBuildingHeatDemandSolveOptions);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.PostgreSQLBuildingHeatDemandSolveOptions(DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions).postgreSQLBuildingHeatDemandSolveOptions'></a>

`postgreSQLBuildingHeatDemandSolveOptions` [PostgreSQLBuildingHeatDemandSolveOptions](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveOptions')

The options to copy\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.PostgreSQLBuildingHeatDemandSolveOptions(System.Text.Json.Nodes.JsonObject)'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\(JsonObject\) Constructor

Initializes a new instance of the [PostgreSQLBuildingHeatDemandSolveOptions](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveOptions') class from a JSON object\.

```csharp
public PostgreSQLBuildingHeatDemandSolveOptions(System.Text.Json.Nodes.JsonObject? jsonObject);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.PostgreSQLBuildingHeatDemandSolveOptions(System.Text.Json.Nodes.JsonObject).jsonObject'></a>

`jsonObject` [System\.Text\.Json\.Nodes\.JsonObject](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonobject 'System\.Text\.Json\.Nodes\.JsonObject')

The JSON object containing the options\. This value can be null\.
### Properties

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.BatchSize'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.BatchSize Property

Gets or sets the number of buildings read, calculated and written together\.

```csharp
public int BatchSize { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.BuildingCountMax'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.BuildingCountMax Property

Gets or sets the number of buildings of each county part after which its walk stops, for a pilot run; null for every building\.

```csharp
public System.Nullable<int> BuildingCountMax { get; set; }
```

#### Property Value
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.CommandTimeout'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.CommandTimeout Property

Gets or sets the timeout in seconds of every database command; 0 disables it\.

```csharp
public int CommandTimeout { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.ConstructionFlavour'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.ConstructionFlavour Property

Gets or sets the construction flavour of the envelope and the openings: the TABULA constructions of the year built, or the regulated limits of its regime\. The typology data is the TypicalAsBuilt set's either way\.

```csharp
public DiGi.Analytical.Building.Regulation.Enums.ConstructionFlavour ConstructionFlavour { get; set; }
```

#### Property Value
[DiGi\.Analytical\.Building\.Regulation\.Enums\.ConstructionFlavour](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.regulation.enums.constructionflavour 'DiGi\.Analytical\.Building\.Regulation\.Enums\.ConstructionFlavour')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.CountyIds'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.CountyIds Property

Gets or sets the ids of the county parts to calculate; null for every part\. A county with several parts is calculated part by part, each under its own id \(`Coding - GIS Administrative Data.md`\)\.

```csharp
public System.Collections.Generic.HashSet<int>? CountyIds { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.HeatDemandOptions'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.HeatDemandOptions Property

Gets or sets the options of the heat\-demand engine; null for its defaults\.

```csharp
public DiGi.Analytical.Building.Energy.Classes.HeatDemandOptions? HeatDemandOptions { get; set; }
```

#### Property Value
[DiGi\.Analytical\.Building\.Energy\.Classes\.HeatDemandOptions](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.energy.classes.heatdemandoptions 'DiGi\.Analytical\.Building\.Energy\.Classes\.HeatDemandOptions')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.MaxDegreeOfParallelism'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.MaxDegreeOfParallelism Property

Gets or sets the largest number of buildings calculated at once; 0 or less for half the processors\.

```csharp
public int MaxDegreeOfParallelism { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.NeighbourRadius'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.NeighbourRadius Property

Gets or sets the radius, in m, around a building within which other buildings are its neighbours: for the party walls \(within 0\.2 m\) and for the fire\-separation and obstruction rules of the opening generation\. The default is [DiGi\.GIS\.Classes\.OpeningGenerationOptions\.ObstructionMaxDistance](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.openinggenerationoptions.obstructionmaxdistance 'DiGi\.GIS\.Classes\.OpeningGenerationOptions\.ObstructionMaxDistance'), the farthest a neighbour counts there\.

```csharp
public double NeighbourRadius { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.SkipCompleted'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.SkipCompleted Property

Gets or sets a value indicating whether a building that already has a result of the current method version is stepped over, so a stopped run resumes where it stopped\.

```csharp
public bool SkipCompleted { get; set; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.SolarRadius'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.SolarRadius Property

Gets or sets the neighbour radius, in m, of the stored solar result the irradiation is taken from\. A building without a result of this radius takes its latest result of any radius, and a building without any result the unshaded irradiation; the radius used is recorded in each result\.

```csharp
public double SolarRadius { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveOptions.TileSize'></a>

## PostgreSQLBuildingHeatDemandSolveOptions\.TileSize Property

Gets or sets the side, in m, of the square tiles a county part is walked in, by the centroid of each building, so the buildings of a batch lie close together and share their neighbours\.

```csharp
public double TileSize { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask'></a>

## PostgreSQLBuildingHeatDemandSolveTask Class

Represents a background task that calculates the monthly heat demand of every building of the county parts in scope and stores it, reading and writing the databases directly through their converters \(DiGi\.GIS\.PostgreSQL\#143\)\.

The run itself - scope, tiling, batching, parallel calculation, counting and logging - is [BuildingHeatDemandSolveTask](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandSolveTask')'s; this class only supplies its reads and its write. The write stores each batch in `building_heat_demand` ([BuildingHeatDemandPostgreSQLConverter](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandPostgreSQLConverter')) and then its four summary columns in `building_data` ([Update\_HeatDemand\(this Table, IEnumerable&lt;BuildingHeatDemandResult&gt;\)](DiGi.GIS.PostgreSQL.Energy.md#DiGi.GIS.PostgreSQL.Energy.Modify.Update_HeatDemand(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_) 'DiGi\.GIS\.PostgreSQL\.Energy\.Modify\.Update\_HeatDemand\(this DiGi\.Core\.IO\.Table\.Classes\.Table, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult\>\)')); both live on the Storage database. The heat-demand converter is registered in the manager here when the host has not ([AddBuildingHeatDemandPostgreSQLConverter\(this GISPostgreSQLConverterManager\)](DiGi.GIS.PostgreSQL.Energy.md#DiGi.GIS.PostgreSQL.Energy.Modify.AddBuildingHeatDemandPostgreSQLConverter(thisDiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager) 'DiGi\.GIS\.PostgreSQL\.Energy\.Modify\.AddBuildingHeatDemandPostgreSQLConverter\(this DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager\)')).

```csharp
public class PostgreSQLBuildingHeatDemandSolveTask : DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject, DiGi.Core.Interfaces.IObject
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → [DiGi\.Core\.Classes\.BackgroundTask](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.backgroundtask 'DiGi\.Core\.Classes\.BackgroundTask') → [DiGi\.Core\.Classes\.CancelableBackgroundTask](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.cancelablebackgroundtask 'DiGi\.Core\.Classes\.CancelableBackgroundTask') → [DiGi\.Core\.Classes\.ReportableBackgroundTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.reportablebackgroundtask-1 'DiGi\.Core\.Classes\.ReportableBackgroundTask\`1')[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.reportablebackgroundtask-1 'DiGi\.Core\.Classes\.ReportableBackgroundTask\`1') → [BuildingHeatDemandSolveTask](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandSolveTask 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandSolveTask') → PostgreSQLBuildingHeatDemandSolveTask

Implements [DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.interfaces.igispostgresqlobject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject'), [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject')
### Constructors

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.PostgreSQLBuildingHeatDemandSolveTask(DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\(GISPostgreSQLConverterManager\) Constructor

Initializes a new instance of the [PostgreSQLBuildingHeatDemandSolveTask](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.PostgreSQLBuildingHeatDemandSolveTask') class\.

```csharp
public PostgreSQLBuildingHeatDemandSolveTask(DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager gISPostgreSQLConverterManager);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.PostgreSQLBuildingHeatDemandSolveTask(DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager).gISPostgreSQLConverterManager'></a>

`gISPostgreSQLConverterManager` [DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.gispostgresqlconvertermanager 'DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager')

The GIS PostgreSQL converter manager used to retrieve converters and execute operations\.
### Fields

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.gISPostgreSQLConverterManager'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.gISPostgreSQLConverterManager Field

The GIS PostgreSQL converter manager used to retrieve converters and execute operations\.

```csharp
protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;
```

#### Field Value
[DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.gispostgresqlconvertermanager 'DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager')
### Methods

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadAddressPointsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadAddressPointsAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the address points matched to the given references of one county part\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string,DiGi.Geometry.Planar.Classes.Point2D>?> ReadAddressPointsAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadAddressPointsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadAddressPointsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The references to read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadAddressPointsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadAddressPointsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[DiGi\.Geometry\.Planar\.Classes\.Point2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.point2d 'DiGi\.Geometry\.Planar\.Classes\.Point2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the address point of each reference that has one, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadBuilding2DCentroidsAsync\(int, int, CancellationToken\) Method

Reads the centroids of the buildings of one county part, the walk of the part\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Classes.Building2DCentroid>?> ReadBuilding2DCentroidsAsync(int countyId, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.building2dcentroid 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the centroid and reference of every building of the part, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuilding2DsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadBuilding2DsAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the 2D buildings of the given references of one county part\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.Classes.Building2D?>?> ReadBuilding2DsAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuilding2DsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuilding2DsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The `building_2d` references of the batch\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuilding2DsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuilding2DsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Classes\.Building2D](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.building2d 'DiGi\.GIS\.Classes\.Building2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the 2D building of each reference found, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadBuildingModelsAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the stored building models of the given references of one county part\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadBuildingModelsAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The `building_2d` references of the batch\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the latest stored model of each reference found, stamped with its reference, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadBuildingSolarRadiationResultsAsync\(int, List\<string\>, Nullable\<double\>, int, CancellationToken\) Method

Reads the stored solar results of the given references of one county part, with their shading factors\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.Classes.BuildingSolarRadiationResult>?> ReadBuildingSolarRadiationResultsAsync(int countyId, System.Collections.Generic.List<string> references, System.Nullable<double> radius, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The references to read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken).radius'></a>

`radius` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The neighbour radius, in m, exactly as stored; null for the latest result of any radius\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadBuildingSolarRadiationResultsAsync(int,System.Collections.Generic.List_string_,System.Nullable_double_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds at most one result per reference, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadCompletedReferencesAsync(int,int,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadCompletedReferencesAsync\(int, int, int, CancellationToken\) Method

Reads the references of one county part that already hold a result of the given method version\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.HashSet<string>?> ReadCompletedReferencesAsync(int countyId, int methodVersion, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadCompletedReferencesAsync(int,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadCompletedReferencesAsync(int,int,int,System.Threading.CancellationToken).methodVersion'></a>

`methodVersion` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The method version\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadCompletedReferencesAsync(int,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadCompletedReferencesAsync(int,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the references, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadCountyReferencesAsync\(int, CancellationToken\) Method

Reads the county parts the run may walk\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference>?> ReadCountyReferencesAsync(int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.administrativeareal2dreference 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds one reference per county part, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadEPWFileAsync\(int, int, CancellationToken\) Method

Reads one EPW file\.

```csharp
protected override System.Threading.Tasks.Task<DiGi.EPW.Classes.EPWFile?> ReadEPWFileAsync(int id, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).id'></a>

`id` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the EPW file\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[DiGi\.EPW\.Classes\.EPWFile](https://learn.microsoft.com/en-us/dotnet/api/digi.epw.classes.epwfile 'DiGi\.EPW\.Classes\.EPWFile')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result is the EPW file, or null when it cannot be read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadEPWFileLocationsAsync\(int, CancellationToken\) Method

Reads the location of every EPW station, in the coordinates of the building models\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<(int Id,double X,double Y)>?> ReadEPWFileLocationsAsync(int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the id and position of every station, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadNeighbourBuildingModelsAsync\(Circle2D, int, CancellationToken\) Method

Reads every building model within a circle, the neighbours of a batch\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D circle2D, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).circle2D'></a>

`circle2D` [DiGi\.Geometry\.Planar\.Classes\.Circle2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.circle2d 'DiGi\.Geometry\.Planar\.Classes\.Circle2D')

The circle\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the models, stamped with their references, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadOccupancyDatasAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadOccupancyDatasAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the occupancy estimate of the given references of one county part\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string,DiGi.GIS.Classes.OccupancyData>?> ReadOccupancyDatasAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadOccupancyDatasAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadOccupancyDatasAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The references to read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadOccupancyDatasAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadOccupancyDatasAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[DiGi\.GIS\.Classes\.OccupancyData](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.occupancydata 'DiGi\.GIS\.Classes\.OccupancyData')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the occupancy of each reference that has one, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadYearBuiltsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.ReadYearBuiltsAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the year built of the given references of one county part, with its source: the user's year when there is one, else the predicted year\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string,(System.Nullable<short> YearBuilt,System.Nullable<DiGi.GIS.Enums.YearBuiltSource> YearBuiltSource)>?> ReadYearBuiltsAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadYearBuiltsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadYearBuiltsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The references to read\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadYearBuiltsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.ReadYearBuiltsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int16](https://learn.microsoft.com/en-us/dotnet/api/system.int16 'System\.Int16')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[DiGi\.GIS\.Enums\.YearBuiltSource](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.enums.yearbuiltsource 'DiGi\.GIS\.Enums\.YearBuiltSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result holds the year built of each reference that has one, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.TryInitialize()'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.TryInitialize\(\) Method

Prepares the transport before the run: resolves its converters or clients\. A refusal logs its own reason\.

```csharp
protected override bool TryInitialize();
```

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True when the run can go on; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingHeatDemandSolveTask\.WriteAsync\(int, List\<BuildingHeatDemandResult\>, int, CancellationToken\) Method

Writes the results of one batch: the rows of `building_heat_demand` and the heat\-demand columns of `building_data`\.

```csharp
protected override System.Threading.Tasks.Task<System.Nullable<int>> WriteAsync(int countyId, System.Collections.Generic.List<DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult> buildingHeatDemandResults, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the buildings are filed under\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).buildingHeatDemandResults'></a>

`buildingHeatDemandResults` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.energy.classes.buildingheatdemandresult 'DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The results of the batch\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the write; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Energy.Classes.PostgreSQLBuildingHeatDemandSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result is the number of results written, or null when the write failed\.