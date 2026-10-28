#### [DiGi\.GIS\.PostgreSQL](DiGi.GIS.PostgreSQL.Overview.md 'DiGi\.GIS\.PostgreSQL\.Overview')

## DiGi\.GIS\.PostgreSQL Namespace
### Classes

<a name='DiGi.GIS.PostgreSQL.Convert'></a>

## Convert Class

```csharp
public static class Convert
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → Convert
### Methods

<a name='DiGi.GIS.PostgreSQL.Convert.ToDiGi_TSerializableObject_(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject__)'></a>

## Convert\.ToDiGi\<TSerializableObject\>\(this IEnumerable\<ITableSerializableObject\<TSerializableObject\>\>\) Method

Converts a collection of table\-serializable objects \(database row envelopes\) to the DiGi serializable objects they wrap, skipping entries that cannot be converted\.

```csharp
public static System.Collections.Generic.List<TSerializableObject>? ToDiGi<TSerializableObject>(this System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject<TSerializableObject>>? tableSerializableObjects)
    where TSerializableObject : DiGi.Core.Interfaces.ISerializableObject;
```
#### Type parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToDiGi_TSerializableObject_(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject__).TSerializableObject'></a>

`TSerializableObject`

The type of the wrapped DiGi serializable object\.
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToDiGi_TSerializableObject_(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject__).tableSerializableObjects'></a>

`tableSerializableObjects` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject&lt;](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>')[TSerializableObject](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToDiGi_TSerializableObject_(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject__).TSerializableObject 'DiGi\.GIS\.PostgreSQL\.Convert\.ToDiGi\<TSerializableObject\>\(this System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>\>\)\.TSerializableObject')[&gt;](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The collection of [ITableSerializableObject&lt;TSerializableObject&gt;](DiGi.GIS.PostgreSQL.Interfaces.md#DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject_ 'DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>') instances to convert\. This value can be null\.

#### Returns
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[TSerializableObject](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToDiGi_TSerializableObject_(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject__).TSerializableObject 'DiGi\.GIS\.PostgreSQL\.Convert\.ToDiGi\<TSerializableObject\>\(this System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>\>\)\.TSerializableObject')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')  
A list of [TSerializableObject](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToDiGi_TSerializableObject_(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject__).TSerializableObject 'DiGi\.GIS\.PostgreSQL\.Convert\.ToDiGi\<TSerializableObject\>\(this System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>\>\)\.TSerializableObject') instances, or null if [tableSerializableObjects](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToDiGi_TSerializableObject_(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Interfaces.ITableSerializableObject_TSerializableObject__).tableSerializableObjects 'DiGi\.GIS\.PostgreSQL\.Convert\.ToDiGi\<TSerializableObject\>\(this System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Interfaces\.ITableSerializableObject\<TSerializableObject\>\>\)\.tableSerializableObjects') is null\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.Analytical.Building.Classes.BuildingModel,System.Nullable_int_)'></a>

## Convert\.ToPostgreSQL\(this BuildingModel, Nullable\<int\>\) Method

Converts the specified analytical building model to a PostgreSQL\-compatible building model object, reading the reference from the building model parameters and taking the county identifier as an argument\.

The row carries the identifier of the <b>model</b> in `UniqueId` and the reference of the 2D building it describes in `Reference`, which is the addressing convention every referenced-object table follows - see [Building2DReferencedObject&lt;TUniqueObject&gt;](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DReferencedObject_TUniqueObject_ 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DReferencedObject\<TUniqueObject\>'). `(CountyId, Reference)` addresses everything held for the building; `UniqueId` addresses this one model within it.

A model is handed a fresh [System\.Guid](https://learn.microsoft.com/en-us/dotnet/api/system.guid 'System\.Guid') whenever one is created, so a regenerated model carries a new identifier and is stored <b>beside</b> the one the building already had rather than replacing it. That is the intended behaviour of the table, and it makes replacing a building's model the caller's job: remove what the building holds, then write. It is not a reason to key the row on the reference instead - that pins the table to one row per building and discards every record after the first.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.BuildingModel? ToPostgreSQL(this DiGi.Analytical.Building.Classes.BuildingModel? buildingModel, System.Nullable<int> countyId=null);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.Analytical.Building.Classes.BuildingModel,System.Nullable_int_).buildingModel'></a>

`buildingModel` [DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')

The analytical building model to convert\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.Analytical.Building.Classes.BuildingModel,System.Nullable_int_).countyId'></a>

`countyId` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The identifier of the county the building model belongs to, resolved by the caller from the administrative area code\.

#### Returns
[BuildingModel](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingModel 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingModel')  
A [BuildingModel](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingModel 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingModel') object if the provided building model is not null and carries both the [DiGi\.GIS\.Analytical\.Enums\.BuildingModelParameter\.Reference](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.enums.buildingmodelparameter.reference 'DiGi\.GIS\.Analytical\.Enums\.BuildingModelParameter\.Reference') parameter value and its own unique identifier; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.CityGML.Classes.Building,System.Nullable_int_)'></a>

## Convert\.ToPostgreSQL\(this Building, Nullable\<int\>\) Method

Converts a CityGML Building instance to a PostgreSQL\-compatible Building instance\.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.Building? ToPostgreSQL(this DiGi.CityGML.Classes.Building? building, System.Nullable<int> countyId=null);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.CityGML.Classes.Building,System.Nullable_int_).building'></a>

`building` [DiGi\.CityGML\.Classes\.Building](https://learn.microsoft.com/en-us/dotnet/api/digi.citygml.classes.building 'DiGi\.CityGML\.Classes\.Building')

The source CityGML building object to convert\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.CityGML.Classes.Building,System.Nullable_int_).countyId'></a>

`countyId` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

An optional county identifier associated with the building\.

#### Returns
[Building](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building 'DiGi\.GIS\.PostgreSQL\.Classes\.Building')  
A converted [Building](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building 'DiGi\.GIS\.PostgreSQL\.Classes\.Building') instance, or null if the input is null\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.AdministrativeAreal2D)'></a>

## Convert\.ToPostgreSQL\(this AdministrativeAreal2D\) Method

Converts a GIS administrative areal 2D object to its PostgreSQL representation\.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D? ToPostgreSQL(this DiGi.GIS.Classes.AdministrativeAreal2D? administrativeAreal2D);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.AdministrativeAreal2D).administrativeAreal2D'></a>

`administrativeAreal2D` [DiGi\.GIS\.Classes\.AdministrativeAreal2D](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.administrativeareal2d 'DiGi\.GIS\.Classes\.AdministrativeAreal2D')

The source administrative areal 2D object to convert\.

#### Returns
[AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')  
The converted [AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D') object, or null if the input is null\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.Building2D,string)'></a>

## Convert\.ToPostgreSQL\(this Building2D, string\) Method

Converts a GIS Building2D instance to a PostgreSQL\-compatible Building2D instance\.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.Building2D? ToPostgreSQL(this DiGi.GIS.Classes.Building2D? building2D, string? code=null);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.Building2D,string).building2D'></a>

`building2D` [DiGi\.GIS\.Classes\.Building2D](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.building2d 'DiGi\.GIS\.Classes\.Building2D')

The source building 2D object to convert\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.Building2D,string).code'></a>

`code` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

An optional code associated with the building\.

#### Returns
[Building2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2D 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2D')  
A converted [Building2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2D 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2D') instance, or null if the input is null\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.OrtoDatas,System.Nullable_int_)'></a>

## Convert\.ToPostgreSQL\(this OrtoDatas, Nullable\<int\>\) Method

Converts a GIS OrtoDatas instance to a PostgreSQL\-compatible OrtoDatas instance\.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.OrtoDatas? ToPostgreSQL(this DiGi.GIS.Classes.OrtoDatas? ortoDatas, System.Nullable<int> countyId);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.OrtoDatas,System.Nullable_int_).ortoDatas'></a>

`ortoDatas` [DiGi\.GIS\.Classes\.OrtoDatas](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.ortodatas 'DiGi\.GIS\.Classes\.OrtoDatas')

The source GIS OrtoDatas object to convert\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.OrtoDatas,System.Nullable_int_).countyId'></a>

`countyId` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The optional county identifier associated with the data\.

#### Returns
[OrtoDatas](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatas 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatas')  
A new PostgreSQL\-compatible [OrtoDatas](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatas 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatas') instance, or null if the input is null\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.OrtoDatas,System.Nullable_int_,System.Nullable_int_)'></a>

## Convert\.ToPostgreSQL\(this OrtoDatas, Nullable\<int\>, Nullable\<int\>\) Method

Converts a GIS OrtoDatas instance to a PostgreSQL\-compatible OrtoDatas instance with a specified subdivision identifier\.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.OrtoDatas? ToPostgreSQL(this DiGi.GIS.Classes.OrtoDatas? ortoDatas, System.Nullable<int> countyId, System.Nullable<int> subdivisionId);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.OrtoDatas,System.Nullable_int_,System.Nullable_int_).ortoDatas'></a>

`ortoDatas` [DiGi\.GIS\.Classes\.OrtoDatas](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.ortodatas 'DiGi\.GIS\.Classes\.OrtoDatas')

The source GIS OrtoDatas object to convert\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.OrtoDatas,System.Nullable_int_,System.Nullable_int_).countyId'></a>

`countyId` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The optional county identifier associated with the data\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Classes.OrtoDatas,System.Nullable_int_,System.Nullable_int_).subdivisionId'></a>

`subdivisionId` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The optional subdivision identifier associated with the data\.

#### Returns
[OrtoDatas](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatas 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatas')  
A new PostgreSQL\-compatible [OrtoDatas](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatas 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatas') instance, or null if the input is null\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Interfaces.IOccupancyData)'></a>

## Convert\.ToPostgreSQL\(this IOccupancyData\) Method

Converts the specified occupancy data to a PostgreSQL\-compatible administrative areal 2D occupancy data object\.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DOccupancyData? ToPostgreSQL(this DiGi.GIS.Interfaces.IOccupancyData? occupancyData);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Interfaces.IOccupancyData).occupancyData'></a>

`occupancyData` [DiGi\.GIS\.Interfaces\.IOccupancyData](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.interfaces.ioccupancydata 'DiGi\.GIS\.Interfaces\.IOccupancyData')

The occupancy data to convert\.

#### Returns
[AdministrativeAreal2DOccupancyData](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DOccupancyData 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DOccupancyData')  
A [AdministrativeAreal2DOccupancyData](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DOccupancyData 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DOccupancyData') instance if the provided occupancy data is not null; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Interfaces.IOccupancyData,System.Nullable_int_)'></a>

## Convert\.ToPostgreSQL\(this IOccupancyData, Nullable\<int\>\) Method

Converts the specified occupancy data to a PostgreSQL\-compatible building 2D occupancy data object\.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyData? ToPostgreSQL(this DiGi.GIS.Interfaces.IOccupancyData? occupancyData, System.Nullable<int> countyId);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Interfaces.IOccupancyData,System.Nullable_int_).occupancyData'></a>

`occupancyData` [DiGi\.GIS\.Interfaces\.IOccupancyData](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.interfaces.ioccupancydata 'DiGi\.GIS\.Interfaces\.IOccupancyData')

The occupancy data to convert\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Interfaces.IOccupancyData,System.Nullable_int_).countyId'></a>

`countyId` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The optional county identifier associated with the occupancy data\.

#### Returns
[Building2DOccupancyData](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyData 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DOccupancyData')  
A [Building2DOccupancyData](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyData 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DOccupancyData') object if the provided occupancy data is not null; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Interfaces.IYearBuiltData,System.Nullable_int_)'></a>

## Convert\.ToPostgreSQL\(this IYearBuiltData, Nullable\<int\>\) Method

Converts the specified [DiGi\.GIS\.Interfaces\.IYearBuiltData](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.interfaces.iyearbuiltdata 'DiGi\.GIS\.Interfaces\.IYearBuiltData') instance to a [YearBuiltData](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.YearBuiltData 'DiGi\.GIS\.PostgreSQL\.Classes\.YearBuiltData') object for PostgreSQL storage\.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.YearBuiltData? ToPostgreSQL(this DiGi.GIS.Interfaces.IYearBuiltData? yearBuiltData, System.Nullable<int> countyId);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Interfaces.IYearBuiltData,System.Nullable_int_).yearBuiltData'></a>

`yearBuiltData` [DiGi\.GIS\.Interfaces\.IYearBuiltData](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.interfaces.iyearbuiltdata 'DiGi\.GIS\.Interfaces\.IYearBuiltData')

The source year built data\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Interfaces.IYearBuiltData,System.Nullable_int_).countyId'></a>

`countyId` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The county identifier associated with the data\.

#### Returns
[YearBuiltData](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.YearBuiltData 'DiGi\.GIS\.PostgreSQL\.Classes\.YearBuiltData')  
A [YearBuiltData](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.YearBuiltData 'DiGi\.GIS\.PostgreSQL\.Classes\.YearBuiltData') instance if [yearBuiltData](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToPostgreSQL(thisDiGi.GIS.Interfaces.IYearBuiltData,System.Nullable_int_).yearBuiltData 'DiGi\.GIS\.PostgreSQL\.Convert\.ToPostgreSQL\(this DiGi\.GIS\.Interfaces\.IYearBuiltData, System\.Nullable\<int\>\)\.yearBuiltData') is not null; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToSystem_ByteArrays(thisbyte[])'></a>

## Convert\.ToSystem\_ByteArrays\(this byte\[\]\) Method

Unpacks the `shading_factors` value of the [BuildingSolarRadiation](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.BuildingSolarRadiation 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.BuildingSolarRadiation') table into the shading factors of each surface, the inverse of [ToSystem\_Bytes\(this IEnumerable&lt;byte\[\]&gt;\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToSystem_Bytes(thisSystem.Collections.Generic.IEnumerable_byte[]_) 'DiGi\.GIS\.PostgreSQL\.Convert\.ToSystem\_Bytes\(this System\.Collections\.Generic\.IEnumerable\<byte\[\]\>\)')\.

```csharp
public static System.Collections.Generic.List<byte[]>? ToSystem_ByteArrays(this byte[]? bytes);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToSystem_ByteArrays(thisbyte[]).bytes'></a>

