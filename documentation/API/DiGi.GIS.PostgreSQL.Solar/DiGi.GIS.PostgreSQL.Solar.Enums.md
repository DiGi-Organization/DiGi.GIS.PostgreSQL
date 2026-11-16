#### [DiGi\.GIS\.PostgreSQL\.Solar](DiGi.GIS.PostgreSQL.Solar.Overview.md 'DiGi\.GIS\.PostgreSQL\.Solar\.Overview')

## DiGi\.GIS\.PostgreSQL\.Solar\.Enums Namespace
### Enums

<a name='DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType'></a>

## BuildingSolarRadiationSolveOutcomeType Enum

Represents how a single building and radius solve of the batch solar radiation solve ended\.

```csharp
public enum BuildingSolarRadiationSolveOutcomeType
```
### Fields

<a name='DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType.Undefined'></a>

`Undefined` 0

The solve did not run: the run was cancelled before it started\.

<a name='DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType.Solved'></a>

`Solved` 1

The solve produced a result that can be stored\.

<a name='DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType.NotSolvable'></a>

`NotSolvable` 2

The building could not be solved at this radius, for a reason given with the outcome\.

<a name='DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType.OverCeiling'></a>

`OverCeiling` 3

The shading model holds more caster triangles than the ceiling allows\.

<a name='DiGi.GIS.PostgreSQL.Solar.Enums.BuildingSolarRadiationSolveOutcomeType.Failed'></a>

`Failed` 4

The solve threw, or produced nothing that can be stored\.