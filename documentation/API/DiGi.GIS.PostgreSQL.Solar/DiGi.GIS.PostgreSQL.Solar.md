#### [DiGi\.GIS\.PostgreSQL\.Solar](DiGi.GIS.PostgreSQL.Solar.Overview.md 'DiGi\.GIS\.PostgreSQL\.Solar\.Overview')

## DiGi\.GIS\.PostgreSQL\.Solar Namespace
### Classes

<a name='DiGi.GIS.PostgreSQL.Solar.Query'></a>

## Query Class

```csharp
public static class Query
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → Query
### Methods

<a name='DiGi.GIS.PostgreSQL.Solar.Query.SolarSolveTiles(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DCentroid_,double,System.Nullable_int_)'></a>

## Query\.SolarSolveTiles\(this IEnumerable\<Building2DCentroid\>, double, Nullable\<int\>\) Method

Puts the buildings of a county part in the order the batch solar radiation solve walks them, and cuts the walk at [buildingCountMax](DiGi.GIS.PostgreSQL.Solar.md#DiGi.GIS.PostgreSQL.Solar.Query.SolarSolveTiles(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DCentroid_,double,System.Nullable_int_).buildingCountMax 'DiGi\.GIS\.PostgreSQL\.Solar\.Query\.SolarSolveTiles\(this System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid\>, double, System\.Nullable\<int\>\)\.buildingCountMax') buildings\.

Buildings are grouped into square tiles of [tileSize](DiGi.GIS.PostgreSQL.Solar.md#DiGi.GIS.PostgreSQL.Solar.Query.SolarSolveTiles(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DCentroid_,double,System.Nullable_int_).tileSize 'DiGi\.GIS\.PostgreSQL\.Solar\.Query\.SolarSolveTiles\(this System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid\>, double, System\.Nullable\<int\>\)\.tileSize') by their centroid; tiles go in row order (by Y, then by X) and the references of a tile in ordinal order, so a re-run walks the same batches. The cut is made after ordering: the buildings kept are the first ones of the walk, a compact cluster, and the last tile kept may be partial (DiGi.GIS.PostgreSQL#138).

A centroid that is null, has no reference or has a non-finite coordinate is stepped over; a repeated reference is kept once, at its first occurrence.

```csharp
public static System.Collections.Generic.List<System.Collections.Generic.List<string>>? SolarSolveTiles(this System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.Building2DCentroid?>? building2DCentroids, double tileSize, System.Nullable<int> buildingCountMax=null);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Solar.Query.SolarSolveTiles(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DCentroid_,double,System.Nullable_int_).building2DCentroids'></a>

`building2DCentroids` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.postgresql.classes.building2dcentroid 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The centroids of the buildings to solve\.

<a name='DiGi.GIS.PostgreSQL.Solar.Query.SolarSolveTiles(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DCentroid_,double,System.Nullable_int_).tileSize'></a>

`tileSize` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The edge length of a tile, in m\. Values below 1 are treated as 1\.

<a name='DiGi.GIS.PostgreSQL.Solar.Query.SolarSolveTiles(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DCentroid_,double,System.Nullable_int_).buildingCountMax'></a>

`buildingCountMax` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The largest number of buildings kept in all\. Null keeps every building; values below 1 are treated as 1\.

#### Returns
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')  
The references of each tile in walk order, or null when [building2DCentroids](DiGi.GIS.PostgreSQL.Solar.md#DiGi.GIS.PostgreSQL.Solar.Query.SolarSolveTiles(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DCentroid_,double,System.Nullable_int_).building2DCentroids 'DiGi\.GIS\.PostgreSQL\.Solar\.Query\.SolarSolveTiles\(this System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Classes\.Building2DCentroid\>, double, System\.Nullable\<int\>\)\.building2DCentroids') is null\.