`bytes` [System\.Byte](https://learn.microsoft.com/en-us/dotnet/api/system.byte 'System\.Byte')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

The packed value: a little\-endian `uint32` hour count `n`, then `n` bytes per surface\.

#### Returns
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.Byte](https://learn.microsoft.com/en-us/dotnet/api/system.byte 'System\.Byte')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')  
One array of `n` bytes per surface, in stored order, or null when [bytes](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToSystem_ByteArrays(thisbyte[]).bytes 'DiGi\.GIS\.PostgreSQL\.Convert\.ToSystem\_ByteArrays\(this byte\[\]\)\.bytes') is null, shorter than its header, has a zero hour count, or its length is not a whole number of surfaces\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToSystem_Bytes(thisSystem.Collections.Generic.IEnumerable_byte[]_)'></a>

## Convert\.ToSystem\_Bytes\(this IEnumerable\<byte\[\]\>\) Method

Packs the shading factors of every surface of a stored solar radiation result into the single `shading_factors` value of the [BuildingSolarRadiation](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.BuildingSolarRadiation 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.BuildingSolarRadiation') table\.

The layout is a little-endian `uint32` hour count `n`, then `n` bytes per surface in the order given (the order of the surfaces in the stored object). Keeping the count in the value makes it self-describing, so a later time step still unpacks with [ToSystem\_ByteArrays\(this byte\[\]\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToSystem_ByteArrays(thisbyte[]) 'DiGi\.GIS\.PostgreSQL\.Convert\.ToSystem\_ByteArrays\(this byte\[\]\)').

```csharp
public static byte[]? ToSystem_Bytes(this System.Collections.Generic.IEnumerable<byte[]?>? shadingFactors);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToSystem_Bytes(thisSystem.Collections.Generic.IEnumerable_byte[]_).shadingFactors'></a>

`shadingFactors` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Byte](https://learn.microsoft.com/en-us/dotnet/api/system.byte 'System\.Byte')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The quantized shading factors of each surface, in surface order\.

#### Returns
[System\.Byte](https://learn.microsoft.com/en-us/dotnet/api/system.byte 'System\.Byte')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')  
The packed value, or null when [shadingFactors](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToSystem_Bytes(thisSystem.Collections.Generic.IEnumerable_byte[]_).shadingFactors 'DiGi\.GIS\.PostgreSQL\.Convert\.ToSystem\_Bytes\(this System\.Collections\.Generic\.IEnumerable\<byte\[\]\>\)\.shadingFactors') is null or empty, an element is null or empty, or the elements differ in length\.

<a name='DiGi.GIS.PostgreSQL.Convert.ToSystem_Bytes(thisSystem.Text.Json.Nodes.JsonNode)'></a>

## Convert\.ToSystem\_Bytes\(this JsonNode\) Method

Converts the stored form of a `byte[]` member back to bytes\.

The DiGi serializer writes a `byte[]` as a JSON array of numbers (`Core.Create.JsonNode` treats it as an enumerable), so that is the shape a projection such as `v->'Bytes'` reads out of a stored object. A base64 string - the form System.Text.Json gives a `byte[]` - is accepted as well, so a row written that way decodes rather than fails. Any other shape answers null.

```csharp
public static byte[]? ToSystem_Bytes(this System.Text.Json.Nodes.JsonNode? jsonNode);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Convert.ToSystem_Bytes(thisSystem.Text.Json.Nodes.JsonNode).jsonNode'></a>

`jsonNode` [System\.Text\.Json\.Nodes\.JsonNode](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonnode 'System\.Text\.Json\.Nodes\.JsonNode')

The [System\.Text\.Json\.Nodes\.JsonNode](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonnode 'System\.Text\.Json\.Nodes\.JsonNode') holding the member\. This value can be null\.

#### Returns
[System\.Byte](https://learn.microsoft.com/en-us/dotnet/api/system.byte 'System\.Byte')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')  
The bytes, or null when [jsonNode](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToSystem_Bytes(thisSystem.Text.Json.Nodes.JsonNode).jsonNode 'DiGi\.GIS\.PostgreSQL\.Convert\.ToSystem\_Bytes\(this System\.Text\.Json\.Nodes\.JsonNode\)\.jsonNode') is null, an element is not a byte, or the node is neither an array nor a base64 string\.

<a name='DiGi.GIS.PostgreSQL.Create'></a>

## Create Class

```csharp
public static class Create
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → Create
### Methods

<a name='DiGi.GIS.PostgreSQL.Create.BuildingDataCoverageResultAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,System.Threading.CancellationToken)'></a>

## Create\.BuildingDataCoverageResultAsync\(this BuildingDataPostgreSQLConverter, Building2DPostgreSQLConverter, int, int, CancellationToken\) Method

Asynchronously measures what one county's building data holds against the buildings that county actually has\.

The comparison is made on references read from each side rather than by a join, because the two tables are in different databases - `building_2d` in the main one and `building_data` in the storage one.

The reads run sequentially on their own connections without fanning out per building or per subdivision: a coverage read that opened a connection per item is what exhausted the pool the last time this shape was written.

```csharp
public static System.Threading.Tasks.Task<DiGi.GIS.PostgreSQL.Classes.BuildingDataCoverageResult?> BuildingDataCoverageResultAsync(this DiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter? building2DPostgreSQLConverter, int countyId, int commandTimeout=600, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.BuildingDataCoverageResultAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,System.Threading.CancellationToken).buildingDataPostgreSQLConverter'></a>

`buildingDataPostgreSQLConverter` [BuildingDataPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataPostgreSQLConverter')

The converter reading the building data side\.

<a name='DiGi.GIS.PostgreSQL.Create.BuildingDataCoverageResultAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,System.Threading.CancellationToken).building2DPostgreSQLConverter'></a>

`building2DPostgreSQLConverter` [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')

The converter reading the building side\.

<a name='DiGi.GIS.PostgreSQL.Create.BuildingDataCoverageResultAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county to measure\.

<a name='DiGi.GIS.PostgreSQL.Create.BuildingDataCoverageResultAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\. Defaults to 600 seconds\.

<a name='DiGi.GIS.PostgreSQL.Create.BuildingDataCoverageResultAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[BuildingDataCoverageResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingDataCoverageResult 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataCoverageResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result contains the coverage, or null when either converter is missing or either side could not be read\.

<a name='DiGi.GIS.PostgreSQL.Create.BuildingSolarRadiationResult(thisDiGi.GIS.Classes.BuildingSolarRadiationResult,int,System.Collections.Generic.IList_byte[]_)'></a>

## Create\.BuildingSolarRadiationResult\(this BuildingSolarRadiationResult, int, IList\<byte\[\]\>\) Method

Creates a copy of a stored solar radiation result filed under the given county part and carrying the given shading factors, the two things that differ between the result a caller holds and the row the [BuildingSolarRadiation](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.BuildingSolarRadiation 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.BuildingSolarRadiation') table stores\.

A write passes the resolved county part and null factors: the `object` column holds the result without factors, which go to the packed `shading_factors` column. A read passes the factors unpacked from that column back. Every other member is copied as it is.

```csharp
public static DiGi.GIS.Classes.BuildingSolarRadiationResult? BuildingSolarRadiationResult(this DiGi.GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult, int countyId, System.Collections.Generic.IList<byte[]?>? shadingFactors);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.BuildingSolarRadiationResult(thisDiGi.GIS.Classes.BuildingSolarRadiationResult,int,System.Collections.Generic.IList_byte[]_).buildingSolarRadiationResult'></a>

`buildingSolarRadiationResult` [DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult')

The result to copy\.

<a name='DiGi.GIS.PostgreSQL.Create.BuildingSolarRadiationResult(thisDiGi.GIS.Classes.BuildingSolarRadiationResult,int,System.Collections.Generic.IList_byte[]_).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The county part the copy is filed under\.

<a name='DiGi.GIS.PostgreSQL.Create.BuildingSolarRadiationResult(thisDiGi.GIS.Classes.BuildingSolarRadiationResult,int,System.Collections.Generic.IList_byte[]_).shadingFactors'></a>

`shadingFactors` [System\.Collections\.Generic\.IList&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1')[System\.Byte](https://learn.microsoft.com/en-us/dotnet/api/system.byte 'System\.Byte')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1')

The shading factors of each surface, in surface order, or null to remove every surface's factors\.

#### Returns
[DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult')  
The copy, or null when [buildingSolarRadiationResult](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Create.BuildingSolarRadiationResult(thisDiGi.GIS.Classes.BuildingSolarRadiationResult,int,System.Collections.Generic.IList_byte[]_).buildingSolarRadiationResult 'DiGi\.GIS\.PostgreSQL\.Create\.BuildingSolarRadiationResult\(this DiGi\.GIS\.Classes\.BuildingSolarRadiationResult, int, System\.Collections\.Generic\.IList\<byte\[\]\>\)\.buildingSolarRadiationResult') is null or [shadingFactors](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Create.BuildingSolarRadiationResult(thisDiGi.GIS.Classes.BuildingSolarRadiationResult,int,System.Collections.Generic.IList_byte[]_).shadingFactors 'DiGi\.GIS\.PostgreSQL\.Create\.BuildingSolarRadiationResult\(this DiGi\.GIS\.Classes\.BuildingSolarRadiationResult, int, System\.Collections\.Generic\.IList\<byte\[\]\>\)\.shadingFactors') does not hold one entry per surface\.

<a name='DiGi.GIS.PostgreSQL.Create.GISPostgreSQLConverterManager()'></a>

## Create\.GISPostgreSQLConverterManager\(\) Method

Creates a [GISPostgreSQLConverterManager\(\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Create.GISPostgreSQLConverterManager() 'DiGi\.GIS\.PostgreSQL\.Create\.GISPostgreSQLConverterManager\(\)') with all PostgreSQL converters registered\.
Reads connection configuration from `PostgreSQL_Main` and `PostgreSQL_Storage` files
in the executing assembly's directory\.

IMPORTANT: Every converter consumed by a GIS WebAPI controller (e.g. `BuildingController`,
`AdministrativeAreal2DController`) MUST be registered here. The WebAPI `InitializeAsync`
reads converters from the returned manager and adds them to the DI container. A missing
registration causes the controller's converter dependency to be [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null'),
resulting in a 500 Internal Server Error at runtime.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager? GISPostgreSQLConverterManager();
```

#### Returns
[GISPostgreSQLConverterManager](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager 'DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager')  
A configured [GISPostgreSQLConverterManager\(\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Create.GISPostgreSQLConverterManager() 'DiGi\.GIS\.PostgreSQL\.Create\.GISPostgreSQLConverterManager\(\)') if successful; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Create.Reference(DiGi.Analytical.Building.Classes.BuildingModel,DiGi.Analytical.Building.Interfaces.IBuildingGuidObject,System.Nullable_int_)'></a>

## Create\.Reference\(BuildingModel, IBuildingGuidObject, Nullable\<int\>\) Method

Creates a reference chain for the specified building model, optionally anchored to a county administrative division and a specific building element\.

The reference chain is ordered from the root of the containment hierarchy inwards: [DiGi\.GIS\.Classes\.AdministrativeDivision](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.administrativedivision 'DiGi\.GIS\.Classes\.AdministrativeDivision') (if a county identifier is provided), [DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel') (by its [DiGi\.GIS\.Analytical\.Enums\.BuildingModelParameter\.Reference](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.analytical.enums.buildingmodelparameter.reference 'DiGi\.GIS\.Analytical\.Enums\.BuildingModelParameter\.Reference') parameter value or a [DiGi\.Core\.Classes\.GuidReference](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.guidreference 'DiGi\.Core\.Classes\.GuidReference') fallback), and the optional [DiGi\.Analytical\.Building\.Interfaces\.IBuildingGuidObject](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.interfaces.ibuildingguidobject 'DiGi\.Analytical\.Building\.Interfaces\.IBuildingGuidObject') element. If the chain contains only a single entry, that entry is returned directly instead of wrapped in a [DiGi\.Core\.Classes\.ComplexReference](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.complexreference 'DiGi\.Core\.Classes\.ComplexReference').

```csharp
public static DiGi.Core.Interfaces.IReference? Reference(DiGi.Analytical.Building.Classes.BuildingModel buildingModel, DiGi.Analytical.Building.Interfaces.IBuildingGuidObject? buildingGuidObject=null, System.Nullable<int> countyId=null);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.Reference(DiGi.Analytical.Building.Classes.BuildingModel,DiGi.Analytical.Building.Interfaces.IBuildingGuidObject,System.Nullable_int_).buildingModel'></a>

`buildingModel` [DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')

The analytical building model to create the reference for\.

<a name='DiGi.GIS.PostgreSQL.Create.Reference(DiGi.Analytical.Building.Classes.BuildingModel,DiGi.Analytical.Building.Interfaces.IBuildingGuidObject,System.Nullable_int_).buildingGuidObject'></a>

`buildingGuidObject` [DiGi\.Analytical\.Building\.Interfaces\.IBuildingGuidObject](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.interfaces.ibuildingguidobject 'DiGi\.Analytical\.Building\.Interfaces\.IBuildingGuidObject')

An optional specific building element \(e\.g\. component, space\) to include as the innermost reference in the chain\.

<a name='DiGi.GIS.PostgreSQL.Create.Reference(DiGi.Analytical.Building.Classes.BuildingModel,DiGi.Analytical.Building.Interfaces.IBuildingGuidObject,System.Nullable_int_).countyId'></a>

`countyId` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

An optional county identifier that anchors the reference to an [DiGi\.GIS\.Classes\.AdministrativeDivision](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.administrativedivision 'DiGi\.GIS\.Classes\.AdministrativeDivision') at the outermost level of the chain\.

#### Returns
[DiGi\.Core\.Interfaces\.IReference](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.ireference 'DiGi\.Core\.Interfaces\.IReference')  
An [DiGi\.Core\.Interfaces\.IReference](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.ireference 'DiGi\.Core\.Interfaces\.IReference') representing the containment chain, or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') if [buildingModel](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Create.Reference(DiGi.Analytical.Building.Classes.BuildingModel,DiGi.Analytical.Building.Interfaces.IBuildingGuidObject,System.Nullable_int_).buildingModel 'DiGi\.GIS\.PostgreSQL\.Create\.Reference\(DiGi\.Analytical\.Building\.Classes\.BuildingModel, DiGi\.Analytical\.Building\.Interfaces\.IBuildingGuidObject, System\.Nullable\<int\>\)\.buildingModel') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AddressPoint(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_AddressPoint\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the [AddressPoint](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.AddressPoint 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.AddressPoint') table and its indexes, together with the `pg_trgm` extension its search index needs\.

One row is one PRG address point. `reference_id` (the PRG local identifier) is the only unique key, so a re-import of a newer version replaces the row rather than appending one, and a point that now falls in a sibling county part moves with it. The table is therefore not partitioned: a partitioned table would have to put `county_id` into that key. `county_id` is the county <b>part</b> id, never a county code (`Coding - GIS Administrative Data.md`).

`search_text` is normalised in C# by [SearchText\(string\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.SearchText(string) 'DiGi\.GIS\.PostgreSQL\.Query\.SearchText\(string\)'), at import and at query time alike, and indexed with a GIN trigram index. `unaccent()` is not `IMMUTABLE`, so an index cannot be built on it, and a leading-wildcard `ILIKE` over it scans the whole table. Every statement is idempotent.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_AddressPoint(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AddressPoint(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AddressPoint(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AddressPoint(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table exists afterwards; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AdministrativeArea2D(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_AdministrativeArea2D\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the AdministrativeArea2D table in the PostgreSQL database\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_AdministrativeArea2D(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AdministrativeArea2D(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AdministrativeArea2D(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AdministrativeArea2D(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AdministrativeArea2DReferencedObject(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_AdministrativeArea2DReferencedObject\(this NpgsqlConnection, string, int, CancellationToken\) Method

Asynchronously creates the AdministrativeArea2DReferencedObject table for the specified table name\.

`reference` is what every read of this table filters on, so it carries an index of its own. `unique_id` needs none: the `UNIQUE` constraint on it is already an index, and a second one on the same column would only cost storage and write time.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_AdministrativeArea2DReferencedObject(this Npgsql.NpgsqlConnection? npgsqlConnection, string tableName, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AdministrativeArea2DReferencedObject(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The PostgreSQL connection instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AdministrativeArea2DReferencedObject(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).tableName'></a>

`tableName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the table associated with the administrative area 2D referenced object\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AdministrativeArea2DReferencedObject(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_AdministrativeArea2DReferencedObject(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_Building\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the partitioned [Building](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building 'DiGi\.GIS\.PostgreSQL\.Classes\.Building') table along with its supporting composite index, if it does not already exist\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_Building(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2D(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_Building2D\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the Building2D table in the PostgreSQL database\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_Building2D(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2D(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The Npgsql connection instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2D(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2D(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReference(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_Building2DReference\(this NpgsqlConnection, string, int, CancellationToken\) Method

Asynchronously creates the Building 2D reference table in the PostgreSQL database\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_Building2DReference(this Npgsql.NpgsqlConnection? npgsqlConnection, string? tableName, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReference(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The PostgreSQL connection instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReference(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).tableName'></a>

`tableName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the table to be created for Building 2D references\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReference(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReference(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used by other methods as a token for cancelling the asynchronous operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_Building2DReferencedObject\(this NpgsqlConnection, string, int, CancellationToken\) Method

Asynchronously creates the Building 2D Referenced Object table for the specified table name\.

The two constraints carry the addressing convention described on [Building2DReferencedObject&lt;TUniqueObject&gt;](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DReferencedObject_TUniqueObject_ 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DReferencedObject\<TUniqueObject\>'). `UNIQUE (county_id, unique_id)` makes one <b>stored object</b> the unit of a row, and the absence of any constraint on `(county_id, reference)` is deliberate: a building may hold several rows here, so writes append rather than replace.

Do not add a unique constraint on `(county_id, reference)` to stop the table growing on re-runs. It would reduce the table to one row per building and discard every record after the first. The plain index created on that pair is not a constraint and places no such restriction on what may be stored.

Indexes: `(county_id, reference)` is the primary access path and every read filters on it, so it carries an index. `(county_id, unique_id)` carries none of its own, because the `UNIQUE` constraint is already an index on exactly those columns in that order.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_Building2DReferencedObject(this Npgsql.NpgsqlConnection? npgsqlConnection, string tableName, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).tableName'></a>

`tableName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String') representing the name of the table to be created\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject(thisNpgsql.NpgsqlConnection,string,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A [System\.Threading\.Tasks\.Task&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1') that represents the asynchronous operation\. The task result is a [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean') value indicating whether the table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject_Partition(thisNpgsql.NpgsqlConnection,string,int,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_Building2DReferencedObject\_Partition\(this NpgsqlConnection, string, int, int, CancellationToken\) Method

Asynchronously creates a partition for the Building2DReferencedObject table based on the specified table name and county identifier\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_Building2DReferencedObject_Partition(this Npgsql.NpgsqlConnection? npgsqlConnection, string tableName, int countyId, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject_Partition(thisNpgsql.NpgsqlConnection,string,int,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The PostgreSQL connection instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject_Partition(thisNpgsql.NpgsqlConnection,string,int,int,System.Threading.CancellationToken).tableName'></a>

`tableName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the parent table that is being partitioned\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject_Partition(thisNpgsql.NpgsqlConnection,string,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The integer identifier of the county for which the partition is created\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject_Partition(thisNpgsql.NpgsqlConnection,string,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2DReferencedObject_Partition(thisNpgsql.NpgsqlConnection,string,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the partition was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2D_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_Building2D\_Partition\(this NpgsqlConnection, int, int, CancellationToken\) Method

Asynchronously creates a partition for the Building2D table associated with the specified county identifier\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_Building2D_Partition(this Npgsql.NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2D_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The PostgreSQL connection instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2D_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The integer identifier of the county for which the partition is being created\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2D_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building2D_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the Building2D partition was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_BuildingSolarRadiation(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_BuildingSolarRadiation\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the [BuildingSolarRadiation](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.BuildingSolarRadiation 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.BuildingSolarRadiation') table and its indexes, partitioned by county part\.

One row holds one building's [DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult'): the `object` column carries it with every surface's shading factors removed, and `shading_factors` carries those factors packed into one `bytea` ([ToSystem\_Bytes\(this IEnumerable&lt;byte\[\]&gt;\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Convert.ToSystem_Bytes(thisSystem.Collections.Generic.IEnumerable_byte[]_) 'DiGi\.GIS\.PostgreSQL\.Convert\.ToSystem\_Bytes\(this System\.Collections\.Generic\.IEnumerable\<byte\[\]\>\)')), compressed with lz4. Keeping the factors out of the JSONB avoids a third more for base64 and lets lz4 collapse the night-time zeros.

The key `(county_id, building_model_id, radius, solver_version)` is unique and every column of it is NOT NULL, so `NULLS NOT DISTINCT` is not needed. The weather columns describe the stored summary and are not part of the key. Every statement is idempotent.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_BuildingSolarRadiation(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_BuildingSolarRadiation(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_BuildingSolarRadiation(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_BuildingSolarRadiation(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table exists afterwards; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_BuildingSolarRadiation_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_BuildingSolarRadiation\_Partition\(this NpgsqlConnection, int, int, CancellationToken\) Method

Asynchronously creates the partition of the [BuildingSolarRadiation](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.BuildingSolarRadiation 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.BuildingSolarRadiation') table for one county part\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_BuildingSolarRadiation_Partition(this Npgsql.NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_BuildingSolarRadiation_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_BuildingSolarRadiation_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The id of the county part the partition holds\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_BuildingSolarRadiation_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_BuildingSolarRadiation_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the partition exists afterwards; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_Building\_Partition\(this NpgsqlConnection, int, int, CancellationToken\) Method

Asynchronously creates a partition for the [Building](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building 'DiGi\.GIS\.PostgreSQL\.Classes\.Building') table based on the specified county identifier\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_Building_Partition(this Npgsql.NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The unique identifier of the county for which the partition is being created\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Building_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the partition was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_EPWFile(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_EPWFile\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the epw\_file table in the PostgreSQL database\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_EPWFile(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_EPWFile(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_EPWFile(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_EPWFile(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_OrtoDatas(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_OrtoDatas\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the OrtoDatas table in the PostgreSQL database\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_OrtoDatas(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_OrtoDatas(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_OrtoDatas(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_OrtoDatas(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the OrtoDatas table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_OrtoDatas_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_OrtoDatas\_Partition\(this NpgsqlConnection, int, int, CancellationToken\) Method

Asynchronously creates a partition for the OrtoDatas table based on the specified county identifier\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_OrtoDatas_Partition(this Npgsql.NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_OrtoDatas_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The PostgreSQL connection instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_OrtoDatas_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The unique identifier of the county for which the partition is being created\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_OrtoDatas_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_OrtoDatas_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the partition was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_StatisticalDataCollection(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_StatisticalDataCollection\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the [StatisticalDataCollection](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.StatisticalDataCollection 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.StatisticalDataCollection') table in the PostgreSQL database\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_StatisticalDataCollection(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_StatisticalDataCollection(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_StatisticalDataCollection(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_StatisticalDataCollection(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_TerrainPoint(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_TerrainPoint\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the partitioned [TerrainPoint](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.TerrainPoint 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.TerrainPoint') table along with its supporting indexes in the PostgreSQL database\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_TerrainPoint(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_TerrainPoint(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_TerrainPoint(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_TerrainPoint(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_TerrainPoint_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_TerrainPoint\_Partition\(this NpgsqlConnection, int, int, CancellationToken\) Method

Asynchronously creates a partition for the [TerrainPoint](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.TerrainPoint 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.TerrainPoint') table based on the specified county identifier\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_TerrainPoint_Partition(this Npgsql.NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_TerrainPoint_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_TerrainPoint_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The integer identifier of the county for which the partition is created\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_TerrainPoint_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_TerrainPoint_Partition(thisNpgsql.NpgsqlConnection,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the partition was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Unit(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken)'></a>

## Create\.TableAsync\_Unit\(this NpgsqlConnection, int, CancellationToken\) Method

Asynchronously creates the [Unit](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.Unit 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.Unit') table along with its supporting indexes in the PostgreSQL database\.

```csharp
public static System.Threading.Tasks.Task<bool> TableAsync_Unit(this Npgsql.NpgsqlConnection? npgsqlConnection, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Unit(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') instance used to execute the command\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Unit(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Create.TableAsync_Unit(thisNpgsql.NpgsqlConnection,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to monitor for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result is true if the table was created successfully; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Create.TerrainPointDensityResult(int,long,double,System.Nullable_double_)'></a>

## Create\.TerrainPointDensityResult\(int, long, double, Nullable\<double\>\) Method

Works out how densely a county partition of the terrain point table is sampled and returns it as a [TerrainPointDensityResult\(int, long, double, Nullable&lt;double&gt;\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Create.TerrainPointDensityResult(int,long,double,System.Nullable_double_) 'DiGi\.GIS\.PostgreSQL\.Create\.TerrainPointDensityResult\(int, long, double, System\.Nullable\<double\>\)')\.

Every figure the result carries beyond the count and the area is derived here rather than by the constructor, which assigns and nothing more.

A figure that cannot be derived is left null rather than filled with a not-a-number. Strict JSON has no token for one, so a not-a-number reaching a response body is a serialization failure rather than a value a reader can act on.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.TerrainPointDensityResult? TerrainPointDensityResult(int countyId, long count, double area, System.Nullable<double> gridSize=null);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TerrainPointDensityResult(int,long,double,System.Nullable_double_).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county partition\.

<a name='DiGi.GIS.PostgreSQL.Create.TerrainPointDensityResult(int,long,double,System.Nullable_double_).count'></a>

`count` [System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

The number of points stored for the county\.

<a name='DiGi.GIS.PostgreSQL.Create.TerrainPointDensityResult(int,long,double,System.Nullable_double_).area'></a>

`area` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The area the points were meant to cover, in square model units \- summed from the county's subdivisions, which is what a sampling run tests its points against\.

<a name='DiGi.GIS.PostgreSQL.Create.TerrainPointDensityResult(int,long,double,System.Nullable_double_).gridSize'></a>

`gridSize` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

The spacing a sampling run used, when it is known\. Supplying it is what fills in [ExpectedDensity](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPointDensityResult.ExpectedDensity 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPointDensityResult\.ExpectedDensity') and [Completeness](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPointDensityResult.Completeness 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPointDensityResult\.Completeness')\.

#### Returns
[TerrainPointDensityResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.TerrainPointDensityResult 'DiGi\.GIS\.PostgreSQL\.Classes\.TerrainPointDensityResult')  
The [TerrainPointDensityResult\(int, long, double, Nullable&lt;double&gt;\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Create.TerrainPointDensityResult(int,long,double,System.Nullable_double_) 'DiGi\.GIS\.PostgreSQL\.Create\.TerrainPointDensityResult\(int, long, double, System\.Nullable\<double\>\)'), or null when the count is negative or the area is not a usable measurement\.

<a name='DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken)'></a>

## Create\.TypologyAsync\(this BuildingDataPostgreSQLConverter, ColumnTypologyFilter\<Column\>, IEnumerable\<int\>, Column, TypologyItem, bool, int, int, CancellationToken\) Method

Asynchronously classifies the building data of the given county partitions into a typology tree, grouping it by the chained columns of a column typology filter\.

The partitions to read are named explicitly and never defaulted to all of them. A county identifier addresses one polygon part rather than a county - there are 406 parts for 380 counties - and the table holds tens of thousands of rows per part, so classifying the whole country is millions of rows in memory rather than a larger query. Grouping by county name rather than county identifier is what re-merges the parts of one county into a single node.

Only the columns the chain needs are projected, plus the reference and county identifier the pull adds itself. One connection is opened for the whole run and every partition is paged over it, because the overload opening its own connection would open one per page.

Each partition is read in physical order ([PullByPhysicalOrderAsync\(NpgsqlConnection, int, IEnumerable&lt;string&gt;, string, int, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter.PullByPhysicalOrderAsync(Npgsql.NpgsqlConnection,int,System.Collections.Generic.IEnumerable_string_,string,int,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataPostgreSQLConverter\.PullByPhysicalOrderAsync\(Npgsql\.NpgsqlConnection, int, System\.Collections\.Generic\.IEnumerable\<string\>, string, int, int, System\.Threading\.CancellationToken\)')), sequentially rather than one random heap read per row in reference order. On a server older than PostgreSQL 14 the reference keyset is used instead. The whole read runs in one `REPEATABLE READ` transaction, so rows written by another session during the read neither repeat nor go missing, and the result is exactly the partitions as they were when the read began. Rows are still deduplicated on the primary key (the reference within each county part), whichever column [column\_Reference](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken).column_Reference 'DiGi\.GIS\.PostgreSQL\.Create\.TypologyAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataPostgreSQLConverter, DiGi\.Typology\.Classes\.ColumnTypologyFilter\<DiGi\.Core\.IO\.Table\.Classes\.Column\>, System\.Collections\.Generic\.IEnumerable\<int\>, DiGi\.Core\.IO\.Table\.Classes\.Column, DiGi\.Typology\.Classes\.TypologyItem, bool, int, int, System\.Threading\.CancellationToken\)\.column\_Reference') names. The rows arrive in physical rather than reference order; the classification does not depend on row order, but the order of the references stored on a node follows it.

Rows are classified by [DiGi\.GIS\.Create\.Typology\(DiGi\.Core\.IO\.Table\.Classes\.Table,DiGi\.Typology\.Classes\.ColumnTypologyFilter\{DiGi\.Core\.IO\.Table\.Classes\.Column\},DiGi\.Core\.IO\.Table\.Classes\.Column,DiGi\.Typology\.Classes\.TypologyItem,System\.Boolean\)](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.create.typology#digi-gis-create-typology(digi-core-io-table-classes-table-digi-typology-classes-columntypologyfilter{digi-core-io-table-classes-column}-digi-core-io-table-classes-column-digi-typology-classes-typologyitem-system-boolean) 'DiGi\.GIS\.Create\.Typology\(DiGi\.Core\.IO\.Table\.Classes\.Table,DiGi\.Typology\.Classes\.ColumnTypologyFilter\{DiGi\.Core\.IO\.Table\.Classes\.Column\},DiGi\.Core\.IO\.Table\.Classes\.Column,DiGi\.Typology\.Classes\.TypologyItem,System\.Boolean\)'), whose remarks describe which rows a level excludes.

```csharp
public static System.Threading.Tasks.Task<DiGi.Typology.Classes.Typology?> TypologyAsync(this DiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter, DiGi.Typology.Classes.ColumnTypologyFilter<DiGi.Core.IO.Table.Classes.Column>? columnTypologyFilter, System.Collections.Generic.IEnumerable<int>? countyIds, DiGi.Core.IO.Table.Classes.Column? column_Reference=null, DiGi.Typology.Classes.TypologyItem? typologyItem_Root=null, bool includeReferences=true, int pageSize=5000, int commandTimeout=600, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken).buildingDataPostgreSQLConverter'></a>

`buildingDataPostgreSQLConverter` [BuildingDataPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataPostgreSQLConverter')

The converter reading the building data\.

<a name='DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken).columnTypologyFilter'></a>

`columnTypologyFilter` [DiGi\.Typology\.Classes\.ColumnTypologyFilter&lt;](https://learn.microsoft.com/en-us/dotnet/api/digi.typology.classes.columntypologyfilter-1 'DiGi\.Typology\.Classes\.ColumnTypologyFilter\`1')[DiGi\.Core\.IO\.Table\.Classes\.Column](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.column 'DiGi\.Core\.IO\.Table\.Classes\.Column')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/digi.typology.classes.columntypologyfilter-1 'DiGi\.Typology\.Classes\.ColumnTypologyFilter\`1')

The root of the filter chain describing the grouping levels\.

<a name='DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The identifiers of the county partitions to read\. One identifier is one polygon part\.

<a name='DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken).column_Reference'></a>

`column_Reference` [DiGi\.Core\.IO\.Table\.Classes\.Column](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.column 'DiGi\.Core\.IO\.Table\.Classes\.Column')

The column identifying a row\. Defaults to the shared reference column\.

<a name='DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken).typologyItem_Root'></a>

`typologyItem_Root` [DiGi\.Typology\.Classes\.TypologyItem](https://learn.microsoft.com/en-us/dotnet/api/digi.typology.classes.typologyitem 'DiGi\.Typology\.Classes\.TypologyItem')

The item naming the root node\. When null the root is left unnamed\.

<a name='DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken).includeReferences'></a>

`includeReferences` [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

A value indicating whether the identified references are stored on the nodes\.

<a name='DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken).pageSize'></a>

`pageSize` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The number of rows read per page\.

<a name='DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of the command\. A value of 0 disables the timeout\. Defaults to 600 seconds\.

<a name='DiGi.GIS.PostgreSQL.Create.TypologyAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.Typology.Classes.ColumnTypologyFilter_DiGi.Core.IO.Table.Classes.Column_,System.Collections.Generic.IEnumerable_int_,DiGi.Core.IO.Table.Classes.Column,DiGi.Typology.Classes.TypologyItem,bool,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[DiGi\.Typology\.Classes\.Typology](https://learn.microsoft.com/en-us/dotnet/api/digi.typology.classes.typology 'DiGi\.Typology\.Classes\.Typology')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result contains the solved typology, or null when the converter, the chain or the partition list is missing, when a page could not be read \(including a non\-empty page without the reference column\), or when the partitions hold no rows at all\.

<a name='DiGi.GIS.PostgreSQL.Create.UnitComplianceResultAsync(thisDiGi.GIS.PostgreSQL.Classes.UnitPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken)'></a>

## Create\.UnitComplianceResultAsync\(this UnitPostgreSQLConverter, AdministrativeAreal2DPostgreSQLConverter, AdministrativeArealType, int, CancellationToken\) Method

Asynchronously evaluates the matching compliance of administrative area references of the specified type against BDL territorial units\.

```csharp
public static System.Threading.Tasks.Task<DiGi.GIS.PostgreSQL.Classes.UnitComplianceResult?> UnitComplianceResultAsync(this DiGi.GIS.PostgreSQL.Classes.UnitPostgreSQLConverter? unitPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter, DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType administrativeArealType, int commandTimeout=60, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Create.UnitComplianceResultAsync(thisDiGi.GIS.PostgreSQL.Classes.UnitPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).unitPostgreSQLConverter'></a>

`unitPostgreSQLConverter` [UnitPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.UnitPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.UnitPostgreSQLConverter')

The converter used to access stored BDL unit data and extract the statistical unit hierarchy\.

<a name='DiGi.GIS.PostgreSQL.Create.UnitComplianceResultAsync(thisDiGi.GIS.PostgreSQL.Classes.UnitPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).administrativeAreal2DPostgreSQLConverter'></a>

`administrativeAreal2DPostgreSQLConverter` [AdministrativeAreal2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter')

The converter used to retrieve administrative area references\.

<a name='DiGi.GIS.PostgreSQL.Create.UnitComplianceResultAsync(thisDiGi.GIS.PostgreSQL.Classes.UnitPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).administrativeArealType'></a>

`administrativeArealType` [AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')

The administrative area type to evaluate\.

<a name='DiGi.GIS.PostgreSQL.Create.UnitComplianceResultAsync(thisDiGi.GIS.PostgreSQL.Classes.UnitPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for database commands\.

<a name='DiGi.GIS.PostgreSQL.Create.UnitComplianceResultAsync(thisDiGi.GIS.PostgreSQL.Classes.UnitPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[UnitComplianceResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.UnitComplianceResult 'DiGi\.GIS\.PostgreSQL\.Classes\.UnitComplianceResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result contains the [UnitComplianceResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.UnitComplianceResult 'DiGi\.GIS\.PostgreSQL\.Classes\.UnitComplianceResult') if successful; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Modify'></a>

## Modify Class

```csharp
public static class Modify
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → Modify
### Methods

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken)'></a>

## Modify\.RefreshCountyIdsAsync\(this NpgsqlConnection, string, IEnumerable\<string\>, int, IEnumerable\<int\>, int, int, CancellationToken\) Method

Asynchronously moves the rows keyed on the given building references onto [countyId](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken).countyId 'DiGi\.GIS\.PostgreSQL\.Modify\.RefreshCountyIdsAsync\(this Npgsql\.NpgsqlConnection, string, System\.Collections\.Generic\.IEnumerable\<string\>, int, System\.Collections\.Generic\.IEnumerable\<int\>, int, int, System\.Threading\.CancellationToken\)\.countyId'), in a table partitioned by county\.

A building belongs to the county polygon part its footprint lies in, and `building_2d` is where that is recorded. Everything else keyed on a building has to follow it: every read of these tables filters on `county_id` first, so a row left under the part the building came from answers nothing, for anyone, ever again. This is the move that keeps them together, and it is meant to be called for the same references immediately after `Building2DPostgreSQLConverter.RefreshCountyIdsAsync` has moved the buildings themselves.

`county_id` is the <b>partition key</b>, so this is a row movement between partitions rather than an ordinary column update. The destination partition is created first, and the identifiers of the rows are preserved.

<b>Nothing is deleted.</b> A row cannot move onto a destination that already holds the same key, and two rows carrying one key under two different parts cannot both arrive; such a row stays where it is and its reference is not reported, so a caller comparing the result against what it passed in learns what is left to settle by hand.

<b>Only three tables are accepted</b> - `building`, `building_data` and `orto_datas` - and their key columns are named here rather than taken from the caller, so no identifier in the statement below comes from outside this method. Every one of them is checked against the columns the table actually has before it reaches the database. The tables holding referenced objects are keyed on `unique_id` instead and are moved by `Building2DReferencedObjectPostgreSQLConverter.RefreshCountyIdsAsync`; `building_2d` has its own method on its own converter.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.HashSet<string>?> RefreshCountyIdsAsync(this Npgsql.NpgsqlConnection? npgsqlConnection, string? tableName, System.Collections.Generic.IEnumerable<string>? references, int countyId, System.Collections.Generic.IEnumerable<int>? countyIds_Source=null, int batchSize=1000, int commandTimeout=600, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken).npgsqlConnection'></a>

`npgsqlConnection` [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection')

The [Npgsql\.NpgsqlConnection](https://learn.microsoft.com/en-us/dotnet/api/npgsql.npgsqlconnection 'Npgsql\.NpgsqlConnection') used to connect to the PostgreSQL database\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken).tableName'></a>

`tableName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The table to move rows in\. One of `building`, `building_data` or `orto_datas`; anything else is refused\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The building references known to belong to [countyId](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken).countyId 'DiGi\.GIS\.PostgreSQL\.Modify\.RefreshCountyIdsAsync\(this Npgsql\.NpgsqlConnection, string, System\.Collections\.Generic\.IEnumerable\<string\>, int, System\.Collections\.Generic\.IEnumerable\<int\>, int, int, System\.Threading\.CancellationToken\)\.countyId')\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county polygon part the rows should be held under\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken).countyIds_Source'></a>

`countyIds_Source` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The parts the rows may currently sit under, normally the other parts of the same county code\. When null every part is searched, which cannot be pruned to a partition and reads the whole table\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken).batchSize'></a>

`batchSize` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The number of references sent in one statement\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshCountyIdsAsync(thisNpgsql.NpgsqlConnection,string,System.Collections.Generic.IEnumerable_string_,int,System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result contains the references that had at least one row moved, an empty set when the table does not exist, or null when the connection was null, no references were given, or the table is not one of the three this handles\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshOrtoDatasAsync(thisDiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager,DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshOptions,System.IProgress_long_,System.Threading.CancellationToken)'></a>

## Modify\.RefreshOrtoDatasAsync\(this GISPostgreSQLConverterManager, PostgreSQLOrtoDatasRefreshOptions, IProgress\<long\>, CancellationToken\) Method

Asynchronously queues the orthophoto downloads the given counties are short of\.

This writes no orthophoto data of its own. Each county's building references are read, the ones already stored are dropped unless [OverrideExisting](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshOptions.OverrideExisting 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLOrtoDatasRefreshOptions\.OverrideExisting') says otherwise, and the rest are appended to the queue the download task drains. What comes back is therefore work scheduled, not work done.

With [UpdateSubdivisionIds](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshOptions.UpdateSubdivisionIds 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLOrtoDatasRefreshOptions\.UpdateSubdivisionIds') set, a second and unrelated thing happens first: each building's own subdivision identifier is pushed onto its stored row. A building whose subdivision has not been resolved is skipped rather than clearing the stored one.

A county that fails is logged with the exception that stopped it and stepped over, so one unreachable partition cannot cost the run the counties behind it. The counties are visited in ascending order, so a run started again covers them in the same order as the one it replaces.

```csharp
public static System.Threading.Tasks.Task<DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshResult?> RefreshOrtoDatasAsync(this DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager? gISPostgreSQLConverterManager, DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshOptions? postgreSQLOrtoDatasRefreshOptions=null, System.IProgress<long>? progress=null, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshOrtoDatasAsync(thisDiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager,DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshOptions,System.IProgress_long_,System.Threading.CancellationToken).gISPostgreSQLConverterManager'></a>

`gISPostgreSQLConverterManager` [GISPostgreSQLConverterManager](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager 'DiGi\.GIS\.PostgreSQL\.Classes\.GISPostgreSQLConverterManager')

The manager used to retrieve the necessary PostgreSQL converters\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshOrtoDatasAsync(thisDiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager,DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshOptions,System.IProgress_long_,System.Threading.CancellationToken).postgreSQLOrtoDatasRefreshOptions'></a>

`postgreSQLOrtoDatasRefreshOptions` [PostgreSQLOrtoDatasRefreshOptions](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshOptions 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLOrtoDatasRefreshOptions')

The options specifying how the orthophoto queue should be refreshed\. Null uses the defaults\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshOrtoDatasAsync(thisDiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager,DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshOptions,System.IProgress_long_,System.Threading.CancellationToken).progress'></a>

`progress` [System\.IProgress&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1 'System\.IProgress\`1')[System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1 'System\.IProgress\`1')

An optional progress reporter carrying the running total of references the queue has accepted\.

<a name='DiGi.GIS.PostgreSQL.Modify.RefreshOrtoDatasAsync(thisDiGi.GIS.PostgreSQL.Classes.GISPostgreSQLConverterManager,DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshOptions,System.IProgress_long_,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[PostgreSQLOrtoDatasRefreshResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.PostgreSQLOrtoDatasRefreshResult 'DiGi\.GIS\.PostgreSQL\.Classes\.PostgreSQLOrtoDatasRefreshResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result carries what the run queued and what it stepped over, or null when the run could not be attempted at all \- a converter missing, or the counties unreadable\.

<a name='DiGi.GIS.PostgreSQL.Modify.ResetIds(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D)'></a>

## Modify\.ResetIds\(this AdministrativeAreal2D\) Method

Resets the administrative identifiers of the specified administrative areal 2D object to null\.

```csharp
public static bool ResetIds(this DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D? administrativeAreal2D);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.ResetIds(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D).administrativeAreal2D'></a>

`administrativeAreal2D` [AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')

The administrative areal 2D object whose identifiers are to be reset\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the identifiers were successfully reset; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Modify.SetId(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D)'></a>

## Modify\.SetId\(this AdministrativeAreal2D, AdministrativeAreal2D\) Method

Sets the appropriate identifier on the destination administrative areal object based on the source's administrative areal type\.

```csharp
public static bool SetId(this DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D? administrativeAreal2D_Destination, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D? administrativeAreal2D_Source);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.SetId(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D).administrativeAreal2D_Destination'></a>

`administrativeAreal2D_Destination` [AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')

The destination administrative areal object where the ID will be set\.

<a name='DiGi.GIS.PostgreSQL.Modify.SetId(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D).administrativeAreal2D_Source'></a>

`administrativeAreal2D_Source` [AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')

The source administrative areal object providing the ID and type\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the ID was successfully set; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken)'></a>

## Modify\.UpdateAddressPointsAsync\(this AddressPointPostgreSQLConverter, Building2DPostgreSQLConverter, AdministrativeAreal2DPostgreSQLConverter, IEnumerable\<AddressPoint\>, IEnumerable\<int\>, double, bool, int, int, CancellationToken\) Method

Imports the PRG address points of one county into `address_point`: assigns every point to the county part it lies in, matches the points of each part to that part's `building_2d` footprints with [DiGi\.GIS\.Create\.AddressPointMatchResult\(System\.Collections\.Generic\.IEnumerable\{DiGi\.GIS\.Classes\.AddressPoint\},System\.Collections\.Generic\.IEnumerable\{DiGi\.GIS\.Classes\.Building2D\},System\.Double,System\.Double\)](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.create.addresspointmatchresult#digi-gis-create-addresspointmatchresult(system-collections-generic-ienumerable{digi-gis-classes-addresspoint}-system-collections-generic-ienumerable{digi-gis-classes-building2d}-system-double-system-double) 'DiGi\.GIS\.Create\.AddressPointMatchResult\(System\.Collections\.Generic\.IEnumerable\{DiGi\.GIS\.Classes\.AddressPoint\},System\.Collections\.Generic\.IEnumerable\{DiGi\.GIS\.Classes\.Building2D\},System\.Double,System\.Double\)'), and stores the matches\.

[countyIds](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).countyIds 'DiGi\.GIS\.PostgreSQL\.Modify\.UpdateAddressPointsAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.AddressPointPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.Classes\.AddressPoint\>, System\.Collections\.Generic\.IEnumerable\<int\>, double, bool, int, int, System\.Threading\.CancellationToken\)\.countyIds') are county part ids, normally every polygon part of one county, never a county code: 18 counties have disconnected territory stored as one row per part (`Coding - GIS Administrative Data.md`). A point is assigned to the part whose face contains it ([IdsByPoint2Ds\(this IDictionary&lt;int,PolygonalFace2D&gt;, IReadOnlyList&lt;Point2D&gt;, double\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.IdsByPoint2Ds(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,System.Collections.Generic.IReadOnlyList_DiGi.Geometry.Planar.Classes.Point2D_,double) 'DiGi\.GIS\.PostgreSQL\.Query\.IdsByPoint2Ds\(this System\.Collections\.Generic\.IDictionary\<int,DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D\>, System\.Collections\.Generic\.IReadOnlyList\<DiGi\.Geometry\.Planar\.Classes\.Point2D\>, double\)'), lowest id on overlap); a point in none of the parts is, when [retryOutside](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).retryOutside 'DiGi\.GIS\.PostgreSQL\.Modify\.UpdateAddressPointsAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.AddressPointPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.Classes\.AddressPoint\>, System\.Collections\.Generic\.IEnumerable\<int\>, double, bool, int, int, System\.Threading\.CancellationToken\)\.retryOutside') is true, retried against the parts of other counties containing it and filed under the part it lies in ([Rescued](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AddressPointUpdateResult.Rescued 'DiGi\.GIS\.PostgreSQL\.Classes\.AddressPointUpdateResult\.Rescued')), because the commune code of a point can name a county whose boundary differs from the BDOT10k polygon its building is filed under (DiGi.GIS.PostgreSQL#109); only a point in no county part is counted as outside and not written.

Known limit: a point is matched only against the buildings of its own part, so a point within [maxDistance](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).maxDistance 'DiGi\.GIS\.PostgreSQL\.Modify\.UpdateAddressPointsAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.AddressPointPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.Classes\.AddressPoint\>, System\.Collections\.Generic\.IEnumerable\<int\>, double, bool, int, int, System\.Threading\.CancellationToken\)\.maxDistance') of a building filed under a sibling part, across the part boundary, is matched to its own part's nearest building or left unmatched.

Known limit: PRG places many points on a plot, road or verge rather than on a building, so a share of every county stays [DiGi\.GIS\.Enums\.AddressPointMatchType\.Unmatched](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.enums.addresspointmatchtype.unmatched 'DiGi\.GIS\.Enums\.AddressPointMatchType\.Unmatched') (8.5 % nationally, 5 % to 11.5 % per voivodeship, up to 36 % in a few counties; DiGi.GIS.PostgreSQL#111, #113). In the sampled worst counties the far points lie on empty plots, fields or roads, or 25 m to 60 m from a mapped building; BDOT10k coverage did not explain them, and it is not reported upstream. The 10 m [maxDistance](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).maxDistance 'DiGi\.GIS\.PostgreSQL\.Modify\.UpdateAddressPointsAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.AddressPointPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.Classes\.AddressPoint\>, System\.Collections\.Generic\.IEnumerable\<int\>, double, bool, int, int, System\.Threading\.CancellationToken\)\.maxDistance') is kept (a larger value risks matching a neighbour's building in towns, DiGi.GIS.PostgreSQL#114).

Re-running an import replaces each point's row unless the stored PRG version is newer ([UpdateAsync\(NpgsqlConnection, int, IEnumerable&lt;AddressPointMatch&gt;, int, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection,int,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPointMatch_,int,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.AddressPointPostgreSQLConverter\.UpdateAsync\(Npgsql\.NpgsqlConnection, int, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.Classes\.AddressPointMatch\>, int, int, System\.Threading\.CancellationToken\)')). All three converters read and write the Main database.

```csharp
public static System.Threading.Tasks.Task<DiGi.GIS.PostgreSQL.Classes.AddressPointUpdateResult?> UpdateAddressPointsAsync(this DiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter? addressPointPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter? building2DPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter, System.Collections.Generic.IEnumerable<DiGi.GIS.Classes.AddressPoint?>? addressPoints, System.Collections.Generic.IEnumerable<int>? countyIds, double maxDistance=10.0, bool retryOutside=true, int batchSize=1000, int commandTimeout=600, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).addressPointPostgreSQLConverter'></a>

`addressPointPostgreSQLConverter` [AddressPointPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AddressPointPostgreSQLConverter')

The converter of the address point table\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).building2DPostgreSQLConverter'></a>

`building2DPostgreSQLConverter` [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')

The converter the building footprints of each part are read through\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).administrativeAreal2DPostgreSQLConverter'></a>

`administrativeAreal2DPostgreSQLConverter` [AdministrativeAreal2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter')

The converter the county part polygons are read through\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).addressPoints'></a>

`addressPoints` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[DiGi\.GIS\.Classes\.AddressPoint](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.addresspoint 'DiGi\.GIS\.Classes\.AddressPoint')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The address points of the county\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The county part ids of the county\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).maxDistance'></a>

`maxDistance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The distance, in m, within which a point outside every footprint is assigned to the nearest one\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).retryOutside'></a>

`retryOutside` [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

True to retry a point that lies in none of [countyIds](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).countyIds 'DiGi\.GIS\.PostgreSQL\.Modify\.UpdateAddressPointsAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.AddressPointPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.Classes\.AddressPoint\>, System\.Collections\.Generic\.IEnumerable\<int\>, double, bool, int, int, System\.Threading\.CancellationToken\)\.countyIds') against the county parts of other counties that contain it; false to count it as outside\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).batchSize'></a>

`batchSize` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The number of rows written per statement\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateAddressPointsAsync(thisDiGi.GIS.PostgreSQL.Classes.AddressPointPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,System.Collections.Generic.IEnumerable_int_,double,bool,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[AddressPointUpdateResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AddressPointUpdateResult 'DiGi\.GIS\.PostgreSQL\.Classes\.AddressPointUpdateResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result counts the points by outcome and the rows written, or is null when a converter, the points or the parts are missing, the parts have no readable polygon, or a read or the write could not run\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateBuildingDataYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,int,System.Threading.CancellationToken)'></a>

## Modify\.UpdateBuildingDataYearBuiltAsync\(this BuildingDataPostgreSQLConverter, YearBuiltDataPostgreSQLConverter, Building2DPostgreSQLConverter, IEnumerable\<int\>, IEnumerable\<string\>, int, int, CancellationToken\) Method

Recomputes the three derived year built columns of `building_data` \- predicted, user and calculated \- from the stored year built history of explicit county parts, writing NULL where the history no longer holds a value\.

The columns are derived, so after a prediction run or a user entry is removed they are stale until something derives them again. The building data run cannot clear them: it emits no row for a building with no value left. This names every building in scope through `IO.Modify.Update_Building2D_YearBuilt`'s references parameter instead, so a building whose history is now empty is written with three unset cells and `PushAsync` stores NULL in each.

The buildings in scope are [references](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Modify.UpdateBuildingDataYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,int,System.Threading.CancellationToken).references 'DiGi\.GIS\.PostgreSQL\.Modify\.UpdateBuildingDataYearBuiltAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.YearBuiltDataPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<int\>, System\.Collections\.Generic\.IEnumerable\<string\>, int, int, System\.Threading\.CancellationToken\)\.references'), or, when it is null, every building holding a `year_built_data` row or a `building_data` row under the parts. The history is read from the main database and the rows from the storage one - no join crosses them. Each building is written under the part its `building_data` row is filed under; one with no such row is resolved through `building_2d` and written only when its history holds a value, so a recompute never adds empty rows.

```csharp
public static System.Threading.Tasks.Task<DiGi.GIS.PostgreSQL.Classes.BuildingDataYearBuiltUpdateResult?> UpdateBuildingDataYearBuiltAsync(this DiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter? building2DPostgreSQLConverter, System.Collections.Generic.IEnumerable<int>? countyIds, System.Collections.Generic.IEnumerable<string>? references, int batchSize=1000, int commandTimeout=600, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateBuildingDataYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,int,System.Threading.CancellationToken).buildingDataPostgreSQLConverter'></a>

`buildingDataPostgreSQLConverter` [BuildingDataPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataPostgreSQLConverter')

The converter of the building data table, in the storage database\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateBuildingDataYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,int,System.Threading.CancellationToken).yearBuiltDataPostgreSQLConverter'></a>

`yearBuiltDataPostgreSQLConverter` [YearBuiltDataPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.YearBuiltDataPostgreSQLConverter')

The converter of the stored year built history, in the main database\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateBuildingDataYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,int,System.Threading.CancellationToken).building2DPostgreSQLConverter'></a>

`building2DPostgreSQLConverter` [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')

The converter used to resolve the part of a building that holds no building data row yet\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateBuildingDataYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,int,System.Threading.CancellationToken).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The county parts in scope\. Normally every polygon part of one county\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateBuildingDataYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The references of the buildings to recompute, or null for every building of the parts\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateBuildingDataYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,int,System.Threading.CancellationToken).batchSize'></a>

`batchSize` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The number of references read per statement and rows written per batch\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateBuildingDataYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateBuildingDataYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.BuildingDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[BuildingDataYearBuiltUpdateResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingDataYearBuiltUpdateResult 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingDataYearBuiltUpdateResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result counts the buildings considered, written with a value and cleared, or is null when a converter or the parts are missing, or a read or the write could not run\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateIds(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D)'></a>

## Modify\.UpdateIds\(this AdministrativeAreal2D, AdministrativeAreal2D\) Method

Updates the identification properties of the destination administrative areal object using values from the source object\.

```csharp
public static bool UpdateIds(this DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D? administrativeAreal2D_Destination, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D? administrativeAreal2D_Source);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateIds(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D).administrativeAreal2D_Destination'></a>

`administrativeAreal2D_Destination` [AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')

The destination AdministrativeAreal2D object to be updated\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateIds(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D).administrativeAreal2D_Source'></a>

`administrativeAreal2D_Source` [AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')

The source AdministrativeAreal2D object containing the new identification values\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the IDs were successfully updated; otherwise, false if either the destination or source object is null\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateIds(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_,double)'></a>

## Modify\.UpdateIds\(this AdministrativeAreal2D, IEnumerable\<AdministrativeAreal2D\>, double\) Method

Finds the parent of the destination administrative areal object among the given sources by geometry, and updates its identification properties from that parent\.

The sources are expected to be one administrative level, and all of it - the search is by geometry, not by an existing identifier. A source is chosen by containment of a sample point taken from the destination (its bounding box centroid, falling back to an internal point of its polygon); where several sources contain that point, the smallest of them wins as the most specific.

When no source contains the point, the destination is assigned to the source covering the <b>majority of its own area</b> instead. The BDOT10k settlement layer (`OT_ADMS_A`) and the administrative-division layer (`OT_ADJA_A`) are digitised independently, so a handful of settlements sit just outside every municipality polygon and their sample point lands in a gap. Requiring a majority - rather than any overlap - leaves a destination whose level genuinely holds no parent unassigned, so the caller can search the next level up instead of filing it under a neighbour it merely shares a border with. Full account: https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/14.

```csharp
public static bool UpdateIds(this DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D? administrativeAreal2D_Destination, System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D>? administrativeAreal2Ds_Source, double tolerance=0.001);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateIds(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_,double).administrativeAreal2D_Destination'></a>

`administrativeAreal2D_Destination` [AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')

The destination AdministrativeAreal2D object to be updated\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateIds(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_,double).administrativeAreal2Ds_Source'></a>

`administrativeAreal2Ds_Source` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The candidate source AdministrativeAreal2D objects, one administrative level, to be searched for a parent\.

<a name='DiGi.GIS.PostgreSQL.Modify.UpdateIds(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_,double).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The distance tolerance used by the containment checks\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if a parent was found and the IDs were updated; otherwise, false\.

<a name='DiGi.GIS.PostgreSQL.Modify.Update_ExternalComponentsArea(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.BuildingModel_)'></a>

## Modify\.Update\_ExternalComponentsArea\(this Table, IEnumerable\<BuildingModel\>\) Method

Classifies the components of the stored [DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel')s into the 35 “External Components Area” columns of the given table, keyed by county identifier and reference\.

A wall is filed under the sector of its outward normal’s azimuth, a roof under its tilt band (flat below 5°, then [5°, 20°], (20°, 45°] and above 45°) crossed with the same sectors, and a floor under the floor column; the total column is the sum of the 34 breakdowns. A building that arrives with a stored model gets a row in which every empty bucket is 0, so a zero row and an absent row stay distinguishable.

The wall outward and roof upward normals are the normals of the faces of the model’s external envelope, built by [DiGi\.Analytical\.Building\.Classes\.BuildingModel\.GetExternalShell\(System\.Nullable\{DiGi\.Geometry\.Core\.Enums\.Side\},System\.Nullable\{DiGi\.Geometry\.Core\.Enums\.Orientation\},System\.Nullable\{DiGi\.Geometry\.Core\.Enums\.Orientation\},System\.Double\)](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel.getexternalshell#digi-analytical-building-classes-buildingmodel-getexternalshell(system-nullable{digi-geometry-core-enums-side}-system-nullable{digi-geometry-core-enums-orientation}-system-nullable{digi-geometry-core-enums-orientation}-system-double) 'DiGi\.Analytical\.Building\.Classes\.BuildingModel\.GetExternalShell\(System\.Nullable\{DiGi\.Geometry\.Core\.Enums\.Side\},System\.Nullable\{DiGi\.Geometry\.Core\.Enums\.Orientation\},System\.Nullable\{DiGi\.Geometry\.Core\.Enums\.Orientation\},System\.Double\)') with [DiGi\.Geometry\.Core\.Enums\.Side\.External](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.core.enums.side.external 'DiGi\.Geometry\.Core\.Enums\.Side\.External'), so each face direction is resolved once over the envelope instead of being guessed from the component’s stored geometry. That method states the selection rule for every consumer: a component bounding exactly one space is external, one bounding two is an internal partition, one bounding none is part of no envelope. Every envelope face carries the [DiGi\.Core\.Interfaces\.IUniqueReference](https://learn.microsoft.com/en-us/dotnet/api/digi.core.interfaces.iuniquereference 'DiGi\.Core\.Interfaces\.IUniqueReference') of the component it was built from, which is how a face is matched back to its component; a component the envelope carries is classified from its face, one it does not carry is excluded from the external area and counted in the result when it bounds two spaces, and is a defect in the model’s space structure otherwise - a component bounding one space that the envelope still leaves out has no polygonal face.

A model that carries components but no envelope at all - fewer than the four external faces a closed solid needs, typically a sliver footprint with walls and no roof or floor - is degenerate: nothing gives its components an outward normal, so it gets no row and its reference is listed in [DegenerateReferences](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.ExternalComponentsAreaResult.DegenerateReferences 'DiGi\.GIS\.PostgreSQL\.Classes\.ExternalComponentsAreaResult\.DegenerateReferences'). A model refused as a defect gets no row either and is listed with its reason in [FailedReferences](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.ExternalComponentsAreaResult.FailedReferences 'DiGi\.GIS\.PostgreSQL\.Classes\.ExternalComponentsAreaResult\.FailedReferences'); neither stops the classification of the other models, and neither leaves a partial row, because a row is only built once its model is classified.

The finest tolerance at which the envelope edge-pairs into a closed surface is recorded beside the areas - the closing tolerance column, null when the envelope closes at no rung of the ladder 1e-6 to 0.2 m or the model carries no external components. Ray parity is sound only for a closed face set, so a null closing tolerance is the signal that the row’s sector and tilt values may rest on an arbitrary face side; such a model is counted in the result, not failed.

A component the method cannot classify - a wall whose normal is vertical, so its azimuth is undefined - is skipped and counted in the result.

A reference can arrive several times (several stored versions of the model); the first record of a given county and reference is the one that is written and the rest are stepped over, so the collection has to reach this method in the caller’s order of preference - the converter returns the newest record first.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.ExternalComponentsAreaResult Update_ExternalComponentsArea(this DiGi.Core.IO.Table.Classes.Table? table, System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.BuildingModel>? buildingModels);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.Update_ExternalComponentsArea(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.BuildingModel_).table'></a>

`table` [DiGi\.Core\.IO\.Table\.Classes\.Table](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.table 'DiGi\.Core\.IO\.Table\.Classes\.Table')

The table to fill with the classification\.

<a name='DiGi.GIS.PostgreSQL.Modify.Update_ExternalComponentsArea(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.BuildingModel_).buildingModels'></a>

`buildingModels` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[BuildingModel](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.BuildingModel 'DiGi\.GIS\.PostgreSQL\.Classes\.BuildingModel')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The envelopes of stored building models, most preferred record first\.

#### Returns
[ExternalComponentsAreaResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.ExternalComponentsAreaResult 'DiGi\.GIS\.PostgreSQL\.Classes\.ExternalComponentsAreaResult')  
The outcome of the classification: the number of components skipped because they bound two spaces or their target bucket is undefined, the number of models whose external envelope does not close on the tolerance ladder, and the references of the degenerate and the refused models \- the counts 0 and the lists empty when every component was classified over a closed envelope\. The tallies cover the written rows only\.

<a name='DiGi.GIS.PostgreSQL.Modify.Update_Id(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DReference_)'></a>

## Modify\.Update\_Id\(this Table, IEnumerable\<Building2DReference\>\) Method

Updates the Id column of the table based on the provided building2DReferences\. If a matching row is found \(based on CountyId and Reference\), it updates the Id value\. If no matching row is found, it adds a new row with the CountyId, Reference, and Id values\.

```csharp
public static void Update_Id(this DiGi.Core.IO.Table.Classes.Table? table, System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.Building2DReference>? building2DReferences);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.Update_Id(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DReference_).table'></a>

`table` [DiGi\.Core\.IO\.Table\.Classes\.Table](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.table 'DiGi\.Core\.IO\.Table\.Classes\.Table')

The table to update

<a name='DiGi.GIS.PostgreSQL.Modify.Update_Id(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DReference_).building2DReferences'></a>

`building2DReferences` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[Building2DReference](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DReference 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DReference')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The building2DReferences to use for updating

<a name='DiGi.GIS.PostgreSQL.Modify.Update_Occupancy(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyData_)'></a>

## Modify\.Update\_Occupancy\(this Table, IEnumerable\<Building2DOccupancyData\>\) Method

Updates the occupancy data in the specified table based on the provided collection of building occupancy records\.

The occupancy table holds one row per stored object rather than one per building, so a reference can arrive here several times. The first record of a given county and reference is the one that is written and the rest are stepped over, which means the collection has to reach this method in the caller's order of preference - the converter returns the newest record first, so passing its result straight through stores the newest.

```csharp
public static void Update_Occupancy(this DiGi.Core.IO.Table.Classes.Table? table, System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyData>? building2DOccupancyDatas);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.Update_Occupancy(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyData_).table'></a>

`table` [DiGi\.Core\.IO\.Table\.Classes\.Table](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.table 'DiGi\.Core\.IO\.Table\.Classes\.Table')

The PostgreSQL table to be updated\.

<a name='DiGi.GIS.PostgreSQL.Modify.Update_Occupancy(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyData_).building2DOccupancyDatas'></a>

`building2DOccupancyDatas` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[Building2DOccupancyData](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyData 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DOccupancyData')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

A collection of [Building2DOccupancyData](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DOccupancyData 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DOccupancyData') objects containing the new occupancy information, most preferred record first\.

<a name='DiGi.GIS.PostgreSQL.Modify.Update_SolarRadiation(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.BuildingSolarRadiationResult_)'></a>

## Modify\.Update\_SolarRadiation\(this Table, IEnumerable\<BuildingSolarRadiationResult\>\) Method

Aggregates the yearly solar radiation of the stored [DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult')s into the 25 “Solar radiation” columns of the given table, keyed by county identifier and reference\.

Irradiation is the area-weighted average over the building’s usable surfaces (a finite area greater than zero); energy is their sum. A surface is filed under the cardinal sector of its outward normal’s azimuth, in the same eight sectors as the external components area columns. The first record of a given county and reference is the one written and the rest are stepped over, so the collection has to reach this method in the caller’s order of preference - the converter returns the newest first.

A surface with no usable area is skipped and counted. A usable surface with no classifiable facing direction - a missing or vertical outward normal, typically a flat roof - counts toward the building totals but toward no direction sector. A direction with no usable surface is left null, and a building with no usable surface is left null rather than zeroed.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.SolarRadiationSummaryResult Update_SolarRadiation(this DiGi.Core.IO.Table.Classes.Table? table, System.Collections.Generic.IEnumerable<DiGi.GIS.Classes.BuildingSolarRadiationResult>? results);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Modify.Update_SolarRadiation(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.BuildingSolarRadiationResult_).table'></a>

`table` [DiGi\.Core\.IO\.Table\.Classes\.Table](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.table 'DiGi\.Core\.IO\.Table\.Classes\.Table')

The table to fill with the yearly solar radiation summary\.

<a name='DiGi.GIS.PostgreSQL.Modify.Update_SolarRadiation(thisDiGi.Core.IO.Table.Classes.Table,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.BuildingSolarRadiationResult_).results'></a>

`results` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[DiGi\.GIS\.Classes\.BuildingSolarRadiationResult](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.buildingsolarradiationresult 'DiGi\.GIS\.Classes\.BuildingSolarRadiationResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The stored solar radiation results, most preferred record of a county and reference first\.

#### Returns
[SolarRadiationSummaryResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.SolarRadiationSummaryResult 'DiGi\.GIS\.PostgreSQL\.Classes\.SolarRadiationSummaryResult')  
The outcome of the aggregation: the buildings written and the surfaces aggregated, plus the ones skipped or without a classifiable sector\.

<a name='DiGi.GIS.PostgreSQL.Query'></a>

## Query Class

```csharp
public static class Query
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → Query
### Methods

<a name='DiGi.GIS.PostgreSQL.Query.AddressPointsByCountyId(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,double)'></a>

## Query\.AddressPointsByCountyId\(this IDictionary\<int,PolygonalFace2D\>, IEnumerable\<AddressPoint\>, double\) Method

Files each address point under the county part whose face contains it, without touching the database\.

A point that lies in none of the faces is returned separately rather than filed anywhere, so a caller can retry it against the faces of other counties (`Modify.UpdateAddressPointsAsync`). The decision is the one [IdsByPoint2Ds\(this IDictionary&lt;int,PolygonalFace2D&gt;, IReadOnlyList&lt;Point2D&gt;, double\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.IdsByPoint2Ds(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,System.Collections.Generic.IReadOnlyList_DiGi.Geometry.Planar.Classes.Point2D_,double) 'DiGi\.GIS\.PostgreSQL\.Query\.IdsByPoint2Ds\(this System\.Collections\.Generic\.IDictionary\<int,DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D\>, System\.Collections\.Generic\.IReadOnlyList\<DiGi\.Geometry\.Planar\.Classes\.Point2D\>, double\)') makes: lowest identifier wins where faces overlap.

```csharp
public static System.Nullable<(System.Collections.Generic.Dictionary<int,System.Collections.Generic.List<DiGi.GIS.Classes.AddressPoint>> AddressPoints_ByCountyId,System.Collections.Generic.List<DiGi.GIS.Classes.AddressPoint> AddressPoints_Outside)> AddressPointsByCountyId(this System.Collections.Generic.IDictionary<int,DiGi.Geometry.Planar.Classes.PolygonalFace2D>? polygonalFace2Ds_ById, System.Collections.Generic.IEnumerable<DiGi.GIS.Classes.AddressPoint?>? addressPoints, double tolerance=0.001);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.AddressPointsByCountyId(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,double).polygonalFace2Ds_ById'></a>

`polygonalFace2Ds_ById` [System\.Collections\.Generic\.IDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2 'System\.Collections\.Generic\.IDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2 'System\.Collections\.Generic\.IDictionary\`2')[DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.polygonalface2d 'DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2 'System\.Collections\.Generic\.IDictionary\`2')

The county part faces to decide against, keyed by county part id\.

<a name='DiGi.GIS.PostgreSQL.Query.AddressPointsByCountyId(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,double).addressPoints'></a>

`addressPoints` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[DiGi\.GIS\.Classes\.AddressPoint](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.addresspoint 'DiGi\.GIS\.Classes\.AddressPoint')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The address points to file\. Points without a location are returned as outside\.

<a name='DiGi.GIS.PostgreSQL.Query.AddressPointsByCountyId(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,System.Collections.Generic.IEnumerable_DiGi.GIS.Classes.AddressPoint_,double).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The distance a point may lie outside a face and still be counted as within it\.

#### Returns
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Classes\.AddressPoint](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.addresspoint 'DiGi\.GIS\.Classes\.AddressPoint')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DiGi\.GIS\.Classes\.AddressPoint](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.addresspoint 'DiGi\.GIS\.Classes\.AddressPoint')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')  
The points filed under the county part id containing them, and the points lying in none of the faces; or null when an argument is null\.

<a name='DiGi.GIS.PostgreSQL.Query.AdministrativeArealType(thisDiGi.GIS.Classes.AdministrativeAreal2D)'></a>

## Query\.AdministrativeArealType\(this AdministrativeAreal2D\) Method

Determines the administrative areal type based on the provided administrative areal object\.

```csharp
public static DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType AdministrativeArealType(this DiGi.GIS.Classes.AdministrativeAreal2D? administrativeAreal2D);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.AdministrativeArealType(thisDiGi.GIS.Classes.AdministrativeAreal2D).administrativeAreal2D'></a>

`administrativeAreal2D` [DiGi\.GIS\.Classes\.AdministrativeAreal2D](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.administrativeareal2d 'DiGi\.GIS\.Classes\.AdministrativeAreal2D')

The administrative areal object to evaluate\.

#### Returns
[AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')  
The corresponding [AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')\.

<a name='DiGi.GIS.PostgreSQL.Query.AdministrativeArealType(thisDiGi.GIS.Enums.AdministrativeDivisionType)'></a>

## Query\.AdministrativeArealType\(this AdministrativeDivisionType\) Method

Maps a GIS administrative division type to the PostgreSQL administrative areal type\.

```csharp
public static DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType AdministrativeArealType(this DiGi.GIS.Enums.AdministrativeDivisionType administrativeDivisionType);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.AdministrativeArealType(thisDiGi.GIS.Enums.AdministrativeDivisionType).administrativeDivisionType'></a>

`administrativeDivisionType` [DiGi\.GIS\.Enums\.AdministrativeDivisionType](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.enums.administrativedivisiontype 'DiGi\.GIS\.Enums\.AdministrativeDivisionType')

The GIS administrative division type\.

#### Returns
[AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')  
The corresponding [AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')\.

<a name='DiGi.GIS.PostgreSQL.Query.AdministrativeCodeKey(string,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType)'></a>

## Query\.AdministrativeCodeKey\(string, AdministrativeArealType\) Method

Gets the leading slice of an administrative code that names its ancestor at the given administrative areal type\.

A code carries the whole chain above it: the first 2 characters name the voivodeship, the first 4 the county, and the first 6 the municipality. The 7th character of a municipality code is the gmina <i>type</i> digit - a town and the rural area of one urban-rural gmina carry `4` and `5` against the gmina's own `3` - so the municipality slice is 6 characters, not the whole code. Verified against the stored table: every one of the 100 354 subdivision rows carries a 7-character code whose 4-character prefix is an existing county code, and 99.72% of the rows that already resolved agree with their municipality at 6 characters.

Returns [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') for [Country](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType.Country 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType\.Country'), which has no such relation - every country row's code is `10`, which is also the voivodeship code of łódzkie, so slicing 2 characters there would match one voivodeship's chain rather than the country. A caller that gets [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') has no code constraint to apply and must fall back to searching the whole level.

<b>A code is not a key.</b> 18 county codes and 64 municipality codes name several rows, one per polygon part, so a slice identifies a <i>set</i> of candidate rows and geometry still has to choose between them. See https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/1.

```csharp
public static string? AdministrativeCodeKey(string? code, DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType administrativeArealType);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.AdministrativeCodeKey(string,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).code'></a>

`code` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The administrative code to slice\.

<a name='DiGi.GIS.PostgreSQL.Query.AdministrativeCodeKey(string,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).administrativeArealType'></a>

`administrativeArealType` [AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')

The administrative areal type of the ancestor being named\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The leading slice of [code](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.AdministrativeCodeKey(string,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).code 'DiGi\.GIS\.PostgreSQL\.Query\.AdministrativeCodeKey\(string, DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType\)\.code') naming the ancestor, or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') when the type has no code relation or the code is too short to reach it\.

<a name='DiGi.GIS.PostgreSQL.Query.Building(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building_,DiGi.Geometry.Spatial.Classes.Point3D,double)'></a>

## Query\.Building\(this IEnumerable\<Building\>, Point3D, double\) Method

Selects the single most relevant [Building](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building 'DiGi\.GIS\.PostgreSQL\.Classes\.Building') from a collection of candidates\.

Candidates are ranked ascending by level of detail and then by year, with nulls treated as the lowest rank, and only the candidates sharing the highest rank are considered.

When more than one candidate shares the highest rank and a point is provided, the candidate whose surface geometry is closest to that point wins; candidates without usable geometry are excluded from that comparison.

```csharp
public static DiGi.GIS.PostgreSQL.Classes.Building? Building(this System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.Building>? buildings, DiGi.Geometry.Spatial.Classes.Point3D? point3D, double tolerance=0.001);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.Building(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building_,DiGi.Geometry.Spatial.Classes.Point3D,double).buildings'></a>

`buildings` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[Building](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building 'DiGi\.GIS\.PostgreSQL\.Classes\.Building')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The collection of [Building](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building 'DiGi\.GIS\.PostgreSQL\.Classes\.Building') candidates to choose from\.

<a name='DiGi.GIS.PostgreSQL.Query.Building(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building_,DiGi.Geometry.Spatial.Classes.Point3D,double).point3D'></a>

`point3D` [DiGi\.Geometry\.Spatial\.Classes\.Point3D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.spatial.classes.point3d 'DiGi\.Geometry\.Spatial\.Classes\.Point3D')

The optional [DiGi\.Geometry\.Spatial\.Classes\.Point3D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.spatial.classes.point3d 'DiGi\.Geometry\.Spatial\.Classes\.Point3D') used to break ties between candidates of equal rank\.

<a name='DiGi.GIS.PostgreSQL.Query.Building(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building_,DiGi.Geometry.Spatial.Classes.Point3D,double).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The tolerance used for the closest point calculation\.

#### Returns
[Building](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building 'DiGi\.GIS\.PostgreSQL\.Classes\.Building')  
The most relevant [Building](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building 'DiGi\.GIS\.PostgreSQL\.Classes\.Building'), or null if the collection is null, empty or no candidate could be resolved\.

<a name='DiGi.GIS.PostgreSQL.Query.ChildAdministrativeArealType(thisDiGi.GIS.PostgreSQL.Enums.AdministrativeArealType)'></a>

## Query\.ChildAdministrativeArealType\(this AdministrativeArealType\) Method

Gets the child administrative areal type for the specified administrative areal type based on the administrative hierarchy\.

```csharp
public static System.Nullable<DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType> ChildAdministrativeArealType(this DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType administrativeArealType);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.ChildAdministrativeArealType(thisDiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).administrativeArealType'></a>

`administrativeArealType` [AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')

The current administrative areal type\.

#### Returns
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')  
The next level of administrative areal type in the hierarchy, or null if no child exists or the input is undefined\.

<a name='DiGi.GIS.PostgreSQL.Query.ContainerIds(thisSystem.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double)'></a>

## Query\.ContainerIds\(this IReadOnlyDictionary\<int,PolygonalFace2D\>, double\) Method

Finds, for every face, the other faces that contain it \- the nesting of a layer whose areas lie inside one another, as a city holds its districts and their neighbourhoods\.

A face is contained by another when the other is larger, its bounding box holds this face's box within [tolerance](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.ContainerIds(thisSystem.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double).tolerance 'DiGi\.GIS\.PostgreSQL\.Query\.ContainerIds\(this System\.Collections\.Generic\.IReadOnlyDictionary\<int,DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D\>, double\)\.tolerance'), and a point known to be inside this face ([DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D\.GetInternalPoint\(System\.Double\)](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.polygonalface2d.getinternalpoint#digi-geometry-planar-classes-polygonalface2d-getinternalpoint(system-double) 'DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D\.GetInternalPoint\(System\.Double\)')) is inside the other by the [IsInside\(PolygonalFace2D, BoundingBox2D, Point2D, double\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter.IsInside(DiGi.Geometry.Planar.Classes.PolygonalFace2D,DiGi.Geometry.Planar.Classes.BoundingBox2D,DiGi.Geometry.Planar.Classes.Point2D,double) 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter\.IsInside\(DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D, DiGi\.Geometry\.Planar\.Classes\.BoundingBox2D, DiGi\.Geometry\.Planar\.Classes\.Point2D, double\)') rule. One interior point rather than a polygon intersection: the areas of one layer do not cross, they nest or they touch, and a point settles which in constant time where an intersection would cost the product of the two rings.

The containers of a face are listed <b>smallest first</b>, so the first is its immediate parent and the last is the outermost. A face with no container is top level; a layer that does not nest at all - every flat county - answers an empty list for every face. That is what the municipality occupancy roll-up sums over, and what makes a nested city count once rather than once per level ([DiGi\.GIS\.PostgreSQL\#77](https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/77 'https://github\.com/DiGi\-Organization/DiGi\.GIS\.PostgreSQL/issues/77')).

Boxes, areas and interior points are derived once up front. A face whose box or interior point cannot be derived neither contains nor is contained.

```csharp
public static System.Collections.Generic.Dictionary<int,System.Collections.Generic.List<int>> ContainerIds(this System.Collections.Generic.IReadOnlyDictionary<int,DiGi.Geometry.Planar.Classes.PolygonalFace2D>? polygonalFace2Ds_ById, double tolerance=0.001);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.ContainerIds(thisSystem.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double).polygonalFace2Ds_ById'></a>

`polygonalFace2Ds_ById` [System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.polygonalface2d 'DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')

The faces of one layer, keyed by identifier\. May be [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

<a name='DiGi.GIS.PostgreSQL.Query.ContainerIds(thisSystem.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The distance within which a box edge still counts as inside the container's box, and the tolerance of the interior point test\.

#### Returns
[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')  
Every identifier given mapped to the identifiers of the faces containing it, smallest first; an empty list for a top\-level face\. Empty when nothing was given\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyId(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Interfaces.IPolygonal2D_,DiGi.Geometry.Planar.Interfaces.IPolygonal2D,double)'></a>

## Query\.CountyId\(this IDictionary\<int,IPolygonal2D\>, IPolygonal2D, double\) Method

Picks which of the candidate county rows a 2D building belongs to, from parts whose polygons the caller has already converted\.

The decision is the one described on the [AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D') overload; only where the polygons come from differs. Deriving a part's polygon means deserializing the stored geometry, and a county polygon carries thousands of vertices, so a caller deciding many buildings against the same parts should convert once with [Polygonal2DsByCountyId\(this IEnumerable&lt;AdministrativeAreal2D&gt;\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.Polygonal2DsByCountyId(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_) 'DiGi\.GIS\.PostgreSQL\.Query\.Polygonal2DsByCountyId\(this System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D\>\)') and call this - the other overload converts every candidate again on every building.

```csharp
public static System.Nullable<int> CountyId(this System.Collections.Generic.IDictionary<int,DiGi.Geometry.Planar.Interfaces.IPolygonal2D>? polygonal2Ds_ByCountyId, DiGi.Geometry.Planar.Interfaces.IPolygonal2D? polygonal2D, double tolerance=0.001);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.CountyId(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Interfaces.IPolygonal2D_,DiGi.Geometry.Planar.Interfaces.IPolygonal2D,double).polygonal2Ds_ByCountyId'></a>

`polygonal2Ds_ByCountyId` [System\.Collections\.Generic\.IDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2 'System\.Collections\.Generic\.IDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2 'System\.Collections\.Generic\.IDictionary\`2')[DiGi\.Geometry\.Planar\.Interfaces\.IPolygonal2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.interfaces.ipolygonal2d 'DiGi\.Geometry\.Planar\.Interfaces\.IPolygonal2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2 'System\.Collections\.Generic\.IDictionary\`2')

The candidate parts, keyed by the identifier of their county row\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyId(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Interfaces.IPolygonal2D_,DiGi.Geometry.Planar.Interfaces.IPolygonal2D,double).polygonal2D'></a>

`polygonal2D` [DiGi\.Geometry\.Planar\.Interfaces\.IPolygonal2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.interfaces.ipolygonal2d 'DiGi\.Geometry\.Planar\.Interfaces\.IPolygonal2D')

The external edge of the building footprint\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyId(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Interfaces.IPolygonal2D_,DiGi.Geometry.Planar.Interfaces.IPolygonal2D,double).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The distance tolerance used for the containment and overlap tests\.

#### Returns
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')  
The identifier of the county row the building belongs to, or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') when it cannot be decided\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyId(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_,DiGi.Geometry.Planar.Interfaces.IPolygonal2D,double)'></a>

## Query\.CountyId\(this IEnumerable\<AdministrativeAreal2D\>, IPolygonal2D, double\) Method

Picks which of the candidate county rows a 2D building belongs to, by geometry\.

A county code names one row per polygon part, so a code can only narrow the field - this is what decides. Candidates are tried in three steps: the parts whose polygon the footprint lies in, else the nearest part, and where several parts contain it the one it overlaps most.

Where several parts cover the footprint <b>whole</b>, the smallest of them wins. Overlap cannot separate candidates that each hold every square metre of the building, so without this the answer would fall to the lowest identifier - a property of import order rather than of geography. The smallest is the most specific area containing it.

Every remaining comparison breaks ties on the row identifier, so two runs over the same building cannot disagree. Returns [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') when nothing can be decided - the caller is expected to leave such a building unwritten rather than file it under a guess.

```csharp
public static System.Nullable<int> CountyId(this System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D>? administrativeAreal2Ds, DiGi.Geometry.Planar.Interfaces.IPolygonal2D? polygonal2D, double tolerance=0.001);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.CountyId(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_,DiGi.Geometry.Planar.Interfaces.IPolygonal2D,double).administrativeAreal2Ds'></a>

`administrativeAreal2Ds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The candidate county rows\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyId(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_,DiGi.Geometry.Planar.Interfaces.IPolygonal2D,double).polygonal2D'></a>

`polygonal2D` [DiGi\.Geometry\.Planar\.Interfaces\.IPolygonal2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.interfaces.ipolygonal2d 'DiGi\.Geometry\.Planar\.Interfaces\.IPolygonal2D')

The external edge of the building footprint\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyId(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_,DiGi.Geometry.Planar.Interfaces.IPolygonal2D,double).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The distance tolerance used for the containment and overlap tests\.

#### Returns
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')  
The identifier of the county row the building belongs to, or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') when it cannot be decided\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdGroups(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference_,System.Collections.Generic.IReadOnlyDictionary_string,System.Collections.Generic.HashSet_int__)'></a>

## Query\.CountyIdGroups\(this IEnumerable\<AdministrativeAreal2DReference\>, IReadOnlyDictionary\<string,HashSet\<int\>\>\) Method

Orders county polygon parts into one group per county code, each group widened to every part the caller has looked up for its code\.

A county code is not a key - it names one row per polygon part - so a run driven per identifier would sample a multi-part county once per part. Driven per group instead, each code's territory is reached exactly once, and every point it produces can then be filed under the part containing it. The widening is handed in rather than looked up here: the parts of a code live in the database, and this method is pure.

Groups are ordered by their lowest identifier and each group is sorted ascending, so a run that is stopped and started again walks the counties in the same order and its progress means the same thing. A part with no usable code groups with itself.

```csharp
public static System.Collections.Generic.List<System.Collections.Generic.List<int>> CountyIdGroups(this System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference>? countyReferences, System.Collections.Generic.IReadOnlyDictionary<string,System.Collections.Generic.HashSet<int>>? countyIds_ByCode=null);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdGroups(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference_,System.Collections.Generic.IReadOnlyDictionary_string,System.Collections.Generic.HashSet_int__).countyReferences'></a>

`countyReferences` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[AdministrativeAreal2DReference](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The county parts the caller named\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdGroups(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference_,System.Collections.Generic.IReadOnlyDictionary_string,System.Collections.Generic.HashSet_int__).countyIds_ByCode'></a>

`countyIds_ByCode` [System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')

Every part of each code, as looked up from the database, keyed by code\. Only codes present among the references are widened; a code the dictionary does not name keeps the parts supplied with it\.

#### Returns
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')  
One group of identifiers per county code, ordered by lowest identifier, or an empty list when nothing was supplied\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdGroupsAsync(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken)'></a>

## Query\.CountyIdGroupsAsync\(this AdministrativeAreal2DPostgreSQLConverter, IEnumerable\<int\>, int, CancellationToken\) Method

Resolves county part identifiers into one group per county code, each group widened to every part the database holds for its code\.

A county code names one `administrative_areal_2d` row per polygon part, so a run driven per identifier would sample or repair a multi-part county once per part. The groups this returns are the scope a run walks instead: one group per code, each covering the whole county, so its territory is reached exactly once and every point it produces can be filed under the part containing it.

It lives here rather than in a host because it is the same question for every terrain caller: the sampling task, the gap-fill task and any future one have to mean the same thing by a county, and each answering it for itself is how a whole county came to be stored once per part in the first place.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.List<System.Collections.Generic.List<int>>?> CountyIdGroupsAsync(this DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter, System.Collections.Generic.IEnumerable<int>? countyIds, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdGroupsAsync(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).administrativeAreal2DPostgreSQLConverter'></a>

`administrativeAreal2DPostgreSQLConverter` [AdministrativeAreal2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter')

The converter used to read the parts and their codes\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdGroupsAsync(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The county part identifiers the run is scoped to\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdGroupsAsync(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of each command\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdGroupsAsync(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token that can be used by the caller to cancel the asynchronous operation\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
One sorted group of part identifiers per county code, ordered by lowest identifier, or null when the parts or their codes could not be read \- an unreadable scope must not silently narrow to the named parts and file a whole county under one of them\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken)'></a>

## Query\.CountyIdsByReferencesAsync\(this Building2DPostgreSQLConverter, IEnumerable\<string\>, IEnumerable\<int\>, int, CancellationToken\) Method

Reads which county row each reference belongs to, from the `building_2d` row that holds it\.

A county code names one `administrative_areal_2d` row per polygon part, so a code cannot say which part an item belongs to. The 2D building already answers that - it was filed by geometry when it was imported - and reading it back keeps every table keyed by the same `(county_id, reference)` pair. Filing a whole batch under one part instead is what left sibling parts reading back empty while the upload reported success.

The parts are probed in ascending order, one batched lookup each, and a reference is taken by the first part that holds it. A reference held by more than one part therefore resolves to the same one on every run.

A reference no part holds is simply absent from the result: nothing states where it belongs, and the caller decides whether to drop it or resolve it some other way.

The lookup not running at all is a different answer from running and resolving nothing. A null converter, or a part lookup that could not execute, answers `null` for the whole call: reading that as an empty map is what turned a broken connection into items silently dropped while the caller reported success.

It lives here rather than in a host because it is a question about `building_2d` and nothing else: the Web API, the desktop application and any background task all have to answer it the same way, and each answering it for itself is how a batch came to be filed under one part in the first place.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string,int>?> CountyIdsByReferencesAsync(this DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter? building2DPostgreSQLConverter, System.Collections.Generic.IEnumerable<string?>? references, System.Collections.Generic.IEnumerable<int>? countyIds, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).building2DPostgreSQLConverter'></a>

`building2DPostgreSQLConverter` [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')

The converter used to look the references up\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The references to resolve\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The candidate county rows, normally every polygon part of one code\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for each part lookup\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The identifier of the county row holding each resolved reference; a reference no part holds is absent from the result\. An empty map when nothing was asked or no part holds any reference; `null` when the lookup could not run \- never read `null` as nothing resolved\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesWithSiblingFallbackAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken)'></a>

## Query\.CountyIdsByReferencesWithSiblingFallbackAsync\(this Building2DPostgreSQLConverter, AdministrativeAreal2DPostgreSQLConverter, IEnumerable\<string\>, IEnumerable\<int\>, AdministrativeArealType, int, CancellationToken\) Method

Resolves each reference to the county part that holds its `building_2d` row, and \- when the caller named only a subset of a county's parts \- widens the unresolved ones to every part of the named code before giving up\.

[CountyIdsByReferencesAsync\(this Building2DPostgreSQLConverter, IEnumerable&lt;string&gt;, IEnumerable&lt;int&gt;, int, CancellationToken\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Query\.CountyIdsByReferencesAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<string\>, System\.Collections\.Generic\.IEnumerable\<int\>, int, System\.Threading\.CancellationToken\)') probes only the county rows the caller sent. That is the safe answer for a caller who named every part, but it is lossy for one who named a single part of a multi-part county: a datum whose 2D building is filed under a sibling part comes back unresolved and is left unwritten, even though the county the caller named does hold it.

This method keeps that first pass exactly, then adds one bounded widening. The references the first pass leaves unresolved are re-resolved against every part of the codes the candidate rows name, so a reference held by any part of a named county lands under that part. A reference held by no part of a named county stays unresolved - the widening never crosses into a county the caller did not name.

The [CountyIdsByReferencesAsync\(this Building2DPostgreSQLConverter, IEnumerable&lt;string&gt;, IEnumerable&lt;int&gt;, int, CancellationToken\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Query\.CountyIdsByReferencesAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<string\>, System\.Collections\.Generic\.IEnumerable\<int\>, int, System\.Threading\.CancellationToken\)') determinism contract is preserved: each pass probes its parts in ascending order and takes the first part that holds a reference, so a reference held by more than one part resolves to the same part on every run.

A lookup that could not run - the first pass, the widening, or the second pass - answers `null` for the whole call: a partial map cannot be told apart from a complete one over fewer parts. Degenerate input (no references, no candidate rows) answers an empty map, decided before the widening so that nothing being asked is never classified as a failure.

It lives next to [CountyIdsByReferencesAsync\(this Building2DPostgreSQLConverter, IEnumerable&lt;string&gt;, IEnumerable&lt;int&gt;, int, CancellationToken\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Query\.CountyIdsByReferencesAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<string\>, System\.Collections\.Generic\.IEnumerable\<int\>, int, System\.Threading\.CancellationToken\)') rather than in a host because the Web API, the desktop application and any background task all have to answer "which part of this county holds that reference" the same way; answering it per controller is how a batch came to be filed under one part in the first place. A caller who already named every part sees no difference - the first pass resolves everything, nothing is left to widen, and the second pass is not reached.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string,int>?> CountyIdsByReferencesWithSiblingFallbackAsync(this DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter? building2DPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter, System.Collections.Generic.IEnumerable<string?>? references, System.Collections.Generic.IEnumerable<int>? countyIds, DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType administrativeArealType=DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType.County, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesWithSiblingFallbackAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).building2DPostgreSQLConverter'></a>

`building2DPostgreSQLConverter` [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')

The converter used to look the references up in `building_2d`\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesWithSiblingFallbackAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).administrativeAreal2DPostgreSQLConverter'></a>

`administrativeAreal2DPostgreSQLConverter` [AdministrativeAreal2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter')

The converter used to widen the candidate rows to every part of their codes\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesWithSiblingFallbackAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).references'></a>

`references` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The references to resolve\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesWithSiblingFallbackAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The candidate county rows the caller named, normally one or more polygon parts of one county\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesWithSiblingFallbackAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).administrativeArealType'></a>

`administrativeArealType` [AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')

The level the candidate rows name; the widening reads the parts of their codes at this level only\. [County](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType.County 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType\.County') for the building\-keyed tables this serves\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesWithSiblingFallbackAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the lookups\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Query.CountyIdsByReferencesWithSiblingFallbackAsync(thisDiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_string_,System.Collections.Generic.IEnumerable_int_,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The identifier of the county row holding each resolved reference; a reference no part of a named county holds is absent from the result\. An empty map when nothing was asked or nothing resolved; `null` when any lookup could not run \- never read `null` as nothing resolved\.

<a name='DiGi.GIS.PostgreSQL.Query.CoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,System.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double,int,System.Threading.CancellationToken)'></a>

## Query\.CoveragesAsync\(this OrtoDatasPostgreSQLConverter, Building2DPostgreSQLConverter, int, IReadOnlyDictionary\<int,PolygonalFace2D\>, double, int, CancellationToken\) Method

Asynchronously measures, for one county, how much of the buildings inside each given polygon the orthophoto store holds\.

What the estimated partition counts cannot answer. Both tables are partitioned by `county_id`, so `reltuples` describes a whole county and there is no figure for any area inside it to be had from it - reporting the county's own factor for a subdivision is [DiGi\.GIS\.WebAPI issue \#8](https://github.com/DiGi-Organization/DiGi.GIS.WebAPI/issues/8 'https://github\.com/DiGi\-Organization/DiGi\.GIS\.WebAPI/issues/8'). This counts instead of estimating, and costs one read per side however many polygons are asked about.

<b>Membership is decided by geometry, not by the stored <c>subdivision_id</c>.</b> A building belongs to a polygon when its bounding-box centre lies strictly inside it - what [DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D\.Inside\(DiGi\.Geometry\.Planar\.Classes\.Point2D,System\.Double\)](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.polygonalface2d.inside#digi-geometry-planar-classes-polygonalface2d-inside(digi-geometry-planar-classes-point2d-system-double) 'DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D\.Inside\(DiGi\.Geometry\.Planar\.Classes\.Point2D,System\.Double\)') answers, asked through a [DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2DPointRelationSolver](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.polygonalface2dpointrelationsolver 'DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2DPointRelationSolver') built once per polygon, because a city outline of 4 000 vertices tested against 155 000 centres through the face itself took 52 seconds on the deployed host ([DiGi\.Geometry\#5](https://github.com/DiGi-Organization/DiGi.Geometry/issues/5 'https://github\.com/DiGi\-Organization/DiGi\.Geometry/issues/5')). The column files a building under one subdivision only, so where the subdivision layer nests it cannot say which buildings a district holds - Warsaw's districts counted zero by column while holding 155 307 buildings between them ([DiGi\.GIS\.PostgreSQL\#77](https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/77 'https://github\.com/DiGi\-Organization/DiGi\.GIS\.PostgreSQL/issues/77')). By polygon, a building inside a neighbourhood counts for the neighbourhood, its district and its city alike; <b>the results of nested polygons therefore overlap and must not be summed</b> - a caller wanting a municipality asks for the municipality's own polygon.

The orthophoto side's own `subdivision_id` is deliberately not used. That column has never been written: not one of the 8 384 055 rows stored across 225 counties carries a value, measured 2026-08-26 through `gis/ortodatas/summariesbycountyids`. The orthophoto side is asked only whether it holds a reference.

The two tables live in different databases - `building_2d` in the main store, `orto_datas` in the storage one - so this cannot be a join and is not one. The building side is read as bounding-box centres ([GetBuilding2DCentroidsByCountyIdAsync\(int, IEnumerable&lt;int&gt;, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter.GetBuilding2DCentroidsByCountyIdAsync(int,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter\.GetBuilding2DCentroidsByCountyIdAsync\(int, System\.Collections\.Generic\.IEnumerable\<int\>, int, System\.Threading\.CancellationToken\)'), the JSONB column untouched), the orthophoto side as references, and the two are matched in memory. Each centre is tested against every polygon.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Classes.OrtoDatasCoverageResult>?> CoveragesAsync(this DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter? ortoDatasPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter? building2DPostgreSQLConverter, int countyId, System.Collections.Generic.IReadOnlyDictionary<int,DiGi.Geometry.Planar.Classes.PolygonalFace2D>? polygonalFace2Ds_ById, double tolerance=0.001, int commandTimeout=600, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.CoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,System.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double,int,System.Threading.CancellationToken).ortoDatasPostgreSQLConverter'></a>

`ortoDatasPostgreSQLConverter` [OrtoDatasPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter')

The converter reading the orthophoto store\.

<a name='DiGi.GIS.PostgreSQL.Query.CoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,System.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double,int,System.Threading.CancellationToken).building2DPostgreSQLConverter'></a>

`building2DPostgreSQLConverter` [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')

The converter reading the building store\.

<a name='DiGi.GIS.PostgreSQL.Query.CoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,System.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county to measure\. One polygon part, not a code \- a multi\-part county is measured a part at a time\.

<a name='DiGi.GIS.PostgreSQL.Query.CoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,System.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double,int,System.Threading.CancellationToken).polygonalFace2Ds_ById'></a>

`polygonalFace2Ds_ById` [System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.polygonalface2d 'DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')

The polygons to measure, keyed by the identifier the results are reported under\.

<a name='DiGi.GIS.PostgreSQL.Query.CoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,System.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double,int,System.Threading.CancellationToken).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The distance tolerance of the containment test\.

<a name='DiGi.GIS.PostgreSQL.Query.CoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,System.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Query.CoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,System.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[OrtoDatasCoverageResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasCoverageResult 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasCoverageResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result contains one [OrtoDatasCoverageResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasCoverageResult 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasCoverageResult') per polygon that holds at least one of the county's buildings, in identifier order, plus one carrying a null [AdministrativeAreal2DId](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasCoverageResult.AdministrativeAreal2DId 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasCoverageResult\.AdministrativeAreal2DId') for the buildings inside none of them when there are any; or null when either converter or the polygon map is missing, either side could not be read, or the county holds no orthophoto row at all\.

<a name='DiGi.GIS.PostgreSQL.Query.IdColumnName(thisDiGi.GIS.PostgreSQL.Enums.AdministrativeArealType)'></a>

## Query\.IdColumnName\(this AdministrativeArealType\) Method

Gets the name of the database column that stores the identifier of an administrative area of the given type\.

This is the column to filter on when the ancestor being matched is known outright, which is not always the level directly above the rows being read - see [ParentIdColumnName\(this AdministrativeArealType\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.ParentIdColumnName(thisDiGi.GIS.PostgreSQL.Enums.AdministrativeArealType) 'DiGi\.GIS\.PostgreSQL\.Query\.ParentIdColumnName\(this DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType\)') for the relative form.

```csharp
public static string? IdColumnName(this DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType administrativeArealType);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.IdColumnName(thisDiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).administrativeArealType'></a>

`administrativeArealType` [AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')

The type of the administrative area whose identifier is stored\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The name of the column holding an identifier of that type, or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') when no column stores one \- nothing references a Subdivision\.

<a name='DiGi.GIS.PostgreSQL.Query.IdsByPoint2Ds(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,System.Collections.Generic.IReadOnlyList_DiGi.Geometry.Planar.Classes.Point2D_,double)'></a>

## Query\.IdsByPoint2Ds\(this IDictionary\<int,PolygonalFace2D\>, IReadOnlyList\<Point2D\>, double\) Method

Decides, for each point, which face contains it, without touching the database\.

The result has one entry per point, at the same position, holding null wherever a point lies in no face. That is an ordinary answer rather than a failure - a caller sampling a rectangle over an irregular area meets it at every corner.

Faces are bucketed into a uniform cell grid first, so a point is tested against the few faces near it rather than against all of them. Without that a run over a whole area costs the number of points times the number of faces, each test walking a ring of thousands of vertices.

Where faces overlap, the lowest identifier wins, so the same point decided twice gives the same answer.

```csharp
public static System.Nullable<int>[]? IdsByPoint2Ds(this System.Collections.Generic.IDictionary<int,DiGi.Geometry.Planar.Classes.PolygonalFace2D>? polygonalFace2Ds_ById, System.Collections.Generic.IReadOnlyList<DiGi.Geometry.Planar.Classes.Point2D>? point2Ds, double tolerance=0.001);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.IdsByPoint2Ds(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,System.Collections.Generic.IReadOnlyList_DiGi.Geometry.Planar.Classes.Point2D_,double).polygonalFace2Ds_ById'></a>

`polygonalFace2Ds_ById` [System\.Collections\.Generic\.IDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2 'System\.Collections\.Generic\.IDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2 'System\.Collections\.Generic\.IDictionary\`2')[DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.polygonalface2d 'DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2 'System\.Collections\.Generic\.IDictionary\`2')

The faces to decide against, keyed by identifier\.

<a name='DiGi.GIS.PostgreSQL.Query.IdsByPoint2Ds(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,System.Collections.Generic.IReadOnlyList_DiGi.Geometry.Planar.Classes.Point2D_,double).point2Ds'></a>

`point2Ds` [System\.Collections\.Generic\.IReadOnlyList&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1 'System\.Collections\.Generic\.IReadOnlyList\`1')[DiGi\.Geometry\.Planar\.Classes\.Point2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.point2d 'DiGi\.Geometry\.Planar\.Classes\.Point2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1 'System\.Collections\.Generic\.IReadOnlyList\`1')

The points to decide\.

<a name='DiGi.GIS.PostgreSQL.Query.IdsByPoint2Ds(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,System.Collections.Generic.IReadOnlyList_DiGi.Geometry.Planar.Classes.Point2D_,double).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The distance a point may lie outside a face and still be counted as within it\.

#### Returns
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')  
The identifier of the face containing each point, null at the position of every point that lies in none, or null when either argument is null\.

<a name='DiGi.GIS.PostgreSQL.Query.InScopeSubdivisionIds(System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference_,System.Collections.Generic.IReadOnlyDictionary_int,System.Collections.Generic.HashSet_int__)'></a>

## Query\.InScopeSubdivisionIds\(IEnumerable\<AdministrativeAreal2DReference\>, IReadOnlyDictionary\<int,HashSet\<int\>\>\) Method

Computes, for every county polygon part, the set of subdivision identifiers the subdivision loop reaches under it\.

The building data update walks subdivisions and reaches a building only when the building's county part sits in the sibling group of the subdivision's parent county. This is the exact set the final per-county pass must leave untouched: a building whose part does not sit in its subdivision's parent group is invisible to the loop, so only the fallback can write it.

A county code is not a key - it names one row per polygon part - so the in-scope set is keyed by part identifier, and a subdivision filed under one part is in scope for every part of that part's code group. A subdivision without a parent county is out of scope everywhere.

```csharp
public static System.Collections.Generic.Dictionary<int,System.Collections.Generic.HashSet<int>> InScopeSubdivisionIds(System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference>? subdivisions, System.Collections.Generic.IReadOnlyDictionary<int,System.Collections.Generic.HashSet<int>>? siblingCountyGroups);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.InScopeSubdivisionIds(System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference_,System.Collections.Generic.IReadOnlyDictionary_int,System.Collections.Generic.HashSet_int__).subdivisions'></a>

`subdivisions` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[AdministrativeAreal2DReference](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The subdivision references, each carrying an identifier and its parent county identifier\. May be [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

<a name='DiGi.GIS.PostgreSQL.Query.InScopeSubdivisionIds(System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference_,System.Collections.Generic.IReadOnlyDictionary_int,System.Collections.Generic.HashSet_int__).siblingCountyGroups'></a>

`siblingCountyGroups` [System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')

Each county part mapped to every part that shares its code\. May be [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null'), in which case each subdivision is in scope for its parent part only\.

#### Returns
[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')  
A map from each in\-scope county part to the subdivision identifiers the subdivision loop reaches under it\. Parts with no in\-scope subdivisions are absent from the map\.

<a name='DiGi.GIS.PostgreSQL.Query.IsInScope(int,string,System.Collections.Generic.ICollection_int_,System.Collections.Generic.ICollection_string_)'></a>

## Query\.IsInScope\(int, string, ICollection\<int\>, ICollection\<string\>\) Method

Decides whether a county row is in scope for a task that runs over a subset of the country\.

A county code is not a key - it names one row per polygon part - so scope is expressed two ways at once: by county row identifier, and by the two-digit voivodeship code a county code starts with. Both filters must admit the row, so a task can be pointed at one voivodeship, at a handful of parts, or at the parts of one voivodeship named by identifier.

A null filter admits everything, which is what makes a national pass the default. A row without a code cannot be placed in a voivodeship, so it is out of scope whenever [voivodeshipCodes](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.IsInScope(int,string,System.Collections.Generic.ICollection_int_,System.Collections.Generic.ICollection_string_).voivodeshipCodes 'DiGi\.GIS\.PostgreSQL\.Query\.IsInScope\(int, string, System\.Collections\.Generic\.ICollection\<int\>, System\.Collections\.Generic\.ICollection\<string\>\)\.voivodeshipCodes') is given.

```csharp
public static bool IsInScope(int countyId, string? code, System.Collections.Generic.ICollection<int>? countyIds, System.Collections.Generic.ICollection<string>? voivodeshipCodes);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.IsInScope(int,string,System.Collections.Generic.ICollection_int_,System.Collections.Generic.ICollection_string_).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county row\. A negative value is never in scope\.

<a name='DiGi.GIS.PostgreSQL.Query.IsInScope(int,string,System.Collections.Generic.ICollection_int_,System.Collections.Generic.ICollection_string_).code'></a>

`code` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The county code, whose leading digits name the voivodeship\.

<a name='DiGi.GIS.PostgreSQL.Query.IsInScope(int,string,System.Collections.Generic.ICollection_int_,System.Collections.Generic.ICollection_string_).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.ICollection&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1 'System\.Collections\.Generic\.ICollection\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1 'System\.Collections\.Generic\.ICollection\`1')

The county row identifiers in scope, or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') for every row\.

<a name='DiGi.GIS.PostgreSQL.Query.IsInScope(int,string,System.Collections.Generic.ICollection_int_,System.Collections.Generic.ICollection_string_).voivodeshipCodes'></a>

`voivodeshipCodes` [System\.Collections\.Generic\.ICollection&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1 'System\.Collections\.Generic\.ICollection\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1 'System\.Collections\.Generic\.ICollection\`1')

The voivodeship codes in scope, or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') for every voivodeship\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
[true](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/builtin\-types/bool') when the county row is in scope, otherwise [false](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/builtin\-types/bool')\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.Classes.AdministrativeAreal2D)'></a>

## Query\.Match\(this StatisticalUnit, AdministrativeAreal2D\) Method

Finds the [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') matching the specified GIS Core [DiGi\.GIS\.Classes\.AdministrativeAreal2D](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.administrativeareal2d 'DiGi\.GIS\.Classes\.AdministrativeAreal2D') within the provided root statistical unit hierarchy\.

```csharp
public static DiGi.GIS.Classes.StatisticalUnit? Match(this DiGi.GIS.Classes.StatisticalUnit? rootStatisticalUnit, DiGi.GIS.Classes.AdministrativeAreal2D? administrativeAreal2D);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.Classes.AdministrativeAreal2D).rootStatisticalUnit'></a>

`rootStatisticalUnit` [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')

The root statistical unit hierarchy\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.Classes.AdministrativeAreal2D).administrativeAreal2D'></a>

`administrativeAreal2D` [DiGi\.GIS\.Classes\.AdministrativeAreal2D](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.administrativeareal2d 'DiGi\.GIS\.Classes\.AdministrativeAreal2D')

The GIS administrative area to match\.

#### Returns
[DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')  
The matching [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') if found; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D)'></a>

## Query\.Match\(this StatisticalUnit, AdministrativeAreal2D\) Method

Finds the [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') matching the specified PostgreSQL [AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D') within the provided root statistical unit hierarchy\.

```csharp
public static DiGi.GIS.Classes.StatisticalUnit? Match(this DiGi.GIS.Classes.StatisticalUnit? rootStatisticalUnit, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D? administrativeAreal2D);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D).rootStatisticalUnit'></a>

`rootStatisticalUnit` [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')

The root statistical unit hierarchy\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D).administrativeAreal2D'></a>

`administrativeAreal2D` [AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')

The administrative area to match\.

#### Returns
[DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')  
The matching [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') if found; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference)'></a>

## Query\.Match\(this StatisticalUnit, AdministrativeAreal2DReference\) Method

Finds the [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') matching the specified [AdministrativeAreal2DReference](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference') within the provided root statistical unit hierarchy\.

```csharp
public static DiGi.GIS.Classes.StatisticalUnit? Match(this DiGi.GIS.Classes.StatisticalUnit? rootStatisticalUnit, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference? administrativeAreal2DReference);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference).rootStatisticalUnit'></a>

`rootStatisticalUnit` [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')

The root statistical unit hierarchy\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference).administrativeAreal2DReference'></a>

`administrativeAreal2DReference` [AdministrativeAreal2DReference](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference')

The administrative area reference to match\.

#### Returns
[DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')  
The matching [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') if found; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath)'></a>

## Query\.Match\(this StatisticalUnit, AdministrativeAreal2DReference, AdministrativeAreal2DReferencePath\) Method

Finds the [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') matching the subdivision reference within the hierarchy, falling back to the subdivision's parent municipality reference from the path when the subdivision itself does not match\.

```csharp
public static DiGi.GIS.Classes.StatisticalUnit? Match(this DiGi.GIS.Classes.StatisticalUnit? rootStatisticalUnit, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference? administrativeAreal2DReference, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath? administrativeAreal2DReferencePath);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath).rootStatisticalUnit'></a>

`rootStatisticalUnit` [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')

The root statistical unit hierarchy\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath).administrativeAreal2DReference'></a>

`administrativeAreal2DReference` [AdministrativeAreal2DReference](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference')

The subdivision reference to match\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath).administrativeAreal2DReferencePath'></a>

`administrativeAreal2DReferencePath` [AdministrativeAreal2DReferencePath](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReferencePath')

The subdivision's territorial ancestor chain, consulted for the municipality fallback\.

#### Returns
[DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')  
The matching [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') if found; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath)'></a>

## Query\.Match\(this StatisticalUnit, AdministrativeAreal2DReferencePath\) Method

Finds the [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') matching the leaf reference in the specified [AdministrativeAreal2DReferencePath](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReferencePath') within the hierarchy\.

```csharp
public static DiGi.GIS.Classes.StatisticalUnit? Match(this DiGi.GIS.Classes.StatisticalUnit? rootStatisticalUnit, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath? administrativeAreal2DReferencePath);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath).rootStatisticalUnit'></a>

`rootStatisticalUnit` [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')

The root statistical unit hierarchy\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath).administrativeAreal2DReferencePath'></a>

`administrativeAreal2DReferencePath` [AdministrativeAreal2DReferencePath](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReferencePath 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReferencePath')

The reference path containing the territorial ancestor chain\.

#### Returns
[DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')  
The matching [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') if found; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,string,string,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType)'></a>

## Query\.Match\(this StatisticalUnit, string, string, AdministrativeArealType\) Method

Finds the [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') matching the specified territorial name, code, and administrative type\.

```csharp
public static DiGi.GIS.Classes.StatisticalUnit? Match(this DiGi.GIS.Classes.StatisticalUnit? rootStatisticalUnit, string? name, string? code, DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType administrativeArealType);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,string,string,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).rootStatisticalUnit'></a>

`rootStatisticalUnit` [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')

The root statistical unit hierarchy\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,string,string,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The territorial entity name\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,string,string,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).code'></a>

`code` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The territorial entity code\.

<a name='DiGi.GIS.PostgreSQL.Query.Match(thisDiGi.GIS.Classes.StatisticalUnit,string,string,DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).administrativeArealType'></a>

`administrativeArealType` [AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')

The administrative area type\.

#### Returns
[DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit')  
The matching [DiGi\.GIS\.Classes\.StatisticalUnit](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalunit 'DiGi\.GIS\.Classes\.StatisticalUnit') if found; otherwise, null\.

<a name='DiGi.GIS.PostgreSQL.Query.OccupancySubdivisionId(int,System.Collections.Generic.IReadOnlyDictionary_int,System.Collections.Generic.List_int__,System.Collections.Generic.IReadOnlyDictionary_int,System.Nullable_uint__)'></a>

## Query\.OccupancySubdivisionId\(int, IReadOnlyDictionary\<int,List\<int\>\>, IReadOnlyDictionary\<int,Nullable\<uint\>\>\) Method

Names the subdivision whose occupancy figure a building filed under the given subdivision takes its share from: the subdivision itself when it carries a figure, otherwise the smallest of its containers that does\.

The subdivision layer nests and the source figures are patchy at the deeper levels - in Warsaw 13 neighbourhoods carry none while their districts do - so a building in Jelonki, which has no figure, takes Bemowo's density rather than nothing ([DiGi\.GIS\.PostgreSQL\#80](https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/80 'https://github\.com/DiGi\-Organization/DiGi\.GIS\.PostgreSQL/issues/80')). An explicit zero is a figure. A subdivision with no figure anywhere up its chain of containers answers [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null'), and its buildings stay unwritten.

```csharp
public static System.Nullable<int> OccupancySubdivisionId(int subdivisionId, System.Collections.Generic.IReadOnlyDictionary<int,System.Collections.Generic.List<int>>? containerIds_ById, System.Collections.Generic.IReadOnlyDictionary<int,System.Nullable<uint>>? occupancies_BySubdivisionId);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.OccupancySubdivisionId(int,System.Collections.Generic.IReadOnlyDictionary_int,System.Collections.Generic.List_int__,System.Collections.Generic.IReadOnlyDictionary_int,System.Nullable_uint__).subdivisionId'></a>

`subdivisionId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the subdivision the building is filed under\.

<a name='DiGi.GIS.PostgreSQL.Query.OccupancySubdivisionId(int,System.Collections.Generic.IReadOnlyDictionary_int,System.Collections.Generic.List_int__,System.Collections.Generic.IReadOnlyDictionary_int,System.Nullable_uint__).containerIds_ById'></a>

`containerIds_ById` [System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')

The containers of each subdivision, smallest first, as [ContainerIds\(this IReadOnlyDictionary&lt;int,PolygonalFace2D&gt;, double\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.ContainerIds(thisSystem.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double) 'DiGi\.GIS\.PostgreSQL\.Query\.ContainerIds\(this System\.Collections\.Generic\.IReadOnlyDictionary\<int,DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D\>, double\)') gives them\.

<a name='DiGi.GIS.PostgreSQL.Query.OccupancySubdivisionId(int,System.Collections.Generic.IReadOnlyDictionary_int,System.Collections.Generic.List_int__,System.Collections.Generic.IReadOnlyDictionary_int,System.Nullable_uint__).occupancies_BySubdivisionId'></a>

`occupancies_BySubdivisionId` [System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.UInt32](https://learn.microsoft.com/en-us/dotnet/api/system.uint32 'System\.UInt32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')

The stored figure of each subdivision, [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') where the source carries none\.

#### Returns
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')  
The identifier of the subdivision whose figure applies, or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') when neither the subdivision nor any of its containers carries one\.

<a name='DiGi.GIS.PostgreSQL.Query.ParentAdministrativeArealType(thisDiGi.GIS.PostgreSQL.Enums.AdministrativeArealType)'></a>

## Query\.ParentAdministrativeArealType\(this AdministrativeArealType\) Method

Gets the parent administrative areal type for the specified administrative areal type based on the administrative hierarchy\.

```csharp
public static System.Nullable<DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType> ParentAdministrativeArealType(this DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType administrativeArealType);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.ParentAdministrativeArealType(thisDiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).administrativeArealType'></a>

`administrativeArealType` [AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')

The current administrative areal type\.

#### Returns
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')  
The parent administrative areal type, or `null` if no parent exists \(e\.g\., for Country or Undefined\)\.

<a name='DiGi.GIS.PostgreSQL.Query.ParentIdColumnName(thisDiGi.GIS.PostgreSQL.Enums.AdministrativeArealType)'></a>

## Query\.ParentIdColumnName\(this AdministrativeArealType\) Method

Gets the name of the database column that stores the identifier of the parent administrative area for a given administrative areal type\.

This is the column of the level <b>directly above</b>. A search that has to step over an empty level - m. Poznan holds no `gmina` feature, so its subdivisions hang off the county - knows which ancestor it matched and should use [IdColumnName\(this AdministrativeArealType\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.IdColumnName(thisDiGi.GIS.PostgreSQL.Enums.AdministrativeArealType) 'DiGi\.GIS\.PostgreSQL\.Query\.IdColumnName\(this DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType\)') with that type instead.

```csharp
public static string? ParentIdColumnName(this DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType administrativeArealType);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.ParentIdColumnName(thisDiGi.GIS.PostgreSQL.Enums.AdministrativeArealType).administrativeArealType'></a>

`administrativeArealType` [AdministrativeArealType](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType 'DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType')

The type of the administrative area\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The name of the parent ID column as a string, or null if no parent column exists for the specified type\.

<a name='DiGi.GIS.PostgreSQL.Query.PartitionCommandText(string,int)'></a>

## Query\.PartitionCommandText\(string, int\) Method

Builds the statement that creates one county partition of a list\-partitioned table and leaves it with statistics\.

A partition PostgreSQL has never analysed reports `reltuples = -1`, and an empty one stays that way for good - autovacuum analyses on modifications, and nothing ever modifies it. Every estimated count then reads it as "not measured" rather than as zero, and one such partition voids the aggregate of the whole voivodeship it sits in. The statement therefore analyses the partition right after creating it, and only then: the check on `reltuples` keeps the analyse off the path of every later write, which creates nothing and would otherwise pay for a statistics pass over the whole partition on each batch.

```csharp
public static string PartitionCommandText(string tableName, int countyId);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.PartitionCommandText(string,int).tableName'></a>

`tableName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the partitioned table\.

<a name='DiGi.GIS.PostgreSQL.Query.PartitionCommandText(string,int).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county the partition holds; it names the partition and is its single list value\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The SQL text creating the partition if it is absent and analysing it if it has never been analysed\.

<a name='DiGi.GIS.PostgreSQL.Query.Polygonal2DsByCountyId(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_)'></a>

## Query\.Polygonal2DsByCountyId\(this IEnumerable\<AdministrativeAreal2D\>\) Method

Derives the polygon of each county row once, keyed by the identifier of the row it came from\.

A row stores its geometry as JSON, so reading its polygon deserializes the whole object - and a county polygon carries thousands of vertices. Deciding many buildings against the same parts should derive them once through this and pass the result to [CountyId\(this IDictionary&lt;int,IPolygonal2D&gt;, IPolygonal2D, double\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.CountyId(thisSystem.Collections.Generic.IDictionary_int,DiGi.Geometry.Planar.Interfaces.IPolygonal2D_,DiGi.Geometry.Planar.Interfaces.IPolygonal2D,double) 'DiGi\.GIS\.PostgreSQL\.Query\.CountyId\(this System\.Collections\.Generic\.IDictionary\<int,DiGi\.Geometry\.Planar\.Interfaces\.IPolygonal2D\>, DiGi\.Geometry\.Planar\.Interfaces\.IPolygonal2D, double\)'), rather than handing the rows themselves to the other overload, which repeats the conversion for every building.

A row whose geometry cannot be read is left out, so the result holds only parts that can actually be tested against.

```csharp
public static System.Collections.Generic.Dictionary<int,DiGi.Geometry.Planar.Interfaces.IPolygonal2D> Polygonal2DsByCountyId(this System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D>? administrativeAreal2Ds);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.Polygonal2DsByCountyId(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_).administrativeAreal2Ds'></a>

`administrativeAreal2Ds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The county rows to derive polygons from\.

#### Returns
[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[DiGi\.Geometry\.Planar\.Interfaces\.IPolygonal2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.interfaces.ipolygonal2d 'DiGi\.Geometry\.Planar\.Interfaces\.IPolygonal2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')  
The external edge of each row that has one, keyed by row identifier\. Empty when nothing could be derived\.

<a name='DiGi.GIS.PostgreSQL.Query.PolygonalFace2DsById(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_)'></a>

## Query\.PolygonalFace2DsById\(this IEnumerable\<AdministrativeAreal2D\>\) Method

Derives the face of each row once, keyed by the identifier of the row it came from\.

Reading a row's geometry deserializes the whole stored object, and the property that exposes the face hands back a clone on every access - so a caller deciding many points against the same rows should derive the faces once through this and test against the result, rather than reaching through the rows again for each point.

The whole face is kept, holes included, unlike [Polygonal2DsByCountyId\(this IEnumerable&lt;AdministrativeAreal2D&gt;\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.Polygonal2DsByCountyId(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_) 'DiGi\.GIS\.PostgreSQL\.Query\.Polygonal2DsByCountyId\(this System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D\>\)'), which keeps only the outer ring. An area that excludes a town inside it is a face with a hole, and a point in that town belongs to the town rather than to the area around it - testing against the outer ring alone would claim it.

A row whose geometry cannot be read is left out, so the result holds only rows that can actually be tested against.

```csharp
public static System.Collections.Generic.Dictionary<int,DiGi.Geometry.Planar.Classes.PolygonalFace2D> PolygonalFace2DsById(this System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D>? administrativeAreal2Ds);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.PolygonalFace2DsById(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D_).administrativeAreal2Ds'></a>

`administrativeAreal2Ds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[AdministrativeAreal2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2D 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The rows to derive faces from\.

#### Returns
[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D](https://learn.microsoft.com/en-us/dotnet/api/digi.geometry.planar.classes.polygonalface2d 'DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')  
The face of each row that has one, keyed by row identifier\. Empty when nothing could be derived\.

<a name='DiGi.GIS.PostgreSQL.Query.Population(thisDiGi.GIS.Classes.StatisticalDataCollection)'></a>

## Query\.Population\(this StatisticalDataCollection\) Method

Retrieves the demographic population yearly data series from the statistical data collection, scaling BDL values to individual counts if applicable\.

```csharp
public static DiGi.GIS.Classes.StatisticalYearlyDoubleData? Population(this DiGi.GIS.Classes.StatisticalDataCollection? statisticalDataCollection);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.Population(thisDiGi.GIS.Classes.StatisticalDataCollection).statisticalDataCollection'></a>

`statisticalDataCollection` [DiGi\.GIS\.Classes\.StatisticalDataCollection](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticaldatacollection 'DiGi\.GIS\.Classes\.StatisticalDataCollection')

The statistical data collection containing demographic information\.

#### Returns
[DiGi\.GIS\.Classes\.StatisticalYearlyDoubleData](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalyearlydoubledata 'DiGi\.GIS\.Classes\.StatisticalYearlyDoubleData')  
A [DiGi\.GIS\.Classes\.StatisticalYearlyDoubleData](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.statisticalyearlydoubledata 'DiGi\.GIS\.Classes\.StatisticalYearlyDoubleData') containing normalized yearly population counts, or null if no population series is found\.

<a name='DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken)'></a>

## Query\.RandomBuilding2DReferenceWithoutUserYearBuiltAsync\(this OrtoDatasPostgreSQLConverter, AdministrativeAreal2DPostgreSQLConverter, YearBuiltDataPostgreSQLConverter, IEnumerable\<int\>, int, int, int, CancellationToken\) Method

Asynchronously draws one building that has orthophoto coverage and no user\-provided year built yet, optionally confined to specific `building_2d` parts\.

The orthophoto rows live in the storage database and the building, year-built and administrative rows in the main one, so this cannot be a join and is not one (the same split [SubdivisionLinksAsync\(this OrtoDatasPostgreSQLConverter, Building2DPostgreSQLConverter, int, int, int, CancellationToken\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.SubdivisionLinksAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Query\.SubdivisionLinksAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, int, int, int, System\.Threading\.CancellationToken\)') works across). Stage 1 draws a county code from the parts that hold orthophotos, weighted by the estimated rows of the parts ([GetEstimatedCountsAsync\(IEnumerable&lt;int&gt;, bool, int, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter.GetEstimatedCountsAsync(System.Collections.Generic.IEnumerable_int_,bool,int,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter\.GetEstimatedCountsAsync\(System\.Collections\.Generic\.IEnumerable\<int\>, bool, int, int, System\.Threading\.CancellationToken\)'), storage side) over the county references ([GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync\(AdministrativeArealType, Nullable&lt;int&gt;, bool, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType,System.Nullable_int_,bool,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter\.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync\(DiGi\.GIS\.PostgreSQL\.Enums\.AdministrativeArealType, System\.Nullable\<int\>, bool, int, System\.Threading\.CancellationToken\)'), main side). Stage 2 draws a batch of references from the code's parts without reading the imagery ([GetRandomReferencesByCountyIdsAsync\(IEnumerable&lt;int&gt;, int, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter.GetRandomReferencesByCountyIdsAsync(System.Collections.Generic.IEnumerable_int_,int,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter\.GetRandomReferencesByCountyIdsAsync\(System\.Collections\.Generic\.IEnumerable\<int\>, int, int, System\.Threading\.CancellationToken\)')), keeps the ones that are buildings without a user entry ([GetBuilding2DReferencesWithoutUserYearBuiltAsync\(IEnumerable&lt;int&gt;, IEnumerable&lt;string&gt;, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter.GetBuilding2DReferencesWithoutUserYearBuiltAsync(System.Collections.Generic.IEnumerable_int_,System.Collections.Generic.IEnumerable_string_,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.YearBuiltDataPostgreSQLConverter\.GetBuilding2DReferencesWithoutUserYearBuiltAsync\(System\.Collections\.Generic\.IEnumerable\<int\>, System\.Collections\.Generic\.IEnumerable\<string\>, int, System\.Threading\.CancellationToken\)')), and answers the first survivor that holds at least one photo year ([GetYearsByReferenceAsync\(string, Nullable&lt;int&gt;, bool, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter.GetYearsByReferenceAsync(string,System.Nullable_int_,bool,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter\.GetYearsByReferenceAsync\(string, System\.Nullable\<int\>, bool, int, System\.Threading\.CancellationToken\)')), so the drawn building always carries a card.

A code whose parts yield nothing within [maxBatchCount](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken).maxBatchCount 'DiGi\.GIS\.PostgreSQL\.Query\.RandomBuilding2DReferenceWithoutUserYearBuiltAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.YearBuiltDataPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<int\>, int, int, int, System\.Threading\.CancellationToken\)\.maxBatchCount') batches is removed and redrawn, so the loop is bounded by codes times batches; `null` is the answer when nothing is left. [countyIds](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken).countyIds 'DiGi\.GIS\.PostgreSQL\.Query\.RandomBuilding2DReferenceWithoutUserYearBuiltAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.YearBuiltDataPostgreSQLConverter, System\.Collections\.Generic\.IEnumerable\<int\>, int, int, int, System\.Threading\.CancellationToken\)\.countyIds') are `building_2d` part ids, never county codes; `null` or empty draws from every covered part, and an id that names no covered part simply falls out of the pool. A building holding only `PredictedYearBuilt` entries is eligible; a `UserYearBuilt` of any relation is not, because a bound is still a verification.

```csharp
public static System.Threading.Tasks.Task<DiGi.GIS.PostgreSQL.Classes.Building2DReference?> RandomBuilding2DReferenceWithoutUserYearBuiltAsync(this DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter? ortoDatasPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter, System.Collections.Generic.IEnumerable<int>? countyIds=null, int batchSize=64, int maxBatchCount=4, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken).ortoDatasPostgreSQLConverter'></a>

`ortoDatasPostgreSQLConverter` [OrtoDatasPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter')

The converter reading the orthophoto store\.

<a name='DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken).administrativeAreal2DPostgreSQLConverter'></a>

`administrativeAreal2DPostgreSQLConverter` [AdministrativeAreal2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter')

The converter reading the administrative areas, in the main store\.

<a name='DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken).yearBuiltDataPostgreSQLConverter'></a>

`yearBuiltDataPostgreSQLConverter` [YearBuiltDataPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.YearBuiltDataPostgreSQLConverter')

The converter reading the buildings and their year\-built rows, in the main store\.

<a name='DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The `building_2d` part ids to confine the draw to, or null/empty to draw from every covered part\.

<a name='DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken).batchSize'></a>

`batchSize` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

How many references one storage\-side draw takes before the main side filters them\.

<a name='DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken).maxBatchCount'></a>

`maxBatchCount` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

How many batches a drawn code is given before it is removed from the pool\.

<a name='DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.YearBuiltDataPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[Building2DReference](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DReference 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DReference')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result contains the drawn [Building2DReference](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DReference 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DReference'), or null when a converter is missing, either side could not be read, or no covered \(and requested\) county part yields a candidate\.

<a name='DiGi.GIS.PostgreSQL.Query.RandomSeed(int,int)'></a>

## Query\.RandomSeed\(int, int\) Method

Combines a run seed with a county row identifier into a seed for that county alone\.

A single generator advanced across counties makes each county's draw depend on how many items every preceding county held, so changing the scope of a run - or the population of one county - changes what every county after it draws. Seeding per county removes that: a county draws the same sample whether it is verified on its own, with its voivodeship, or nationally.

<b>Do not replace this with <see cref="M:System.HashCode.Combine``2(``0,``1)"/>.</b> That mixes in a seed randomized per process, so it returns a different value on every run - the opposite of what this exists to provide.

```csharp
public static int RandomSeed(int randomSeed, int countyId);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.RandomSeed(int,int).randomSeed'></a>

`randomSeed` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The seed identifying the run\.

<a name='DiGi.GIS.PostgreSQL.Query.RandomSeed(int,int).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county row\.

#### Returns
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')  
The seed to draw that county's sample with\.

<a name='DiGi.GIS.PostgreSQL.Query.RowsByCountyIdAndReference(thisDiGi.Core.IO.Table.Classes.Table,DiGi.Core.IO.Table.Classes.Column,DiGi.Core.IO.Table.Classes.Column)'></a>

## Query\.RowsByCountyIdAndReference\(this Table, Column, Column\) Method

Indexes the rows of a table by their county identifier and reference so that a caller matching a collection of items against them does not rescan the table for each one\.

Rows are read from the back, so where several rows carry the same county and reference the one at the highest index wins - which is what a backwards scan stopping at its first hit did.

```csharp
public static System.Collections.Generic.Dictionary<(int,string),DiGi.Core.IO.Table.Classes.Row> RowsByCountyIdAndReference(this DiGi.Core.IO.Table.Classes.Table? table, DiGi.Core.IO.Table.Classes.Column? column_CountyId, DiGi.Core.IO.Table.Classes.Column? column_Reference);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.RowsByCountyIdAndReference(thisDiGi.Core.IO.Table.Classes.Table,DiGi.Core.IO.Table.Classes.Column,DiGi.Core.IO.Table.Classes.Column).table'></a>

`table` [DiGi\.Core\.IO\.Table\.Classes\.Table](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.table 'DiGi\.Core\.IO\.Table\.Classes\.Table')

The table whose rows are indexed\.

<a name='DiGi.GIS.PostgreSQL.Query.RowsByCountyIdAndReference(thisDiGi.Core.IO.Table.Classes.Table,DiGi.Core.IO.Table.Classes.Column,DiGi.Core.IO.Table.Classes.Column).column_CountyId'></a>

`column_CountyId` [DiGi\.Core\.IO\.Table\.Classes\.Column](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.column 'DiGi\.Core\.IO\.Table\.Classes\.Column')

The county identifier column\.

<a name='DiGi.GIS.PostgreSQL.Query.RowsByCountyIdAndReference(thisDiGi.Core.IO.Table.Classes.Table,DiGi.Core.IO.Table.Classes.Column,DiGi.Core.IO.Table.Classes.Column).column_Reference'></a>

`column_Reference` [DiGi\.Core\.IO\.Table\.Classes\.Column](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.column 'DiGi\.Core\.IO\.Table\.Classes\.Column')

The reference column\.

#### Returns
[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple 'System\.ValueTuple')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[DiGi\.Core\.IO\.Table\.Classes\.Row](https://learn.microsoft.com/en-us/dotnet/api/digi.core.io.table.classes.row 'DiGi\.Core\.IO\.Table\.Classes\.Row')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')  
A dictionary of rows keyed by county identifier and reference\. Rows missing either value are not included\. Never null\.

<a name='DiGi.GIS.PostgreSQL.Query.Sample_T_(thisSystem.Collections.Generic.IEnumerable_T_,int,System.Random)'></a>

## Query\.Sample\<T\>\(this IEnumerable\<T\>, int, Random\) Method

Draws a reproducible sample of the given size from a collection\.

A partial Fisher-Yates shuffle over a copy: every item is equally likely to be drawn and none is drawn twice, without shuffling a list that can hold tens of thousands of entries in full.

The draw consumes exactly one value from [random](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.Sample_T_(thisSystem.Collections.Generic.IEnumerable_T_,int,System.Random).random 'DiGi\.GIS\.PostgreSQL\.Query\.Sample\<T\>\(this System\.Collections\.Generic\.IEnumerable\<T\>, int, System\.Random\)\.random') per item returned, so a generator shared across several calls hands each call a different stream depending on how large the preceding populations were. Seed a fresh generator per call with [RandomSeed\(int, int\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.RandomSeed(int,int) 'DiGi\.GIS\.PostgreSQL\.Query\.RandomSeed\(int, int\)') when the draws are meant to be independent of one another.

```csharp
public static System.Collections.Generic.List<T>? Sample<T>(this System.Collections.Generic.IEnumerable<T>? values, int sampleSize, System.Random? random);
```
#### Type parameters

<a name='DiGi.GIS.PostgreSQL.Query.Sample_T_(thisSystem.Collections.Generic.IEnumerable_T_,int,System.Random).T'></a>

`T`

The item type\.
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.Sample_T_(thisSystem.Collections.Generic.IEnumerable_T_,int,System.Random).values'></a>

`values` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[T](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.Sample_T_(thisSystem.Collections.Generic.IEnumerable_T_,int,System.Random).T 'DiGi\.GIS\.PostgreSQL\.Query\.Sample\<T\>\(this System\.Collections\.Generic\.IEnumerable\<T\>, int, System\.Random\)\.T')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The items to draw from\.

<a name='DiGi.GIS.PostgreSQL.Query.Sample_T_(thisSystem.Collections.Generic.IEnumerable_T_,int,System.Random).sampleSize'></a>

`sampleSize` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The number of items to draw\. A value of zero or less takes them all\.

<a name='DiGi.GIS.PostgreSQL.Query.Sample_T_(thisSystem.Collections.Generic.IEnumerable_T_,int,System.Random).random'></a>

`random` [System\.Random](https://learn.microsoft.com/en-us/dotnet/api/system.random 'System\.Random')

The random source, seeded by the caller so the draw can be repeated\.

#### Returns
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[T](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.Sample_T_(thisSystem.Collections.Generic.IEnumerable_T_,int,System.Random).T 'DiGi\.GIS\.PostgreSQL\.Query\.Sample\<T\>\(this System\.Collections\.Generic\.IEnumerable\<T\>, int, System\.Random\)\.T')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')  
The drawn items, or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') when [values](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.Sample_T_(thisSystem.Collections.Generic.IEnumerable_T_,int,System.Random).values 'DiGi\.GIS\.PostgreSQL\.Query\.Sample\<T\>\(this System\.Collections\.Generic\.IEnumerable\<T\>, int, System\.Random\)\.values') or [random](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.Sample_T_(thisSystem.Collections.Generic.IEnumerable_T_,int,System.Random).random 'DiGi\.GIS\.PostgreSQL\.Query\.Sample\<T\>\(this System\.Collections\.Generic\.IEnumerable\<T\>, int, System\.Random\)\.random') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

<a name='DiGi.GIS.PostgreSQL.Query.SearchText(string)'></a>

## Query\.SearchText\(string\) Method

Normalises free text for the address search: the one form both the stored `search_text` column of [AddressPoint](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.TableName.AddressPoint 'DiGi\.GIS\.PostgreSQL\.Constants\.TableName\.AddressPoint') and every search query are reduced to, so the two can never diverge\.

Steps: lower-cased with the invariant culture; `ł` mapped to `l` (it has no canonical decomposition, so [System\.Text\.NormalizationForm\.FormD](https://learn.microsoft.com/en-us/dotnet/api/system.text.normalizationform.formd 'System\.Text\.NormalizationForm\.FormD') leaves it alone); diacritics removed by decomposing and dropping the non-spacing marks; every character that is neither a letter nor a digit turned into a space; the street-type tokens `ul`, `al`, `pl`, `os` and `rondo` dropped; whitespace collapsed. Only whole tokens are dropped, so `ulica` or `plac` stay.

This replaces `unaccent()` on the database side, which is not `IMMUTABLE` and so cannot back an index. The result does not depend on [System\.Globalization\.CultureInfo\.CurrentCulture](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.currentculture 'System\.Globalization\.CultureInfo\.CurrentCulture').

```csharp
public static string? SearchText(string? text);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.SearchText(string).text'></a>

`text` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The text to normalise\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The normalised text, or null when [text](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.SearchText(string).text 'DiGi\.GIS\.PostgreSQL\.Query\.SearchText\(string\)\.text') is null or holds no letter or digit\. When the text holds nothing but street\-type tokens they are kept, so a street named `Rondo` is still searchable\.

<a name='DiGi.GIS.PostgreSQL.Query.SearchText(thisDiGi.GIS.Classes.AddressPoint)'></a>

## Query\.SearchText\(this AddressPoint\) Method

Builds the normalised `search_text` of an address point: `"<street> <locality>"`, or the locality alone for a locality without streets, reduced by [SearchText\(string\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.SearchText(string) 'DiGi\.GIS\.PostgreSQL\.Query\.SearchText\(string\)')\.

```csharp
public static string? SearchText(this DiGi.GIS.Classes.AddressPoint? addressPoint);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.SearchText(thisDiGi.GIS.Classes.AddressPoint).addressPoint'></a>

`addressPoint` [DiGi\.GIS\.Classes\.AddressPoint](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.addresspoint 'DiGi\.GIS\.Classes\.AddressPoint')

The address point\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The normalised search text, or null when the address point is null or has neither a street nor a locality name\.

<a name='DiGi.GIS.PostgreSQL.Query.SiblingCountyGroups(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference_)'></a>

## Query\.SiblingCountyGroups\(this IEnumerable\<AdministrativeAreal2DReference\>\) Method

Groups county polygon parts by code: every part is mapped to the full set of parts that share its code\.

A county code is not a key - it names one row per polygon part - so the result is keyed by part `Id`, and a part with no usable code groups with itself. This is the single definition the building data update uses to reach a building under every part of its subdivision's parent county.

```csharp
public static System.Collections.Generic.Dictionary<int,System.Collections.Generic.HashSet<int>> SiblingCountyGroups(this System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference>? countyReferences);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.SiblingCountyGroups(thisSystem.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference_).countyReferences'></a>

`countyReferences` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[AdministrativeAreal2DReference](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DReference 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DReference')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

County references to group\. May be [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null'), in which case the result is empty\.

#### Returns
[System\.Collections\.Generic\.Dictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2 'System\.Collections\.Generic\.Dictionary\`2')  
A map from each county part `Id` to the set of part `Id`s sharing its code; a code\-less part maps to itself\.

<a name='DiGi.GIS.PostgreSQL.Query.SiblingCountyIdsAsync(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken)'></a>

## Query\.SiblingCountyIdsAsync\(this AdministrativeAreal2DPostgreSQLConverter, IEnumerable\<int\>, int, CancellationToken\) Method

Widens county rows to every polygon part of the codes they name\.

A county whose territory is disconnected is stored as one row per part, and a building's data is filed under the part holding its `building_2d` row - so a caller naming one part, such as a visitor whose building view carries a single county id, may be looking at data stored under a sibling. The answer never crosses into a county the caller did not name.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.HashSet<int>?> SiblingCountyIdsAsync(this DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter, System.Collections.Generic.IEnumerable<int>? countyIds, int commandTimeout=30, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.SiblingCountyIdsAsync(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).administrativeAreal2DPostgreSQLConverter'></a>

`administrativeAreal2DPostgreSQLConverter` [AdministrativeAreal2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter')

The converter used to read the codes of the rows and the parts of the codes\.

<a name='DiGi.GIS.PostgreSQL.Query.SiblingCountyIdsAsync(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).countyIds'></a>

`countyIds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The county rows to widen\.

<a name='DiGi.GIS.PostgreSQL.Query.SiblingCountyIdsAsync(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the lookups\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Query.SiblingCountyIdsAsync(thisDiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,System.Collections.Generic.IEnumerable_int_,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.HashSet&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1 'System\.Collections\.Generic\.HashSet\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The given rows together with every part of their codes; an empty set when no row is given; `null` when a lookup could not run\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionCoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,int,double,int,System.Threading.CancellationToken)'></a>

## Query\.SubdivisionCoveragesAsync\(this OrtoDatasPostgreSQLConverter, Building2DPostgreSQLConverter, AdministrativeAreal2DPostgreSQLConverter, int, double, int, CancellationToken\) Method

Asynchronously measures, for one county, how much of each of its subdivisions' buildings the orthophoto store holds\.

The subdivisions measured are those filed under the county part and under every sibling part sharing its code (the parent lookup widens by code on its own): a subdivision's `county_id` names one part, and the buildings it holds may be filed under another. Each is measured by its polygon through [CoveragesAsync\(this OrtoDatasPostgreSQLConverter, Building2DPostgreSQLConverter, int, IReadOnlyDictionary&lt;int,PolygonalFace2D&gt;, double, int, CancellationToken\)](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.CoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,System.Collections.Generic.IReadOnlyDictionary_int,DiGi.Geometry.Planar.Classes.PolygonalFace2D_,double,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Query\.CoveragesAsync\(this DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter, DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter, int, System\.Collections\.Generic\.IReadOnlyDictionary\<int,DiGi\.Geometry\.Planar\.Classes\.PolygonalFace2D\>, double, int, System\.Threading\.CancellationToken\)'), which is where the counting and its caveats live - in particular that where the layer nests, a building is counted for every subdivision containing it, so the results of a city and its districts overlap and must not be summed.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.List<DiGi.GIS.PostgreSQL.Classes.OrtoDatasCoverageResult>?> SubdivisionCoveragesAsync(this DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter? ortoDatasPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter? building2DPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter, int countyId, double tolerance=0.001, int commandTimeout=600, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionCoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,int,double,int,System.Threading.CancellationToken).ortoDatasPostgreSQLConverter'></a>

`ortoDatasPostgreSQLConverter` [OrtoDatasPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter')

The converter reading the orthophoto store\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionCoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,int,double,int,System.Threading.CancellationToken).building2DPostgreSQLConverter'></a>

`building2DPostgreSQLConverter` [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')

The converter reading the building store\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionCoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,int,double,int,System.Threading.CancellationToken).administrativeAreal2DPostgreSQLConverter'></a>

`administrativeAreal2DPostgreSQLConverter` [AdministrativeAreal2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.AdministrativeAreal2DPostgreSQLConverter')

The converter reading the subdivisions and the county parts\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionCoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,int,double,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county to measure\. One polygon part, not a code \- a multi\-part county is measured a part at a time\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionCoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,int,double,int,System.Threading.CancellationToken).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The distance tolerance of the containment test\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionCoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,int,double,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionCoveragesAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.AdministrativeAreal2DPostgreSQLConverter,int,double,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[OrtoDatasCoverageResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasCoverageResult 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasCoverageResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result contains one [OrtoDatasCoverageResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasCoverageResult 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasCoverageResult') per subdivision holding at least one of the county's buildings, plus one carrying a null [AdministrativeAreal2DId](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasCoverageResult.AdministrativeAreal2DId 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasCoverageResult\.AdministrativeAreal2DId') for the buildings inside no subdivision polygon when there are any; or null when a converter is missing, the subdivisions or either side could not be read, or the county holds no orthophoto row at all\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionLinksAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,int,System.Threading.CancellationToken)'></a>

## Query\.SubdivisionLinksAsync\(this OrtoDatasPostgreSQLConverter, Building2DPostgreSQLConverter, int, int, int, CancellationToken\) Method

Asynchronously compares, for one county, the subdivision each building is filed under against the one its orthophoto row carries\.

The two tables are in different databases - `building_2d` in the main store, `orto_datas` in the storage one - so this cannot be a join and is not one. Each side is read once, cheaply, and matched in memory: the orthophoto side through [GetSubdivisionIdsByCountyIdAsync\(int, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter.GetSubdivisionIdsByCountyIdAsync(int,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter\.GetSubdivisionIdsByCountyIdAsync\(int, int, System\.Threading\.CancellationToken\)'), which projects two columns and never the imagery, and the building side through [GetBuilding2DReferencesByCountyIdAsync\(int, Nullable&lt;int&gt;, IEnumerable&lt;string&gt;, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter.GetBuilding2DReferencesByCountyIdAsync(int,System.Nullable_int_,System.Collections.Generic.IEnumerable_string_,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter\.GetBuilding2DReferencesByCountyIdAsync\(int, System\.Nullable\<int\>, System\.Collections\.Generic\.IEnumerable\<string\>, int, System\.Threading\.CancellationToken\)'), which already returns nothing heavier.

That separation is the whole reason the value has to be pushed across by a refresh in the first place, and the reason nothing keeps the two in step on its own.

```csharp
public static System.Threading.Tasks.Task<DiGi.GIS.PostgreSQL.Classes.OrtoDatasSubdivisionResult?> SubdivisionLinksAsync(this DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter? ortoDatasPostgreSQLConverter, DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter? building2DPostgreSQLConverter, int countyId, int sampleCount=20, int commandTimeout=600, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionLinksAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,int,System.Threading.CancellationToken).ortoDatasPostgreSQLConverter'></a>

`ortoDatasPostgreSQLConverter` [OrtoDatasPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasPostgreSQLConverter')

The converter reading the orthophoto store\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionLinksAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,int,System.Threading.CancellationToken).building2DPostgreSQLConverter'></a>

`building2DPostgreSQLConverter` [Building2DPostgreSQLConverter](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2DPostgreSQLConverter')

The converter reading the building store\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionLinksAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,int,System.Threading.CancellationToken).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The identifier of the county to compare\. One polygon part, not a code\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionLinksAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,int,System.Threading.CancellationToken).sampleCount'></a>

`sampleCount` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

How many references to name per disagreeing category, so a result stays a fixed size on a county with a hundred thousand buildings\. Zero names none\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionLinksAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,int,System.Threading.CancellationToken).commandTimeout'></a>

`commandTimeout` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The timeout in seconds for the execution of each command\. A value of 0 disables the timeout\.

<a name='DiGi.GIS.PostgreSQL.Query.SubdivisionLinksAsync(thisDiGi.GIS.PostgreSQL.Classes.OrtoDatasPostgreSQLConverter,DiGi.GIS.PostgreSQL.Classes.Building2DPostgreSQLConverter,int,int,int,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[OrtoDatasSubdivisionResult](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoDatasSubdivisionResult 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoDatasSubdivisionResult')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A task that represents the asynchronous operation\. The task result carries the comparison, or null when either converter is missing or either side could not be read\.

<a name='DiGi.GIS.PostgreSQL.Query.SubjectCount(System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,int)'></a>

## Query\.SubjectCount\(IEnumerable\<Building2D\>, IEnumerable\<Building2D\>, int\) Method

Counts how many of the given buildings a spatial read of their own surroundings brought back\.

The surroundings of a set of buildings are read over the area those buildings cover, so every one of them is inside it and a correct read returns all of them. What comes back short says which of two different things went wrong, and they are not answered by the same fix: none of them back means the read is not reaching the partition they are filed under - a county whose territory is disconnected is one row per polygon part, and pruning to the wrong part answers an empty set rather than an error. Some of them back is per building instead: `min_x` to `max_y` are nullable and the overlap test is made on them, so a building whose stored box is missing drops out of its own neighbourhood.

Matched on county and reference together, because a reference is unique only within a county.

See https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/64.

```csharp
public static int SubjectCount(System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.Building2D>? building2Ds, System.Collections.Generic.IEnumerable<DiGi.GIS.PostgreSQL.Classes.Building2D>? building2Ds_Neighbour, int countyId);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.SubjectCount(System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,int).building2Ds'></a>

`building2Ds` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[Building2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2D 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The buildings the surroundings were read for\. May be [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

<a name='DiGi.GIS.PostgreSQL.Query.SubjectCount(System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,int).building2Ds_Neighbour'></a>

`building2Ds_Neighbour` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[Building2D](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.Building2D 'DiGi\.GIS\.PostgreSQL\.Classes\.Building2D')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The surroundings that came back\. May be [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

<a name='DiGi.GIS.PostgreSQL.Query.SubjectCount(System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,int).countyId'></a>

`countyId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The county part the buildings are filed under\.

#### Returns
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')  
The number of [building2Ds](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.SubjectCount(System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,int).building2Ds 'DiGi\.GIS\.PostgreSQL\.Query\.SubjectCount\(System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Classes\.Building2D\>, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Classes\.Building2D\>, int\)\.building2Ds') present in [building2Ds\_Neighbour](DiGi.GIS.PostgreSQL.md#DiGi.GIS.PostgreSQL.Query.SubjectCount(System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,System.Collections.Generic.IEnumerable_DiGi.GIS.PostgreSQL.Classes.Building2D_,int).building2Ds_Neighbour 'DiGi\.GIS\.PostgreSQL\.Query\.SubjectCount\(System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Classes\.Building2D\>, System\.Collections\.Generic\.IEnumerable\<DiGi\.GIS\.PostgreSQL\.Classes\.Building2D\>, int\)\.building2Ds\_Neighbour')\.

<a name='DiGi.GIS.PostgreSQL.Query.TryParse(thisstring,string,System.Nullable_int_,DiGi.Core.Classes.GuidReference)'></a>

## Query\.TryParse\(this string, string, Nullable\<int\>, GuidReference\) Method

Attempts to parse a reference string into its constituent building model reference, optional county identifier, and optional building element GUID reference\.

The method delegates to [DiGi\.Core\.Query\.TryParse\(System\.String,DiGi\.Core\.Interfaces\.IReference@\)](https://learn.microsoft.com/en-us/dotnet/api/digi.core.query.tryparse#digi-core-query-tryparse(system-string-digi-core-interfaces-ireference@) 'DiGi\.Core\.Query\.TryParse\(System\.String,DiGi\.Core\.Interfaces\.IReference@\)') to deserialize the string. A plain non-parsable string is treated as a bare building model reference. For [DiGi\.Core\.Classes\.ComplexReference](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.complexreference 'DiGi\.Core\.Classes\.ComplexReference') values, the individual segments are extracted by matching [DiGi\.Core\.Classes\.TypeReference](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.typereference 'DiGi\.Core\.Classes\.TypeReference') discriminators for [DiGi\.Analytical\.Building\.Classes\.BuildingModel](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.classes.buildingmodel 'DiGi\.Analytical\.Building\.Classes\.BuildingModel'), [DiGi\.GIS\.Classes\.AdministrativeDivision](https://learn.microsoft.com/en-us/dotnet/api/digi.gis.classes.administrativedivision 'DiGi\.GIS\.Classes\.AdministrativeDivision'), and [DiGi\.Analytical\.Building\.Interfaces\.IBuildingGuidObject](https://learn.microsoft.com/en-us/dotnet/api/digi.analytical.building.interfaces.ibuildingguidobject 'DiGi\.Analytical\.Building\.Interfaces\.IBuildingGuidObject')-assignable types.

```csharp
public static bool TryParse(this string? reference, out string buildingModelReference, out System.Nullable<int> countyId, out DiGi.Core.Classes.GuidReference? buildingObjectGuidReference);
```
#### Parameters

<a name='DiGi.GIS.PostgreSQL.Query.TryParse(thisstring,string,System.Nullable_int_,DiGi.Core.Classes.GuidReference).reference'></a>

`reference` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The reference string to parse\.

<a name='DiGi.GIS.PostgreSQL.Query.TryParse(thisstring,string,System.Nullable_int_,DiGi.Core.Classes.GuidReference).buildingModelReference'></a>

`buildingModelReference` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

When this method returns, contains the extracted building model reference string\. Set to [System\.String\.Empty](https://learn.microsoft.com/en-us/dotnet/api/system.string.empty 'System\.String\.Empty') if parsing fails\.

<a name='DiGi.GIS.PostgreSQL.Query.TryParse(thisstring,string,System.Nullable_int_,DiGi.Core.Classes.GuidReference).countyId'></a>

`countyId` [System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

When this method returns, contains the county identifier if one was found in the reference chain; otherwise, [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

<a name='DiGi.GIS.PostgreSQL.Query.TryParse(thisstring,string,System.Nullable_int_,DiGi.Core.Classes.GuidReference).buildingObjectGuidReference'></a>

`buildingObjectGuidReference` [DiGi\.Core\.Classes\.GuidReference](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.guidreference 'DiGi\.Core\.Classes\.GuidReference')

When this method returns, contains the [DiGi\.Core\.Classes\.GuidReference](https://learn.microsoft.com/en-us/dotnet/api/digi.core.classes.guidreference 'DiGi\.Core\.Classes\.GuidReference') for a building element if one was found in the reference chain; otherwise, [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
[true](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/builtin\-types/bool') if the reference was successfully parsed or treated as a bare building model reference; [false](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/builtin\-types/bool') if the input is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null'), empty, or the parsed structure cannot be resolved\.