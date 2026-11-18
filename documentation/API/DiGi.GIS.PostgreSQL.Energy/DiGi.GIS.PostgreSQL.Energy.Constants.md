#### [DiGi\.GIS\.PostgreSQL\.Energy](DiGi.GIS.PostgreSQL.Energy.Overview.md 'DiGi\.GIS\.PostgreSQL\.Energy\.Overview')

## DiGi\.GIS\.PostgreSQL\.Energy\.Constants Namespace
### Classes

<a name='DiGi.GIS.PostgreSQL.Energy.Constants.TableName'></a>

## TableName Class

The names of the tables of the monthly heat demand \(DiGi\.GIS\.PostgreSQL\#143\)\.

```csharp
public static class TableName
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → TableName
### Fields

<a name='DiGi.GIS.PostgreSQL.Energy.Constants.TableName.BuildingHeatDemand'></a>

## TableName\.BuildingHeatDemand Field

The table of the stored monthly heat demand of every building, one row per county part, building model and method version, on the Storage database beside `building_solar_radiation`\.

```csharp
public const string BuildingHeatDemand = "building_heat_demand";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')