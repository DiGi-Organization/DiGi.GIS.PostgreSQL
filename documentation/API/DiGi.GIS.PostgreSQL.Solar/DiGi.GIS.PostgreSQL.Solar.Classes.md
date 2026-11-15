#### [DiGi\.GIS\.PostgreSQL\.Solar](DiGi.GIS.PostgreSQL.Solar.Overview.md 'DiGi\.GIS\.PostgreSQL\.Solar\.Overview')

## DiGi\.GIS\.PostgreSQL\.Solar\.Classes Namespace
### Classes

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions Class

Provides options for solving the annual solar radiation of every building of a county part, for one or more neighbour radii, and storing it in the building solar radiation table\.

```csharp
public class PostgreSQLBuildingSolarRadiationSolveOptions : DiGi.Core.Classes.SerializableOptions
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → [DiGi\.Core\.Classes\.Object](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.object 'DiGi\.Core\.Classes\.Object') → [DiGi\.Core\.Classes\.SerializableObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.serializableobject 'DiGi\.Core\.Classes\.SerializableObject') → [DiGi\.Core\.Classes\.SerializableOptions](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.serializableoptions 'DiGi\.Core\.Classes\.SerializableOptions') → PostgreSQLBuildingSolarRadiationSolveOptions
### Constructors

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.PostgreSQLBuildingSolarRadiationSolveOptions()'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\(\) Constructor

Initializes a new instance of the [PostgreSQLBuildingSolarRadiationSolveOptions](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions') class\.

```csharp
public PostgreSQLBuildingSolarRadiationSolveOptions();
```

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.PostgreSQLBuildingSolarRadiationSolveOptions(DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions)'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\(PostgreSQLBuildingSolarRadiationSolveOptions\) Constructor

Initializes a new instance of the [PostgreSQLBuildingSolarRadiationSolveOptions](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions') class by copying an existing options instance\.

```csharp
public PostgreSQLBuildingSolarRadiationSolveOptions(DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions postgreSQLBuildingSolarRadiationSolveOptions);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.PostgreSQLBuildingSolarRadiationSolveOptions(DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions).postgreSQLBuildingSolarRadiationSolveOptions'></a>

