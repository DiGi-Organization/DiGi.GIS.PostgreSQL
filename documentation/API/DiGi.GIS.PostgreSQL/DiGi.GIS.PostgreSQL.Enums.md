#### [DiGi\.GIS\.PostgreSQL](DiGi.GIS.PostgreSQL.Overview.md 'DiGi\.GIS\.PostgreSQL\.Overview')

## DiGi\.GIS\.PostgreSQL\.Enums Namespace
### Enums

<a name='DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType'></a>

## AdministrativeArealType Enum

Represents the type of administrative area\.

The value is the `type_id` column of `administrative_areal_2d` and travels the wire as an <b>integer</b>. One row is stored per polygon part of a unit, so a level holds more rows than there are real units - counties are 406 rows for 380 codes, and both country and voivodeship are 406 rows because every county part carries its own ancestor chain.

```csharp
public enum AdministrativeArealType
```
### Fields

<a name='DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType.Undefined'></a>

`Undefined` -1

Undefined administrative area type\.

<a name='DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType.Country'></a>

`Country` 0

Country level administrative area\.

<a name='DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType.Voivodeship'></a>

`Voivodeship` 1

Voivodeship \(province\) level administrative area\.

<a name='DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType.County'></a>

`County` 2

County \(powiat\) level administrative area\.

<a name='DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType.Municipality'></a>

`Municipality` 3

Municipality \(gmina\) level administrative area\.

<a name='DiGi.GIS.PostgreSQL.Enums.AdministrativeArealType.Subdivision'></a>

`Subdivision` 4

Subdivision level administrative area\.

Renamed from the misspelled `Subdivison` (missing the second `i`), which was the accepted wire token until this build. `Subdivison` is now <b>rejected</b> with HTTP 400 - send `Subdivision` or the integer `4`. The value is unchanged, so nothing stored under `type_id` is affected.

<a name='DiGi.GIS.PostgreSQL.Enums.BuildingDataUpdateType'></a>

## BuildingDataUpdateType Enum

Specifies the building data update type\.

```csharp
public enum BuildingDataUpdateType
```
### Fields

<a name='DiGi.GIS.PostgreSQL.Enums.BuildingDataUpdateType.General'></a>

`General` 0

General data update type, which mostly includes Building2D\.

<a name='DiGi.GIS.PostgreSQL.Enums.BuildingDataUpdateType.Database'></a>

`Database` 1

Database data update type\.

<a name='DiGi.GIS.PostgreSQL.Enums.BuildingDataUpdateType.Occupancy'></a>

`Occupancy` 2

Occupancy data update\.

<a name='DiGi.GIS.PostgreSQL.Enums.BuildingDataUpdateType.RadialRatios'></a>

`RadialRatios` 3

Radial ratios \(Radial Building Coverage Ratio, Radial Floor Area Ratio\) update\.

<a name='DiGi.GIS.PostgreSQL.Enums.BuildingDataUpdateType.Statistical'></a>

`Statistical` 4

Statistical demographic data update\.

<a name='DiGi.GIS.PostgreSQL.Enums.BuildingDataUpdateType.YearBuilt'></a>

`YearBuilt` 5

Year built data update: writes the predicted, user and calculated year\-built columns\.

<a name='DiGi.GIS.PostgreSQL.Enums.OpenBuildingMatchClass'></a>

## OpenBuildingMatchClass Enum

