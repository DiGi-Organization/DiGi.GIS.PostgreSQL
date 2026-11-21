#### [DiGi\.GIS\.PostgreSQL\.Energy](DiGi.GIS.PostgreSQL.Energy.Overview.md 'DiGi\.GIS\.PostgreSQL\.Energy\.Overview')

## DiGi\.GIS\.PostgreSQL\.Energy Namespace
### Classes

<a name='DiGi.GIS.PostgreSQL.Energy.Create'></a>

## Create Class

```csharp
public static class Create
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → Create
### Methods

<a name='DiGi.GIS.PostgreSQL.Energy.Create.TableAsync_BuildingHeatDemand(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_BuildingHeatDemand\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the [BuildingHeatDemand](DiGi.GIS.PostgreSQL.Energy.Constants.md#DiGi.GIS.PostgreSQL.Energy.Constants.TableName.BuildingHeatDemand 'DiGi\.GIS\.PostgreSQL\.Energy\.Constants\.TableName\.BuildingHeatDemand') table and its indexes, partitioned by county part \(DiGi\.GIS\.PostgreSQL\#143\)\.

One row holds one building's `BuildingHeatDemandResult`: the `object` column carries the whole result, the nested monthly heat demand and the provenance included, and the summary columns (`heating_energy_need`, `heating_energy_need_per_area`, `design_heat_load`) and the provenance keys (weather and solar source, construction flavour and library version) repeat what queries filter and aggregate on.

The key `(county_id, building_model_id, method, method_version, input_variant)` is unique (DiGi-Organization/DiGi.GIS.PostgreSQL#152, Heat Demand Method Identity DiGi-Organization/DiGi.GIS.Analytical#19): `method` and `input_variant` are the integers of the result's `HeatDemandMethod` and `HeatDemandInputVariant`, so results of different methods or input variants are stored side by side. `NULLS NOT DISTINCT` although every column of it is NOT NULL, so the index stays correct if a column ever becomes nullable.

Every statement is idempotent, and the method also migrates a table created before the two columns: they are added with the default 1 (`Monthly`, `Generated` - the only method and input variant there were), which fills every existing row, and the default is dropped so a write must name both; the key moves from `(county_id, building_model_id, method_version)` to the new one. The migration runs only while a column is missing, so the writes that call this method take no table lock once it is done.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_BuildingHeatDemand(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Create.TableAsync_BuildingHeatDemand(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Energy.Create.TableAsync_BuildingHeatDemand(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Energy.Create.TableAsync_BuildingHeatDemand(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table exists afterwards; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Energy.Create.TableAsync_BuildingHeatDemand_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_BuildingHeatDemand\_Partition\(this NpgsqlConnection, int, int, CancellationToken\) Method

Asynchronously creates the partition of the [BuildingHeatDemand](DiGi.GIS.PostgreSQL.Energy.Constants.md#DiGi.GIS.PostgreSQL.Energy.Constants.TableName.BuildingHeatDemand 'DiGi\.GIS\.PostgreSQL\.Energy\.Constants\.TableName\.BuildingHeatDemand') table for one county part \(DiGi\.GIS\.PostgreSQL\#143\)\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_BuildingHeatDemand_Partition(this Npgsql.NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Create.TableAsync_BuildingHeatDemand_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Energy.Create.TableAsync_BuildingHeatDemand_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the partition holds\.

<a name='DiGi.GIS.PostgreSQL.Energy.Create.TableAsync_BuildingHeatDemand_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Energy.Create.TableAsync_BuildingHeatDemand_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the partition exists afterwards; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Energy.Modify'></a>

## Modify Class

```csharp
public static class Modify
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → Modify
### Methods

<a name='DiGi.GIS.PostgreSQL.Energy.Modify.AddBuildingHeatDemandPostgreSQLConverter(thisDiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager)'></a>

## Modify\.AddBuildingHeatDemandPostgreSQLConverter\(this GISPostgreSQLConverterManager\) Method

Registers a [BuildingHeatDemandPostgreSQLConverter](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandPostgreSQLConverter') in a converter manager, on the Storage database \(DiGi\.GIS\.PostgreSQL\#143\)\.

`DiGi.GIS.PostgreSQL` cannot reference this project, so `Create.GISPostgreSQLConverterManager()` does not register the converter. It goes on the connection and configuration file of the registered [DiGi\.GIS\.PostgreSQL\.Classes\.BuildingSolarRadiationPostgreSQLConverter](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.buildingsolarradiationpostgresqlconverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingSolarRadiationPostgreSQLConverter'): `building_heat_demand` lives beside `building_solar_radiation` and `building_data` on the Storage database. A manager that already holds one keeps it.

```csharp
public static DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter? AddBuildingHeatDemandPostgreSQLConverter(this DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager? gISPostgreSQLConverterManager);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Modify.AddBuildingHeatDemandPostgreSQLConverter(thisDiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager).gISPostgreSQLConverterManager'></a>

`gISPostgreSQLConverterManager` [DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.gispostgresqlconvertermanager 'DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager')

The converter manager\. This value can be null\.

#### Returns
[BuildingHeatDemandPostgreSQLConverter](DiGi.GIS.PostgreSQL.Energy.Classes.md#DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Energy\.Classes\.BuildingHeatDemandPostgreSQLConverter')  
The registered converter, or null when the manager is null or has no Storage connection to take\.

<a name='DiGi.GIS.PostgreSQL.Energy.Modify.Update_HeatDemand(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_)'></a>

## Modify\.Update\_HeatDemand\(this Table, IEnumerable\<BuildingHeatDemandResult\>\) Method

Writes the heat\-demand summary of the given results into the six "Heat demand" columns of a `building_data` table, keyed by county identifier and reference \(DiGi\.GIS\.PostgreSQL\#143\): the annual energy need for space heating \[kWh\], the energy need per reference floor area \[kWh/m2\], the design heat load \[W\], the method version, and the integers of the method and the input variant that identify the result \(DiGi\-Organization/DiGi\.GIS\.IO\#21\) \(`GIS.IO.Create.Columns_HeatDemand`\)\.

The first result of a county and reference is the one written and the rest are stepped over, so the results reach this method in the caller's order of preference. A value that is not a finite number is left as a null cell, never a sentinel. A result without a reference or without a heat demand is stepped over.

```csharp
public static int Update_HeatDemand(this DiGi.Core.IO.Table.Classes.Table? table, System.Collections.Generic.IEnumerable<DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult?>? buildingHeatDemandResults);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Energy.Modify.Update_HeatDemand(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_).table'></a>

`table` [DiGi\.Core\.IO\.Table\.Classes\.Table](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.table 'DiGi\.Core\.IO\.Table\.Classes\.Table')

The table to fill\. This value can be null\.

<a name='DiGi.GIS.PostgreSQL.Energy.Modify.Update_HeatDemand(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.Analytical.Energy.Classes.BuildingHeatDemandResult_).buildingHeatDemandResults'></a>

`buildingHeatDemandResults` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.energy.classes.buildingheatdemandresult 'DiGi\.GIS\.Analytical\.Energy\.Classes\.BuildingHeatDemandResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The results, most preferred first\. This value can be null\.

#### Returns
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')  
The number of buildings written; 0 when the table or the results are null\.