`postgreSQLBuildingSolarRadiationSolveOptions` [PostgreSQLBuildingSolarRadiationSolveOptions](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions')

The source options instance to copy from\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.PostgreSQLBuildingSolarRadiationSolveOptions(System.Text.Json.Nodes.JsonObject)'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\(JsonObject\) Constructor

Initializes a new instance of the [PostgreSQLBuildingSolarRadiationSolveOptions](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions') class using a JSON object\.

```csharp
public PostgreSQLBuildingSolarRadiationSolveOptions(System.Text.Json.Nodes.JsonObject jsonObject);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.PostgreSQLBuildingSolarRadiationSolveOptions(System.Text.Json.Nodes.JsonObject).jsonObject'></a>

`jsonObject` [System\.Text\.Json\.Nodes\.JsonObject](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonobject 'System\.Text\.Json\.Nodes\.JsonObject')

The JSON object containing the configuration settings\.
### Properties

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.BatchSize'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\.BatchSize Property

Gets or sets the largest number of buildings read, solved and written together\. Values below 1 are treated as 1\.

A tile with more buildings is solved in several batches; each batch reads its neighbours once and writes its results in one statement, so a smaller batch holds less in memory and loses less to an interruption.

```csharp
public int BatchSize { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.CasterTriangleCountMax'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\.CasterTriangleCountMax Property

Gets or sets the largest number of shading\-only triangles \- neighbours and the building's own non\-receiving components \- one solve is run against; above it the building is skipped for that radius and counted\.

The default is the background-job ceiling of the viewer (`SolarJobCasterTriangleCountMax` of DiGi.GIS.WebAPI.UI), measured on its web server. A sensitivity run above 100 m needs a higher one.

```csharp
public int CasterTriangleCountMax { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.CommandTimeout'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\.CommandTimeout Property

Gets or sets the timeout in seconds applied to every statement the run issues\. A value of 0 disables the timeout\.

```csharp
public int CommandTimeout { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.CountyIds'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\.CountyIds Property

Gets or sets the set of county identifiers the run is limited to\. Null solves every county; empty solves none\.

Identifiers rather than codes: a county whose territory is in several pieces is held as one row per piece, each with its own identifier, so a code names several of them (`Coding - GIS Administrative Data.md`).

```csharp
public System.Collections.Generic.HashSet<int>? CountyIds { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.Radii'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\.Radii Property

Gets or sets the neighbour radii, in m, every building is solved for\. Each is rounded to 0\.1 m \(`DiGi.GIS.Analytical.Solar.Query.SolarRadiusKey`\), the value it is stored under; a negative or non\-finite radius is ignored, and null or empty solves nothing\.

0 is the building alone (`SolarRadiusIsolated`): no neighbour casts shade, while the building still shades and blocks the view of its own surfaces - the reference of the urban-context effect (DiGi.GIS.PostgreSQL#107). The viewer solves 50 m by default and at most 100 m.

```csharp
public System.Collections.Generic.List<double>? Radii { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.ReceiverCountMax'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\.ReceiverCountMax Property

Gets or sets the largest number of receiving surfaces \- external walls and roofs \- of a building that is solved; above it the building is skipped and counted\.

The default is the background-job ceiling of the viewer (`SolarJobReceiverCountMax` of DiGi.GIS.WebAPI.UI).

```csharp
public int ReceiverCountMax { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.SkipExisting'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\.SkipExisting Property

Gets or sets a value indicating whether a building and radius that already carry a result of the current solver version are stepped over\.

This resumes an interrupted run, and leaves a result the viewer stored for the same radius in place. Clear it to solve everything again, for instance after the stored buildings changed.

```csharp
public bool SkipExisting { get; set; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.TileSize'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\.TileSize Property

Gets or sets the edge length, in m, of the square tiles a county part is solved in\. Values below 1 are treated as 1\.

Buildings are grouped by the tile their centroid falls in, and the neighbours of a whole tile are read in one query, so a tile trades the number of neighbour reads against the area each one covers.

```csharp
public double TileSize { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask'></a>

## PostgreSQLBuildingSolarRadiationSolveTask Class

Represents a background task that solves the annual solar radiation of every building of a county part for one or more neighbour radii and stores it in the building solar radiation table \(DiGi\.GIS\.PostgreSQL\#136\)\.

Each building is solved exactly as the viewer solves it (DiGi.GIS.WebAPI.UI `SolarController`): the same building model detail level, the same neighbour circle (`DiGi.GIS.Analytical.Solar.Query.SolarNeighbourCircle`), the same nearest EPW station, the same solver options and solver version, and the same stored form - so a result does not depend on which of the two produced it. Radius 0 solves the building alone (`SolarRadiusIsolated`): it still shades and blocks the view of its own surfaces, but no neighbour does.

A county part is solved in square tiles of [TileSize](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.TileSize 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.TileSize'), by the centroid of each building, at most [BatchSize](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.BatchSize 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.BatchSize') buildings at a time: one read of the batch's models, one read of every neighbour any of them can see at the largest radius, and one write of the batch's results. The neighbours of each building and radius are then picked in memory, as `gis/buildingmodel/itemsbycircle` picks them - every model whose bounding box lies within or intersects the building's circle.

A building that cannot be solved - no stored model, no location, no closed envelope, no weather, or above a ceiling - is counted and stepped over. A county part whose buildings, models or neighbours cannot be read, or whose results cannot be written, is failed and logged with the exception, and the run keeps going over the other parts; a run with a failed part reports itself as not succeeded. Batch rows carry no user email: no user asked for them.

```csharp
public class PostgreSQLBuildingSolarRadiationSolveTask : DiGi.Core.Classes.ReportableBackgroundTask<long>, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject, DiGi.Core.Interfaces.IObject
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → [DiGi\.Core\.Classes\.BackgroundTask](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.backgroundtask 'DiGi\.Core\.Classes\.BackgroundTask') → [DiGi\.Core\.Classes\.CancelableBackgroundTask](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.cancelablebackgroundtask 'DiGi\.Core\.Classes\.CancelableBackgroundTask') → [DiGi\.Core\.Classes\.ReportableBackgroundTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.reportablebackgroundtask-1 'DiGi\.Core\.Classes\.ReportableBackgroundTask\`1')[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.reportablebackgroundtask-1 'DiGi\.Core\.Classes\.ReportableBackgroundTask\`1') → PostgreSQLBuildingSolarRadiationSolveTask

Implements [DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.interfaces.igispostgresqlobject 'DiGi\.GIS\.PostgreSQL\.Interfaces\.IGISPostgreSQLObject'), [DiGi\.Core\.Interfaces\.IObject](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iobject 'DiGi\.Core\.Interfaces\.IObject')
### Constructors

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.PostgreSQLBuildingSolarRadiationSolveTask(DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager)'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\(GISPostgreSQLConverterManager\) Constructor

Initializes a new instance of the [PostgreSQLBuildingSolarRadiationSolveTask](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveTask') class\.

```csharp
public PostgreSQLBuildingSolarRadiationSolveTask(DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager gISPostgreSQLConverterManager);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.PostgreSQLBuildingSolarRadiationSolveTask(DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager).gISPostgreSQLConverterManager'></a>

`gISPostgreSQLConverterManager` [DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.gispostgresqlconvertermanager 'DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager')

The GIS PostgreSQL converter manager used to retrieve converters and execute operations\.
### Fields

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.failureSampleCountMax'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.failureSampleCountMax Field

The largest number of failure descriptions kept in [FailureSamples](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.FailureSamples 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveTask\.FailureSamples'); the counts are never capped\.

```csharp
private const int failureSampleCountMax = 50;
```

#### Field Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.gISPostgreSQLConverterManager'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.gISPostgreSQLConverterManager Field

The GIS PostgreSQL converter manager used to retrieve converters and execute operations\.

```csharp
protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;
```

#### Field Value
[DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.gispostgresqlconvertermanager 'DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager')
### Properties

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.FailedCount'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.FailedCount Property

Gets the number of building and radius solves that failed with an exception or produced a result that cannot be stored during the last run\.

```csharp
public long FailedCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.FailedCountyCount'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.FailedCountyCount Property

Gets the number of county parts that failed outright and were stepped over during the last run\.

```csharp
public long FailedCountyCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.FailureSamples'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.FailureSamples Property

Gets a bounded sample of the failures of the last run, at most 50 descriptions; [FailedCount](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.FailedCount 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveTask\.FailedCount') and [FailedCountyCount](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.FailedCountyCount 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveTask\.FailedCountyCount') are the full counts\.

```csharp
public System.Collections.Generic.IReadOnlyList<string> FailureSamples { get; }
```

#### Property Value
[System\.Collections\.Generic\.IReadOnlyList&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1 'System\.Collections\.Generic\.IReadOnlyList\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1 'System\.Collections\.Generic\.IReadOnlyList\`1')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.NotSolvableCount'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.NotSolvableCount Property

Gets the number of building and radius solves stepped over during the last run because the building has no stored model, cannot be located, has no closed external envelope or has no weather\.

```csharp
public long NotSolvableCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.OverCeilingCount'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.OverCeilingCount Property

Gets the number of building and radius solves stepped over during the last run because a ceiling was exceeded: more receiving surfaces than [ReceiverCountMax](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.ReceiverCountMax 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.ReceiverCountMax') \(counted once per radius\) or more caster triangles than [CasterTriangleCountMax](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.CasterTriangleCountMax 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.CasterTriangleCountMax')\.

```csharp
public long OverCeilingCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.PostgreSQLBuildingSolarRadiationSolveOptions'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.PostgreSQLBuildingSolarRadiationSolveOptions Property

Gets or sets the options used to configure the run\.

```csharp
public DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions PostgreSQLBuildingSolarRadiationSolveOptions { get; set; }
```

#### Property Value
[PostgreSQLBuildingSolarRadiationSolveOptions](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ProcessedBuildingCount'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.ProcessedBuildingCount Property

Gets the number of buildings that were considered during the last run, whatever happened to them; the progress reported\.

```csharp
public long ProcessedBuildingCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ProcessedCountyCount'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.ProcessedCountyCount Property

Gets the number of county parts that were processed during the last run\.

```csharp
public long ProcessedCountyCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.SkippedExistingCount'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.SkippedExistingCount Property

Gets the number of building and radius solves stepped over during the last run because a result of the current solver version was already stored \([SkipExisting](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.SkipExisting 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.SkipExisting')\)\.

```csharp
public long SkippedExistingCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.SolvedCount'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.SolvedCount Property

Gets the number of building and radius results solved and stored during the last run\.

```csharp
public long SolvedCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.SolveMilliseconds'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.SolveMilliseconds Property

Gets the total time, in milliseconds, spent building shading models and solving during the last run \- not reading or writing\. Divided by [SolvedCount](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.SolvedCount 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveTask\.SolvedCount') it is the solve time per building and radius the runtime evidence of a run is reported from\.

```csharp
public long SolveMilliseconds { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')
### Methods

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ExecuteAsync(System.IProgress_long_,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.ExecuteAsync\(IProgress\<long\>, CancellationToken\) Method

Executes the background task that solves and stores the annual solar radiation of every building of the county parts in scope\.

```csharp
protected override System.Threading.Tasks.Task<bool> ExecuteAsync(System.IProgress<long> progress, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ExecuteAsync(System.IProgress_long_,System.Threading.CancellationToken).progress'></a>

`progress` [System\.IProgress&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1 'System\.IProgress\`1')[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1 'System\.IProgress\`1')

A progress reporter for reporting the number of buildings considered\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ExecuteAsync(System.IProgress_long_,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task representing the asynchronous operation\. Returns true when every county part in scope was processed without error; otherwise, false\.