Names how a footprint of an open building dataset relates to the BDOT10k footprints it overlaps: the class of the connected component of the overlap graph it belongs to \(DiGi\.GIS\.PostgreSQL\#127\)\.

The values are stored as integers in `building_dataset_match.match_class`, so they are explicit and never renumbered.

```csharp
public enum OpenBuildingMatchClass
```
### Fields

<a name='DiGi.GIS.PostgreSQL.Enums.OpenBuildingMatchClass.Undefined'></a>

`Undefined` 0

Not classified\.

<a name='DiGi.GIS.PostgreSQL.Enums.OpenBuildingMatchClass.OneToOne'></a>

`OneToOne` 1

One dataset footprint and one BDOT10k footprint whose intersection over union reaches the threshold: the pair the geometry measures \(IoU, area ratio, offset\) are computed over\.

<a name='DiGi.GIS.PostgreSQL.Enums.OpenBuildingMatchClass.OneToOneWeak'></a>

`OneToOneWeak` 2

One dataset footprint and one BDOT10k footprint joined by an edge but with an intersection over union below the threshold\. Counted as matched for completeness and left out of the geometry measures\.

<a name='DiGi.GIS.PostgreSQL.Enums.OpenBuildingMatchClass.OneToMany'></a>

`OneToMany` 3

One dataset footprint and several BDOT10k footprints: the dataset merges buildings the register keeps apart\.

<a name='DiGi.GIS.PostgreSQL.Enums.OpenBuildingMatchClass.ManyToOne'></a>

`ManyToOne` 4

Several dataset footprints and one BDOT10k footprint: the dataset splits a building the register keeps whole\.

<a name='DiGi.GIS.PostgreSQL.Enums.OpenBuildingMatchClass.ManyToMany'></a>

`ManyToMany` 5

Several dataset footprints and several BDOT10k footprints connected through their overlaps\.

<a name='DiGi.GIS.PostgreSQL.Enums.OpenBuildingMatchClass.Omission'></a>

`Omission` 6

A BDOT10k footprint no dataset footprint overlaps enough: missing from the dataset\.

<a name='DiGi.GIS.PostgreSQL.Enums.OpenBuildingMatchClass.Commission'></a>

`Commission` 7

A dataset footprint no BDOT10k footprint overlaps enough: present in the dataset only\.

<a name='DiGi.GIS.PostgreSQL.Enums.OpenBuildingMatchClass.Excluded'></a>

`Excluded` 8

A footprint smaller than the run's minimum area, on either side: left out of the matching and recorded so the snapshot still accounts for it\.

<a name='DiGi.GIS.PostgreSQL.Enums.OrtoAreaObjectRemoval'></a>

## OrtoAreaObjectRemoval Enum

Names the outcome of removing a polygon drawn in a full\-interpretation test area \([RemoveAsync\(NpgsqlConnection, string, string, string, int, CancellationToken\)](DiGi.GIS.PostgreSQL.Classes.md#DiGi.GIS.PostgreSQL.Classes.OrtoAreaObjectPostgreSQLConverter.RemoveAsync(Npgsql.NpgsqlConnection,string,string,string,int,System.Threading.CancellationToken) 'DiGi\.GIS\.PostgreSQL\.Classes\.OrtoAreaObjectPostgreSQLConverter\.RemoveAsync\(Npgsql\.NpgsqlConnection, string, string, string, int, System\.Threading\.CancellationToken\)')\)\.

Drawn polygons are shared between interpreters, and their labels feed the agreement between them, so a removal is refused unless the caller drew the polygon and no other interpreter has labelled it. The reason is reported because it decides what the caller is told: a polygon someone else drew, or one someone else has already marked, has to stay.

```csharp
public enum OrtoAreaObjectRemoval
```
### Fields

<a name='DiGi.GIS.PostgreSQL.Enums.OrtoAreaObjectRemoval.Undefined'></a>

`Undefined` 0

The outcome is not known\.

<a name='DiGi.GIS.PostgreSQL.Enums.OrtoAreaObjectRemoval.Removed'></a>

`Removed` 1

The polygon and the caller's own labels on it were removed\.

<a name='DiGi.GIS.PostgreSQL.Enums.OrtoAreaObjectRemoval.NotFound'></a>

`NotFound` 2

The area holds no polygon under the identifier \(also when it was removed already, or the table does not exist yet\)\.

<a name='DiGi.GIS.PostgreSQL.Enums.OrtoAreaObjectRemoval.NotCreator'></a>

`NotCreator` 3

Another interpreter drew the polygon; only the one who drew it may remove it\.

<a name='DiGi.GIS.PostgreSQL.Enums.OrtoAreaObjectRemoval.LabelledByOthers'></a>

`LabelledByOthers` 4

Another interpreter has labelled the polygon; removing it would delete their labels, so it stays\.

<a name='DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass'></a>

## PopulationDensityClass Enum

The population density class a census grid cell falls into, by its population per 1 km² cell \(DiGi\.GIS\.PostgreSQL\#106\)\.

[Masked](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass.Masked 'DiGi\.GIS\.PostgreSQL\.Enums\.PopulationDensityClass\.Masked') is a cell whose value statistical secrecy hides - a missing value, not a zero - and [Zero](DiGi.GIS.PostgreSQL.Enums.md#DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass.Zero 'DiGi\.GIS\.PostgreSQL\.Enums\.PopulationDensityClass\.Zero') is a cell the census reports as empty. The two are kept apart so a comparison can exclude the masked cells from its metrics and say how many it excluded, while an empty cell stays a real observation of an error of exactly the modelled sum.

The band boundaries are [PopulationDensity](DiGi.GIS.PostgreSQL.Constants.md#DiGi.GIS.PostgreSQL.Constants.PopulationDensity 'DiGi\.GIS\.PostgreSQL\.Constants\.PopulationDensity'); they are a reporting classification of the census population, never of the modelled one.

```csharp
public enum PopulationDensityClass
```
### Fields

<a name='DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass.Undefined'></a>

`Undefined` 0

The class is not known \- a value outside every band, for example a negative population no census publishes\.

<a name='DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass.Masked'></a>

`Masked` 1

The cell's population is hidden by statistical secrecy; the stored value is null\.

<a name='DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass.Zero'></a>

`Zero` 2

The census reports the cell as empty\.

<a name='DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass.VeryLow'></a>

`VeryLow` 3

Up to and including 50 persons per km²\.

<a name='DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass.Low'></a>

`Low` 4

51 to 200 persons per km²\.

<a name='DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass.Medium'></a>

`Medium` 5

201 to 1000 persons per km²\.

<a name='DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass.High'></a>

`High` 6

1001 to 5000 persons per km²\.

<a name='DiGi.GIS.PostgreSQL.Enums.PopulationDensityClass.VeryHigh'></a>

`VeryHigh` 7

Above 5000 persons per km²\.

<a name='DiGi.GIS.PostgreSQL.Enums.UpdateRejectionReason'></a>

## UpdateRejectionReason Enum

Names why a row handed to an update never reached the database\.

A row that cannot be filed under a county part is dropped rather than written, which used to leave no trace at all: the caller received only the identifiers of the rows that were stored, so a batch that stored one of five thousand looked exactly like a batch that stored all of them. The reason is carried alongside the reference because it decides what the caller should do next - a payload defect is worth correcting and reposting, a footprint that falls outside every candidate part is not.

```csharp
public enum UpdateRejectionReason
```
### Fields

<a name='DiGi.GIS.PostgreSQL.Enums.UpdateRejectionReason.Undefined'></a>

`Undefined` 0

The element itself was null, so there is nothing to name\.

<a name='DiGi.GIS.PostgreSQL.Enums.UpdateRejectionReason.MissingGeometry'></a>

`MissingGeometry` 1

The row carries no bounding box, so no county could even be attempted\. A defect in the posted payload\.

<a name='DiGi.GIS.PostgreSQL.Enums.UpdateRejectionReason.CountyUnresolved'></a>

`CountyUnresolved` 2

County resolution ran and named no part\. Not necessarily the caller's fault \- the last tier decides by geometry, and a footprint falling outside every candidate part lands here\.

<a name='DiGi.GIS.PostgreSQL.Enums.UpdateRejectionReason.PartitionUnavailable'></a>

`PartitionUnavailable` 3

The county resolved, but its partition could not be created, so every row filed under it was dropped\. Server\-side\.

<a name='DiGi.GIS.PostgreSQL.Enums.UpdateRejectionReason.MissingCounty'></a>

`MissingCounty` 4

The caller did not state a county identifier, and the row carries no geometry from which one could be inferred\. A defect in the posted payload\.

<a name='DiGi.GIS.PostgreSQL.Enums.UpdateRejectionReason.GeometryRejected'></a>

`GeometryRejected` 5

The row carries a coordinate reference system the reprojection does not support, or a transformation of its geometry failed \(a null input, a non\-finite vertex, a ring shorter than four ordinates, or a result outside the EPSG:2180 domain\)\. The footprint is counted and never written \- a partially reprojected ring is not a row \(DiGi\.GIS\.PostgreSQL\#125\)\.

<a name='DiGi.GIS.PostgreSQL.Enums.UpdateRejectionReason.NoImages'></a>

`NoImages` 6

The orthophoto series holds no image\. Geoportal answers an image even outside its coverage, so a series with none is a fetch in which every request failed; storing it would record that failure as data and let its queue claim be acknowledged, so a transient outage became permanent \(DiGi\.GIS\.WebAPI\#81\)\. The row is never written, which leaves the claim open for a retry\.