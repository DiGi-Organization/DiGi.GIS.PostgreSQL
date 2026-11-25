#### [DiGi\.GIS\.PostgreSQL\.Solar](DiGi.GIS.PostgreSQL.Solar.Overview.md 'DiGi\.GIS\.PostgreSQL\.Solar\.Overview')

## DiGi\.GIS\.PostgreSQL\.Solar\.Classes Namespace
### Classes

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveBatch'></a>

## BuildingSolarRadiationSolveBatch Class

Holds one batch of the batch solar radiation solve once it has been read: its solve jobs, the neighbours they may see and, in the order a sequential run met them, the buildings stepped over before solving \(DiGi\.GIS\.PostgreSQL\#139\)\.

Read ahead while the previous batch is solved, so it carries what happened instead of counting it: the task aggregates it in order once the batch is solved.

```csharp
internal sealed class BuildingSolarRadiationSolveBatch
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → BuildingSolarRadiationSolveBatch
### Properties

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveBatch.BuildingCount'></a>

## BuildingSolarRadiationSolveBatch\.BuildingCount Property

Gets the number of buildings of the batch\.

```csharp
public int BuildingCount { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveBatch.BuildingSolarRadiationSolveJobs'></a>

## BuildingSolarRadiationSolveBatch\.BuildingSolarRadiationSolveJobs Property

Gets the solve jobs, receiver by receiver and radius by radius\.

```csharp
public System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob> BuildingSolarRadiationSolveJobs { get; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[BuildingSolarRadiationSolveJob](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveJob')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveBatch.Issues'></a>

## BuildingSolarRadiationSolveBatch\.Issues Property

Gets the buildings stepped over while reading, in order\.

```csharp
public System.Collections.Generic.List<(string? Reference,DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType BuildingSolarRadiationSolveOutcomeType,int Count,string? Reason)> Issues { get; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[BuildingSolarRadiationSolveOutcomeType](DiGi.GIS.PostgreSQL.Solar.Enums.md#DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType 'DiGi\.GIS\.PostgreSQL\.Solar\.Enums\.BuildingSolarRadiationSolveOutcomeType')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveBatch.Neighbours'></a>

## BuildingSolarRadiationSolveBatch\.Neighbours Property

Gets the neighbour models any job may see, with their planar bounding boxes\.

```csharp
public System.Collections.Generic.List<(DiGi.Analytical.Building.Classes.BuildingModel BuildingModel,double XMin,double YMin,double XMax,double YMax)> Neighbours { get; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveBatch.Receivers'></a>

## BuildingSolarRadiationSolveBatch\.Receivers Property

Gets the receivers in order, with their number of pending radii and whether a weather file was read for them\.

```csharp
public System.Collections.Generic.List<(string Reference,int Count,bool HasWeather)> Receivers { get; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob'></a>

## BuildingSolarRadiationSolveJob Class

Holds one building and radius of a batch solar radiation solve: the receiving building, its receiver normals, the neighbour radius and the weather it is solved with \(DiGi\.GIS\.PostgreSQL\#139\)\.

The building model is shared by the jobs of its other radii and may be read by several solves at once; a solve only reads it.

```csharp
public class BuildingSolarRadiationSolveJob
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → BuildingSolarRadiationSolveJob
### Constructors

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.BuildingSolarRadiationSolveJob(DiGi.Analytical.Building.Classes.BuildingModel,string,System.Collections.Generic.Dictionary_string,DiGi.Geometry.Spatial.Classes.Vector3D_,double,int,int,DiGi.EPW.Classes.EPWFile)'></a>

## BuildingSolarRadiationSolveJob\(BuildingModel, string, Dictionary\<string,Vector3D\>, double, int, int, EPWFile\) Constructor

Initializes a new instance of the [BuildingSolarRadiationSolveJob](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveJob') class\.

```csharp
public BuildingSolarRadiationSolveJob(DiGi.Analytical.Building.Classes.BuildingModel buildingModel, string reference, System.Collections.Generic.Dictionary<string,DiGi.Geometry.Spatial.Classes.Vector3D> normals, double radius, int countyId, int ePWFileId, DiGi.EPW.Classes.EPWFile ePWFile);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.BuildingSolarRadiationSolveJob(DiGi.Analytical.Building.Classes.BuildingModel,string,System.Collections.Generic.Dictionary_string,DiGi.Geometry.Spatial.Classes.Vector3D_,double,int,int,DiGi.EPW.Classes.EPWFile).buildingModel'></a>

`buildingModel` [DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')

The receiving building model, stamped with its building information\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.BuildingSolarRadiationSolveJob(DiGi.Analytical.Building.Classes.BuildingModel,string,System.Collections.Generic.Dictionary_string,DiGi.Geometry.Spatial.Classes.Vector3D_,double,int,int,DiGi.EPW.Classes.EPWFile).reference'></a>

`reference` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The reference of the building\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.BuildingSolarRadiationSolveJob(DiGi.Analytical.Building.Classes.BuildingModel,string,System.Collections.Generic.Dictionary_string,DiGi.Geometry.Spatial.Classes.Vector3D_,double,int,int,DiGi.EPW.Classes.EPWFile).normals'></a>

`normals` [System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[DiGi\.Geometry\.Spatial\.Classes\.Vector3D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.spatial.classes.vector3d 'DiGi\.Geometry\.Spatial\.Classes\.Vector3D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')

The outward normal of each receiving surface, by the surface's reference\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.BuildingSolarRadiationSolveJob(DiGi.Analytical.Building.Classes.BuildingModel,string,System.Collections.Generic.Dictionary_string,DiGi.Geometry.Spatial.Classes.Vector3D_,double,int,int,DiGi.EPW.Classes.EPWFile).radius'></a>

`radius` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The neighbour radius, in m, exactly as it is stored; 0 is the building alone\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.BuildingSolarRadiationSolveJob(DiGi.Analytical.Building.Classes.BuildingModel,string,System.Collections.Generic.Dictionary_string,DiGi.Geometry.Spatial.Classes.Vector3D_,double,int,int,DiGi.EPW.Classes.EPWFile).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The county part the result is filed under\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.BuildingSolarRadiationSolveJob(DiGi.Analytical.Building.Classes.BuildingModel,string,System.Collections.Generic.Dictionary_string,DiGi.Geometry.Spatial.Classes.Vector3D_,double,int,int,DiGi.EPW.Classes.EPWFile).ePWFileId'></a>

`ePWFileId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the weather file\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.BuildingSolarRadiationSolveJob(DiGi.Analytical.Building.Classes.BuildingModel,string,System.Collections.Generic.Dictionary_string,DiGi.Geometry.Spatial.Classes.Vector3D_,double,int,int,DiGi.EPW.Classes.EPWFile).ePWFile'></a>

`ePWFile` [DiGi\.EPW\.Classes\.EPWFile](https://learn.microsoft.com/en-us/dotnet/api/digi.epw.classes.epwfile 'DiGi\.EPW\.Classes\.EPWFile')

The weather file\.
### Properties

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.BuildingModel'></a>

## BuildingSolarRadiationSolveJob\.BuildingModel Property

Gets the receiving building model\.

```csharp
public DiGi.Analytical.Building.Classes.BuildingModel BuildingModel { get; }
```

#### Property Value
[DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.CountyId'></a>

## BuildingSolarRadiationSolveJob\.CountyId Property

Gets the county part the result is filed under\.

```csharp
public int CountyId { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.EPWFile'></a>

## BuildingSolarRadiationSolveJob\.EPWFile Property

Gets the weather file\.

```csharp
public DiGi.EPW.Classes.EPWFile EPWFile { get; }
```

#### Property Value
[DiGi\.EPW\.Classes\.EPWFile](https://learn.microsoft.com/en-us/dotnet/api/digi.epw.classes.epwfile 'DiGi\.EPW\.Classes\.EPWFile')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.EPWFileId'></a>

## BuildingSolarRadiationSolveJob\.EPWFileId Property

Gets the id of the weather file\.

```csharp
public int EPWFileId { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.Normals'></a>

## BuildingSolarRadiationSolveJob\.Normals Property

Gets the outward normal of each receiving surface, by the surface's reference\.

```csharp
public System.Collections.Generic.Dictionary<string,DiGi.Geometry.Spatial.Classes.Vector3D> Normals { get; }
```

#### Property Value
[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[DiGi\.Geometry\.Spatial\.Classes\.Vector3D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.spatial.classes.vector3d 'DiGi\.Geometry\.Spatial\.Classes\.Vector3D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.Radius'></a>

## BuildingSolarRadiationSolveJob\.Radius Property

Gets the neighbour radius, in m, exactly as it is stored\.

```csharp
public double Radius { get; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob.Reference'></a>

## BuildingSolarRadiationSolveJob\.Reference Property

Gets the reference of the building\.

```csharp
public string Reference { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome'></a>

## BuildingSolarRadiationSolveOutcome Class

Holds how one [BuildingSolarRadiationSolveJob](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveJob 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveOutcome\.BuildingSolarRadiationSolveJob') ended: its result, or why it has none \(DiGi\.GIS\.PostgreSQL\#139\)\.

```csharp
public class BuildingSolarRadiationSolveOutcome
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → BuildingSolarRadiationSolveOutcome
### Constructors

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcome(DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob,DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType,DiGi.GIS.Classes.BuildingSolarRadiationResult,string,System.Exception,long,int)'></a>

## BuildingSolarRadiationSolveOutcome\(BuildingSolarRadiationSolveJob, BuildingSolarRadiationSolveOutcomeType, BuildingSolarRadiationResult, string, Exception, long, int\) Constructor

Initializes a new instance of the [BuildingSolarRadiationSolveOutcome](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveOutcome') class\.

```csharp
public BuildingSolarRadiationSolveOutcome(DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob buildingSolarRadiationSolveJob, DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType buildingSolarRadiationSolveOutcomeType, DiGi.GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult, string? reason, System.Exception? exception, long milliseconds, int casterTriangleCount);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcome(DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob,DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType,DiGi.GIS.Classes.BuildingSolarRadiationResult,string,System.Exception,long,int).buildingSolarRadiationSolveJob'></a>

`buildingSolarRadiationSolveJob` [BuildingSolarRadiationSolveJob](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveJob')

The job the outcome belongs to\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcome(DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob,DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType,DiGi.GIS.Classes.BuildingSolarRadiationResult,string,System.Exception,long,int).buildingSolarRadiationSolveOutcomeType'></a>

`buildingSolarRadiationSolveOutcomeType` [BuildingSolarRadiationSolveOutcomeType](DiGi.GIS.PostgreSQL.Solar.Enums.md#DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType 'DiGi\.GIS\.PostgreSQL\.Solar\.Enums\.BuildingSolarRadiationSolveOutcomeType')

How the job ended\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcome(DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob,DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType,DiGi.GIS.Classes.BuildingSolarRadiationResult,string,System.Exception,long,int).buildingSolarRadiationResult'></a>

`buildingSolarRadiationResult` [DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult')

The result to store; set only for [Solved](DiGi.GIS.PostgreSQL.Solar.Enums.md#DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType.Solved 'DiGi\.GIS\.PostgreSQL\.Solar\.Enums\.BuildingSolarRadiationSolveOutcomeType\.Solved')\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcome(DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob,DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType,DiGi.GIS.Classes.BuildingSolarRadiationResult,string,System.Exception,long,int).reason'></a>

`reason` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

Why the job has no result; null for a solved job\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcome(DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob,DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType,DiGi.GIS.Classes.BuildingSolarRadiationResult,string,System.Exception,long,int).exception'></a>

`exception` [System\.Exception](https://learn.microsoft.com/en-us/dotnet/api/system.exception 'System\.Exception')

The exception a failed solve threw, or null\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcome(DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob,DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType,DiGi.GIS.Classes.BuildingSolarRadiationResult,string,System.Exception,long,int).milliseconds'></a>

`milliseconds` [System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

The time spent building the shading model and solving, in ms, up to the failure for a failed job; 0 for a job over the ceiling or not solvable\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcome(DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob,DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType,DiGi.GIS.Classes.BuildingSolarRadiationResult,string,System.Exception,long,int).casterTriangleCount'></a>

`casterTriangleCount` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The number of caster triangles of the job's shading model; 0 when no shading model was built\.
### Properties

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationResult'></a>

## BuildingSolarRadiationSolveOutcome\.BuildingSolarRadiationResult Property

Gets the result to store; set only for [Solved](DiGi.GIS.PostgreSQL.Solar.Enums.md#DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType.Solved 'DiGi\.GIS\.PostgreSQL\.Solar\.Enums\.BuildingSolarRadiationSolveOutcomeType\.Solved')\.

```csharp
public DiGi.GIS.Classes.BuildingSolarRadiationResult? BuildingSolarRadiationResult { get; }
```

#### Property Value
[DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveJob'></a>

## BuildingSolarRadiationSolveOutcome\.BuildingSolarRadiationSolveJob Property

Gets the job the outcome belongs to\.

```csharp
public DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob BuildingSolarRadiationSolveJob { get; }
```

#### Property Value
[BuildingSolarRadiationSolveJob](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveJob 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveJob')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcomeType'></a>

## BuildingSolarRadiationSolveOutcome\.BuildingSolarRadiationSolveOutcomeType Property

Gets how the job ended\.

```csharp
public DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType BuildingSolarRadiationSolveOutcomeType { get; }
```

#### Property Value
[BuildingSolarRadiationSolveOutcomeType](DiGi.GIS.PostgreSQL.Solar.Enums.md#DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType 'DiGi\.GIS\.PostgreSQL\.Solar\.Enums\.BuildingSolarRadiationSolveOutcomeType')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.CasterTriangleCount'></a>

## BuildingSolarRadiationSolveOutcome\.CasterTriangleCount Property

Gets the number of caster triangles of the job's shading model \- the geometry the solve cost and memory grow with \(DiGi\.GIS\.PostgreSQL\#167\); 0 when no shading model was built\.

```csharp
public int CasterTriangleCount { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.Exception'></a>

## BuildingSolarRadiationSolveOutcome\.Exception Property

Gets the exception a failed solve threw, or null\.

```csharp
public System.Exception? Exception { get; }
```

#### Property Value
[System\.Exception](https://learn.microsoft.com/en-us/dotnet/api/system.exception 'System\.Exception')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.Milliseconds'></a>

## BuildingSolarRadiationSolveOutcome\.Milliseconds Property

Gets the time spent building the shading model and solving, in ms, up to the failure for a failed job; 0 for a job over the ceiling or not solvable\.

```csharp
public long Milliseconds { get; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveOutcome.Reason'></a>

## BuildingSolarRadiationSolveOutcome\.Reason Property

Gets why the job has no result; null for a solved job\.

```csharp
public string? Reason { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask'></a>

## BuildingSolarRadiationSolveTask Class

Represents a background task that solves the annual solar radiation of every building of a county part for one or more neighbour radii and stores it, whatever the inputs are read from and the results written to \(DiGi\.GIS\.PostgreSQL\#136, DiGi\.GIS\.PostgreSQL\.UI\#48\)\.

Every read and the write is a protected hook: [PostgreSQLBuildingSolarRadiationSolveTask](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveTask') implements them with the storage database converters, a client implements them with the GIS Web API. The walk, the skip of stored results, the tiling, the batching and read-ahead, the parallel solve, the counting and the logging are this class's alone, so a result and every count do not depend on the transport.

Each building is solved exactly as the viewer solves it (DiGi.GIS.WebAPI.UI `SolarController`): the same building model detail level, the same neighbour circle (`DiGi.GIS.Analytical.Solar.Query.SolarNeighbourCircle`), the same nearest EPW station, the same solver options and solver version, and the same stored form - so a result does not depend on which of the two produced it. Radius 0 solves the building alone (`SolarRadiusIsolated`): it still shades and blocks the view of its own surfaces, but no neighbour does.

A county part is solved in square tiles of [TileSize](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.TileSize 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.TileSize'), by the centroid of each building, at most [BatchSize](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.BatchSize 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.BatchSize') buildings at a time: one read of the batch's models, one read of every neighbour any of them can see at the largest radius, and one write of the batch's results. The neighbours of each building and radius are then picked in memory, as `gis/buildingmodel/itemsbycircle` picks them - every model whose bounding box lies within or intersects the building's circle.

The jobs of a batch - one per building and pending radius - are solved several at a time ([MaxDegreeOfParallelism](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.MaxDegreeOfParallelism 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.MaxDegreeOfParallelism'), [DiGi\.GIS\.PostgreSQL\.Solar\.Query\.BuildingSolarRadiationSolveOutcomes\(System\.Collections\.Generic\.IList\{DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveJob\},System\.Collections\.Generic\.IList\{System\.ValueTuple\{DiGi\.Analytical\.Building\.Classes\.BuildingModel,System\.Double,System\.Double,System\.Double,System\.Double\}\},System\.Int32,System\.Int32,System\.Threading\.CancellationToken\)](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.solar.query.buildingsolarradiationsolveoutcomes#digi-gis-postgresql-solar-query-buildingsolarradiationsolveoutcomes(system-collections-generic-ilist{digi-gis-postgresql-solar-classes-buildingsolarradiationsolvejob}-system-collections-generic-ilist{system-valuetuple{digi-analytical-building-classes-buildingmodel-system-double-system-double-system-double-system-double}}-system-int32-system-int32-system-threading-cancellationtoken) 'DiGi\.GIS\.PostgreSQL\.Solar\.Query\.BuildingSolarRadiationSolveOutcomes\(System\.Collections\.Generic\.IList\{DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveJob\},System\.Collections\.Generic\.IList\{System\.ValueTuple\{DiGi\.Analytical\.Building\.Classes\.BuildingModel,System\.Double,System\.Double,System\.Double,System\.Double\}\},System\.Int32,System\.Int32,System\.Threading\.CancellationToken\)')), and the next batch is read while they run; the results are still written one batch at a time, in order, and counted in job order, so the counts, failure samples and stored results are those of a sequential run (DiGi.GIS.PostgreSQL#139). Each batch logs the time spent waiting for its read, solving and writing, its neighbour and caster triangle counts, and the process memory - working set, managed heap after the last collection and after the last full collection, fragmentation, committed memory, GC pause share, full collection count and thread count - so a slow batch can be told apart as heavy geometry or a grown process (DiGi.GIS.PostgreSQL#167). A cancellation waits for the solves in flight.

[BuildingCountMax](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.BuildingCountMax 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.BuildingCountMax') cuts the walk of each part after its first buildings in that order ([SolarSolveTiles\(this IEnumerable&lt;Building2DCentroid&gt;, double, Nullable&lt;int&gt;\)](DiGi.GIS.PostgreSQL.Solar.md#DiGi.GIS.PostgreSQL.Solar.Query.SolarSolveTiles(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DCentroid_,double,System.Nullable_int_) 'DiGi\.GIS\.PostgreSQL\.Solar\.Query\.SolarSolveTiles\(this System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid\>, double, System\.Nullable\<int\>\)')), for a pilot that still logs its part and run summaries (DiGi.GIS.PostgreSQL#138).

A building that cannot be solved - no stored model, no location, no closed envelope, no weather, or above a ceiling - is counted and stepped over. A county part whose buildings, models or neighbours cannot be read, or whose results cannot be written, is failed and logged with the exception, and the run keeps going over the other parts; a run with a failed part reports itself as not succeeded.

```csharp
public abstract class BuildingSolarRadiationSolveTask : DiGi.Core.Classes.ReportableBackgroundTask<long>
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → [DiGi\.Core\.Classes\.BackgroundTask](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.backgroundtask 'DiGi\.Core\.Classes\.BackgroundTask') → [DiGi\.Core\.Classes\.CancelableBackgroundTask](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.cancelablebackgroundtask 'DiGi\.Core\.Classes\.CancelableBackgroundTask') → [DiGi\.Core\.Classes\.ReportableBackgroundTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.reportablebackgroundtask-1 'DiGi\.Core\.Classes\.ReportableBackgroundTask\`1')[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.reportablebackgroundtask-1 'DiGi\.Core\.Classes\.ReportableBackgroundTask\`1') → BuildingSolarRadiationSolveTask

Derived  
↳ [PostgreSQLBuildingSolarRadiationSolveTask](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveTask')
### Fields

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.failureSampleCountMax'></a>

## BuildingSolarRadiationSolveTask\.failureSampleCountMax Field

The largest number of failure descriptions kept in [FailureSamples](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.FailureSamples 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveTask\.FailureSamples'); the counts are never capped\.

```csharp
private const int failureSampleCountMax = 50;
```

#### Field Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')
### Properties

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.FailedCount'></a>

## BuildingSolarRadiationSolveTask\.FailedCount Property

Gets the number of building and radius solves that failed with an exception or produced a result that cannot be stored during the last run\.

```csharp
public long FailedCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.FailedCountyCount'></a>

## BuildingSolarRadiationSolveTask\.FailedCountyCount Property

Gets the number of county parts that failed outright and were stepped over during the last run\.

```csharp
public long FailedCountyCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.FailureSamples'></a>

## BuildingSolarRadiationSolveTask\.FailureSamples Property

Gets a bounded sample of the failures of the last run, at most 50 descriptions; [FailedCount](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.FailedCount 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveTask\.FailedCount') and [FailedCountyCount](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.FailedCountyCount 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveTask\.FailedCountyCount') are the full counts\.

```csharp
public System.Collections.Generic.IReadOnlyList<string> FailureSamples { get; }
```

#### Property Value
[System\.Collections\.Generic\.IReadOnlyList&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1 'System\.Collections\.Generic\.IReadOnlyList\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1 'System\.Collections\.Generic\.IReadOnlyList\`1')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.NotSolvableCount'></a>

## BuildingSolarRadiationSolveTask\.NotSolvableCount Property

Gets the number of building and radius solves stepped over during the last run because the building has no stored model, cannot be located, has no closed external envelope or has no weather\.

```csharp
public long NotSolvableCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.OverCeilingCount'></a>

## BuildingSolarRadiationSolveTask\.OverCeilingCount Property

Gets the number of building and radius solves stepped over during the last run because a ceiling was exceeded: more receiving surfaces than [ReceiverCountMax](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.ReceiverCountMax 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.ReceiverCountMax') \(counted once per radius\) or more caster triangles than [CasterTriangleCountMax](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.CasterTriangleCountMax 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.CasterTriangleCountMax')\.

```csharp
public long OverCeilingCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.PostgreSQLBuildingSolarRadiationSolveOptions'></a>

## BuildingSolarRadiationSolveTask\.PostgreSQLBuildingSolarRadiationSolveOptions Property

Gets or sets the options used to configure the run\.

```csharp
public DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions PostgreSQLBuildingSolarRadiationSolveOptions { get; set; }
```

#### Property Value
[PostgreSQLBuildingSolarRadiationSolveOptions](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ProcessedBuildingCount'></a>

## BuildingSolarRadiationSolveTask\.ProcessedBuildingCount Property

Gets the number of buildings that were considered during the last run, whatever happened to them; the progress reported\. Buildings left out by [BuildingCountMax](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.BuildingCountMax 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.BuildingCountMax') are not counted\.

```csharp
public long ProcessedBuildingCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ProcessedCountyCount'></a>

## BuildingSolarRadiationSolveTask\.ProcessedCountyCount Property

Gets the number of county parts that were processed during the last run\.

```csharp
public long ProcessedCountyCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.SkippedExistingCount'></a>

## BuildingSolarRadiationSolveTask\.SkippedExistingCount Property

Gets the number of building and radius solves stepped over during the last run because a result of the current solver version was already stored \([SkipExisting](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.SkipExisting 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.SkipExisting')\)\.

```csharp
public long SkippedExistingCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.SolvedCount'></a>

## BuildingSolarRadiationSolveTask\.SolvedCount Property

Gets the number of building and radius results solved and stored during the last run\.

```csharp
public long SolvedCount { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.SolveMilliseconds'></a>

## BuildingSolarRadiationSolveTask\.SolveMilliseconds Property

Gets the total time, in milliseconds, spent building shading models and solving during the last run \- not reading or writing\. Divided by [SolvedCount](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.SolvedCount 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveTask\.SolvedCount') it is the solve time per building and radius the runtime evidence of a run is reported from\.

```csharp
public long SolveMilliseconds { get; private set; }
```

#### Property Value
[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')
### Methods

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ExecuteAsync(System.IProgress_long_,System.Threading.CancellationToken)'></a>

## BuildingSolarRadiationSolveTask\.ExecuteAsync\(IProgress\<long\>, CancellationToken\) Method

Executes the background task that solves and stores the annual solar radiation of every building of the county parts in scope\.

```csharp
protected override System.Threading.Tasks.Task<bool> ExecuteAsync(System.IProgress<long> progress, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ExecuteAsync(System.IProgress_long_,System.Threading.CancellationToken).progress'></a>

`progress` [System\.IProgress&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1 'System\.IProgress\`1')[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1 'System\.IProgress\`1')

A progress reporter for reporting the number of buildings considered\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ExecuteAsync(System.IProgress_long_,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task representing the asynchronous operation\. Returns true when every county part in scope was processed without error; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken)'></a>

## BuildingSolarRadiationSolveTask\.ReadBuilding2DCentroidsAsync\(int, int, CancellationToken\) Method

Reads the centroid of every building filed under one county part, which the part is tiled and scoped by\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Classes.Building2DCentroid>?> ReadBuilding2DCentroidsAsync(int countyId, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county part; only buildings filed under this part may be returned\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.building2dcentroid 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the centroids \(empty when the part has no building\), or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## BuildingSolarRadiationSolveTask\.ReadBuildingModelsAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the stored building models of the given references of one county part \- the receivers of a batch\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadBuildingModelsAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The `building_2d` references of the batch, at most [BatchSize](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.BatchSize 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.BatchSize')\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the latest stored model of each reference found, stamped with its reference \(a reference with no model is simply absent, an entry that cannot be converted may be null\), or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken)'></a>

## BuildingSolarRadiationSolveTask\.ReadCountyReferencesAsync\(int, CancellationToken\) Method

Reads every county part the run may walk; [CountyIds](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.CountyIds 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.CountyIds') then narrows them\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference>?> ReadCountyReferencesAsync(int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.administrativeareal2dreference 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains every county part \- one per part, not one per code \- or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken)'></a>

## BuildingSolarRadiationSolveTask\.ReadEPWFileAsync\(int, int, CancellationToken\) Method

Reads one weather file by its station identifier; each station a county part uses is read once\.

```csharp
protected abstract System.Threading.Tasks.Task<DiGi.EPW.Classes.EPWFile?> ReadEPWFileAsync(int id, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).id'></a>

`id` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the EPW file, from [ReadEPWFileLocationsAsync\(int, CancellationToken\)](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveTask\.ReadEPWFileLocationsAsync\(int, System\.Threading\.CancellationToken\)')\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[DiGi\.EPW\.Classes\.EPWFile](https://learn.microsoft.com/en-us/dotnet/api/digi.epw.classes.epwfile 'DiGi\.EPW\.Classes\.EPWFile')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the EPW file, or null when it cannot be read \- the buildings that need it are then counted as not solvable\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken)'></a>

## BuildingSolarRadiationSolveTask\.ReadEPWFileLocationsAsync\(int, CancellationToken\) Method

Reads the identifier and location of every stored EPW station, from which each building is given its nearest one\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<(int Id,double X,double Y)>?> ReadEPWFileLocationsAsync(int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the stations, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken)'></a>

## BuildingSolarRadiationSolveTask\.ReadNeighbourBuildingModelsAsync\(Circle2D, int, CancellationToken\) Method

Reads the stored building models of every building whose footprint lies within or intersects a circle, across county borders \- the neighbours of a batch\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D circle2D, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).circle2D'></a>

`circle2D` [DiGi\.Geometry\.Planar\.Classes\.Circle2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.circle2d 'DiGi\.Geometry\.Planar\.Classes\.Circle2D')

The circle around the batch that holds every receiver's circle at the largest radius\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the models \(empty when the circle holds no building\), or null when the read failed \- never read a failure as no neighbour\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken)'></a>

## BuildingSolarRadiationSolveTask\.ReadSolvedReferencesAsync\(int, double, int, int, CancellationToken\) Method

Reads the references of the buildings of one county part that already carry a result for a radius and solver version\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Collections.Generic.HashSet<string>?> ReadSolvedReferencesAsync(int countyId, double radius, int solverVersion, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county part\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken).radius'></a>

`radius` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The radius key, in m, as stored \(`SolarRadiusKey`\)\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken).solverVersion'></a>

`solverVersion` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The solver version a stored result must carry to count\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the references \(empty when none is stored\), or null when the read failed \- never read a failure as nothing solved\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.TryInitialize()'></a>

## BuildingSolarRadiationSolveTask\.TryInitialize\(\) Method

Checks, before anything is read, that the transport has what the run needs, logging the reason at Error level when it has not\.

```csharp
protected virtual bool TryInitialize();
```

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True when the run can start; otherwise, false, and the run ends without reading anything\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Classes.BuildingSolarRadiationResult_,int,System.Threading.CancellationToken)'></a>

## BuildingSolarRadiationSolveTask\.WriteAsync\(int, List\<BuildingSolarRadiationResult\>, int, CancellationToken\) Method

Writes the results of one batch of one county part\.

```csharp
protected abstract System.Threading.Tasks.Task<System.Nullable<int>> WriteAsync(int countyId, System.Collections.Generic.List<DiGi.GIS.Classes.BuildingSolarRadiationResult> buildingSolarRadiationResults, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Classes.BuildingSolarRadiationResult_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county part every result is filed under\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Classes.BuildingSolarRadiationResult_,int,System.Threading.CancellationToken).buildingSolarRadiationResults'></a>

`buildingSolarRadiationResults` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The results of the batch, in job order; never empty\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Classes.BuildingSolarRadiationResult_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the write; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Classes.BuildingSolarRadiationResult_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the number of results written, or null when the write failed \- the part is then failed\.

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

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.BuildingCountMax'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\.BuildingCountMax Property

Gets or sets the largest number of buildings solved per county part\. Null solves every pending building of the part; values below 1 are treated as 1\.

The cap applies to the buildings still pending after [SkipExisting](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.SkipExisting 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.SkipExisting'), cut after they are put in solve order (tiles by row, references ordinal within a tile), so a capped run solves a compact cluster with realistic neighbours, and a repeat run with [SkipExisting](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.SkipExisting 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.SkipExisting') takes the next buildings. Meant for a pilot that measures time and storage before a full run (DiGi.GIS.PostgreSQL#137, #138).

```csharp
public System.Nullable<int> BuildingCountMax { get; set; }
```

#### Property Value
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

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

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.MaxDegreeOfParallelism'></a>

## PostgreSQLBuildingSolarRadiationSolveOptions\.MaxDegreeOfParallelism Property

Gets or sets the largest number of work items of a batch run at once \- the building and radius solves and the solver's work inside them share this many slots, so a heavy batch never runs more \(DiGi\.GIS\.PostgreSQL\#167\)\. 0 or less is automatic \- half the logical processors; 1 solves one at a time, each on one thread, as before DiGi\.GIS\.PostgreSQL\#139\.

The stored results do not depend on it: each solve builds its own shading model and only reads the building models.

```csharp
public int MaxDegreeOfParallelism { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

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

Represents a background task that solves the annual solar radiation of every building of a county part for one or more neighbour radii and stores it in the building solar radiation table, reading and writing the storage database directly through its converters \(DiGi\.GIS\.PostgreSQL\#136\)\.

The run itself - scope, tiling, batching, parallel solve, counting and logging - is [BuildingSolarRadiationSolveTask](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveTask')'s; this class only supplies its reads and its write. Batch rows carry no user email: no user asked for them.

```csharp
public class PostgreSQLBuildingSolarRadiationSolveTask : DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask, DiGi.GIS.PostgreSQL.Interfaces.IGISPostgreSQLObject, DiGi.Core.Interfaces.IObject
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → [DiGi\.Core\.Classes\.BackgroundTask](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.backgroundtask 'DiGi\.Core\.Classes\.BackgroundTask') → [DiGi\.Core\.Classes\.CancelableBackgroundTask](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.cancelablebackgroundtask 'DiGi\.Core\.Classes\.CancelableBackgroundTask') → [DiGi\.Core\.Classes\.ReportableBackgroundTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.reportablebackgroundtask-1 'DiGi\.Core\.Classes\.ReportableBackgroundTask\`1')[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.reportablebackgroundtask-1 'DiGi\.Core\.Classes\.ReportableBackgroundTask\`1') → [BuildingSolarRadiationSolveTask](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveTask') → PostgreSQLBuildingSolarRadiationSolveTask

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

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.gISPostgreSQLConverterManager'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.gISPostgreSQLConverterManager Field

The GIS PostgreSQL converter manager used to retrieve converters and execute operations\.

```csharp
protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;
```

#### Field Value
[DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.gispostgresqlconvertermanager 'DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager')
### Methods

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.ReadBuilding2DCentroidsAsync\(int, int, CancellationToken\) Method

Reads the centroid of every building filed under one county part, which the part is tiled and scoped by\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Classes.Building2DCentroid>?> ReadBuilding2DCentroidsAsync(int countyId, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county part; only buildings filed under this part may be returned\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadBuilding2DCentroidsAsync(int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.building2dcentroid 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the centroids \(empty when the part has no building\), or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.ReadBuildingModelsAsync\(int, List\<string\>, int, CancellationToken\) Method

Reads the stored building models of the given references of one county part \- the receivers of a batch\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadBuildingModelsAsync(int countyId, System.Collections.Generic.List<string> references, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county part the references are filed under\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The `building_2d` references of the batch, at most [BatchSize](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.BatchSize 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.BatchSize')\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadBuildingModelsAsync(int,System.Collections.Generic.List_string_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the latest stored model of each reference found, stamped with its reference \(a reference with no model is simply absent, an entry that cannot be converted may be null\), or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.ReadCountyReferencesAsync\(int, CancellationToken\) Method

Reads every county part the run may walk; [CountyIds](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveOptions.CountyIds 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.PostgreSQLBuildingSolarRadiationSolveOptions\.CountyIds') then narrows them\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference>?> ReadCountyReferencesAsync(int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadCountyReferencesAsync(int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.administrativeareal2dreference 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains every county part \- one per part, not one per code \- or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.ReadEPWFileAsync\(int, int, CancellationToken\) Method

Reads one weather file by its station identifier; each station a county part uses is read once\.

```csharp
protected override System.Threading.Tasks.Task<DiGi.EPW.Classes.EPWFile?> ReadEPWFileAsync(int id, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).id'></a>

`id` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the EPW file, from [ReadEPWFileLocationsAsync\(int, CancellationToken\)](DiGi.GIS.PostgreSQL.Solar.Classes.md#DiGi.GIS.PostgreSQL.Solar.Classes.BuildingSolarRadiationSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Solar\.Classes\.BuildingSolarRadiationSolveTask\.ReadEPWFileLocationsAsync\(int, System\.Threading\.CancellationToken\)')\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadEPWFileAsync(int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[DiGi\.EPW\.Classes\.EPWFile](https://learn.microsoft.com/en-us/dotnet/api/digi.epw.classes.epwfile 'DiGi\.EPW\.Classes\.EPWFile')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the EPW file, or null when it cannot be read \- the buildings that need it are then counted as not solvable\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.ReadEPWFileLocationsAsync\(int, CancellationToken\) Method

Reads the identifier and location of every stored EPW station, from which each building is given its nearest one\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<(int Id,double X,double Y)>?> ReadEPWFileLocationsAsync(int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadEPWFileLocationsAsync(int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the stations, or null when the read failed\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.ReadNeighbourBuildingModelsAsync\(Circle2D, int, CancellationToken\) Method

Reads the stored building models of every building whose footprint lies within or intersects a circle, across county borders \- the neighbours of a batch\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D circle2D, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).circle2D'></a>

`circle2D` [DiGi\.Geometry\.Planar\.Classes\.Circle2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.circle2d 'DiGi\.Geometry\.Planar\.Classes\.Circle2D')

The circle around the batch that holds every receiver's circle at the largest radius\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadNeighbourBuildingModelsAsync(DiGi.Geometry.Planar.Classes.Circle2D,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the models \(empty when the circle holds no building\), or null when the read failed \- never read a failure as no neighbour\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.ReadSolvedReferencesAsync\(int, double, int, int, CancellationToken\) Method

Reads the references of the buildings of one county part that already carry a result for a radius and solver version\.

```csharp
protected override System.Threading.Tasks.Task<System.Collections.Generic.HashSet<string>?> ReadSolvedReferencesAsync(int countyId, double radius, int solverVersion, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county part\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken).radius'></a>

`radius` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The radius key, in m, as stored \(`SolarRadiusKey`\)\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken).solverVersion'></a>

`solverVersion` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The solver version a stored result must carry to count\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the read; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.ReadSolvedReferencesAsync(int,double,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the references \(empty when none is stored\), or null when the read failed \- never read a failure as nothing solved\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.TryInitialize()'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.TryInitialize\(\) Method

Checks, before anything is read, that the transport has what the run needs, logging the reason at Error level when it has not\.

```csharp
protected override bool TryInitialize();
```

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True when the run can start; otherwise, false, and the run ends without reading anything\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Classes.BuildingSolarRadiationResult_,int,System.Threading.CancellationToken)'></a>

## PostgreSQLBuildingSolarRadiationSolveTask\.WriteAsync\(int, List\<BuildingSolarRadiationResult\>, int, CancellationToken\) Method

Writes the results of one batch of one county part\.

```csharp
protected override System.Threading.Tasks.Task<System.Nullable<int>> WriteAsync(int countyId, System.Collections.Generic.List<DiGi.GIS.Classes.BuildingSolarRadiationResult> buildingSolarRadiationResults, int commandTimeout, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Classes.BuildingSolarRadiationResult_,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county part every result is filed under\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Classes.BuildingSolarRadiationResult_,int,System.Threading.CancellationToken).buildingSolarRadiationResults'></a>

`buildingSolarRadiationResults` [System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

The results of the batch, in job order; never empty\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Classes.BuildingSolarRadiationResult_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds of the write; 0 disables it\.

<a name='DiGi.GIS.PostgreSQL.Solar.Classes.PostgreSQLBuildingSolarRadiationSolveTask.WriteAsync(int,System.Collections.Generic.List_DiGi.GIS.Classes.BuildingSolarRadiationResult_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used to cancel the operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task whose result contains the number of results written, or null when the write failed \- the part is then failed\.