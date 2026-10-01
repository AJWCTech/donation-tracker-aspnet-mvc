# Development notes

How DonationTracker is put together, the decisions behind it, and how to work
on it.

## What it is

A small ASP.NET Core MVC application for recording supporters and the donations
they make, with a running campaign total. It is a practice project: the scope is
deliberately small so that every layer can be read and explained in one sitting.

## Stack

| Area | Choice |
|---|---|
| Runtime | .NET 10 (`net10.0`) |
| Web | ASP.NET Core MVC with Razor views and tag helpers |
| UI | The template's Bootstrap and jQuery validation, unstyled beyond that |
| Data access | Entity Framework Core 10.0.12, SQL Server provider |
| Database | SQL Server 2022 LocalDB |
| Tests | NUnit 4.3.2, Moq 4.21.0, Microsoft.NET.Test.Sdk |

No other packages are used. There is no AutoMapper, MediatR, logging framework,
authentication, Docker or CI.

## Architecture

```
Controller  ->  IDonationService  ->  IDonationRepository  ->  AppDbContext  ->  SQL Server
```

- **Controllers** handle HTTP only: bind the form, check `ModelState`, call the
  service, choose a view or a redirect.
- **`DonationService`** holds every business rule and never touches the
  `DbContext`. It depends only on the repository interface.
- **`DonationRepository`** is the only class that uses `AppDbContext`. It loads
  and saves and has no rules in it.

The point of the split is testability: the service can be unit tested with a
mocked repository and no database. Both are registered as scoped in
`Program.cs`, matching the lifetime of the `DbContext` they sit on.

## Data model

| Entity | Fields |
|---|---|
| `Supporter` | `Id`, `FullName` (required, max 100), `Email` (required, valid email, max 254), `CreatedOn` |
| `Donation` | `Id`, `SupporterId` (foreign key), `Amount` (`decimal(10,2)`, 0.01 to 1,000,000), `DonatedOn` |

There is no Campaign table. The campaign total is the sum of all donations,
computed in the service.

The schema comes from a single EF Core migration, `InitialCreate`, which creates
both tables, the foreign key and an index on `Donations.SupporterId`.

## Validation

Rules are enforced in three places, each for a different reason:

1. **Data annotations on the models** state the rule once (`Required`,
   `StringLength`, `EmailAddress`, `Range`).
2. **The browser** enforces those annotations through the template's jQuery
   validation scripts, for immediate feedback.
3. **The server** checks `ModelState.IsValid` in every POST action, because
   client-side checks can be bypassed.

Two rules need more than the posted values, so they live in the service: a
donation cannot be dated in the future, and its supporter must exist. The
service throws `BusinessRuleException`; the controller catches it and shows the
message on the form.

## API

`GET /api/donations` returns the 100 most recent donations as JSON. The response
is a list of `DonationDto` (`Id`, `SupporterName`, `Amount`, `DonatedOn`), mapped
by hand in the service. The entity is never returned, so the JSON contract is
independent of the database model.

## Tests

`DonationTracker.Tests` covers `DonationService` with a mocked
`IDonationRepository`:

- the campaign total sums the donation amounts
- the total is 0 when there are no donations
- a future-dated donation is rejected and nothing is saved
- a donation for an unknown supporter is rejected and nothing is saved
- a valid donation calls the repository's add method exactly once

Run them with `dotnet test`.

## SQL work

The scripts in `sql/` are written by hand and applied with `sqlcmd`, not through
EF Core:

| Script | Purpose |
|---|---|
| `01_seed_volume.sql` | Adds 1,000 supporters and 100,000 donations so execution plans are meaningful |
| `02_usp_GetDonationsBySupporter.sql` | Stored procedure returning one supporter's donations, newest first |
| `03_ix_Donation_SupporterId_DonatedOn.sql` | Covering index on `(SupporterId, DonatedOn DESC) INCLUDE (Amount)` |

The procedure was run with `SET STATISTICS IO ON` and the actual execution plan
captured before and after creating the index. Logical reads on `Donations` went
from 402 to 4, and the plan changed from an index seek plus a key lookup and a
sort to a single index seek. The measured output and both plans are in
`sql/plans/`, with the write-up in `sql/PLAN-NOTES.md`.

## Project layout

```
DonationTracker.Web/
  Controllers/   MVC controllers and the API controller
  Data/          AppDbContext, repository interface and implementation
  Models/        Entities, the list view model and the API DTO
  Services/      Service interface, implementation and BusinessRuleException
  Views/         Razor views
  Migrations/    The InitialCreate migration
DonationTracker.Tests/   NUnit + Moq tests for the service
sql/                     Hand-written scripts, plans and plan notes
docs/                    This file and the study notes
```

## Build order

The commit history follows the order the app was built in, one step per commit:

1. Solution and MVC project
2. EF Core, entities, `AppDbContext` and the migration
3. Repository and service, registered as scoped
4. List, create and edit pages for supporters and donations
5. Validation on the models, in the controllers, in the browser and in the service
6. Build and route verification
7. Unit tests for the service
8. SQL scripts, stored procedure, index and measured plans
9. The JSON API
10. Documentation

## Running locally

See the README. In short: apply the migration, then
`dotnet run --project DonationTracker.Web --launch-profile http` and open
http://localhost:5216.

## Known limitations

- The donations list and the API are capped at the 100 most recent rows rather
  than paged.
- The campaign total loads every donation and sums in memory. A SQL `SUM` would
  be the next change.
- There is no authentication and there are no delete pages.
- The covering index was created by script, so EF Core's model snapshot does not
  include it.
- The seed script gives each seeded supporter a single repeated amount.
