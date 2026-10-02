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
| Sign-in | ASP.NET Core Identity (Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.12) |
| Tests | NUnit 4.3.2, Moq 4.21.0, Microsoft.NET.Test.Sdk |

No other packages are used. There is no AutoMapper, MediatR, logging framework,
or Docker.

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
which the repository asks the database for with a single `SUM`.

The schema is managed by five EF Core migrations:

| Migration | What it does |
|---|---|
| `InitialCreate` | Both tables, the foreign key and an index on `Donations.SupporterId` |
| `AddDonationSupporterDateIndex` | Replaces that index with a covering index on `(SupporterId, DonatedOn DESC) INCLUDE (Amount)` |
| `AddIdentity` | The ASP.NET Core Identity tables for staff accounts |
| `AddDonationsBySupporterProcedure` | The stored procedure `usp_GetDonationsBySupporter`, as raw SQL |
| `AddAuditEntries` | The `AuditEntries` table |

## Paging

The donations page shows 25 rows at a time. The repository uses `Skip` and
`Take` over a fixed order (newest first, then by Id so ties cannot move between
pages). The service counts the rows, works out the number of pages and moves an
out-of-range page number to the nearest valid one.

## Sign-in

Anyone can view the lists and the API. Adding and editing need a signed-in
member of staff: both MVC controllers carry `[Authorize]`, with
`[AllowAnonymous]` on the list actions.

ASP.NET Core Identity stores the accounts and hashes the passwords. The login
page is a plain controller and view using `SignInManager`. There is no
registration page; the staff account is created at startup from the
`SeedStaff:Email` and `SeedStaff:Password` settings, which are kept in user
secrets so that no password is in the repository.

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

## Audit trail

Every add and edit writes one `AuditEntries` row: when, who (the signed-in
user), the action, which record, and a summary such as
`Amount 25.00 to 40.00`. The service builds the entry, because it knows the old
and new values; the repository saves it in the same database transaction as the
change, so there is never a change without its record. Rows are only inserted.
Signed-in staff can read the newest 100 at `/Audit`.

## Supporter page and the stored procedure

`/Supporters/Details/{id}` shows one supporter with their donations and total.
The donations come from the stored procedure, called through
`FromSqlInterpolated`, which sends the supporter Id as a SQL parameter rather
than pasting it into the SQL text.

## Continuous integration

`.github/workflows/ci.yml` restores, builds and runs every test on a Windows
runner (for LocalDB) on each push and pull request. `azure-pipelines.yml` is the
same build written for Azure Pipelines; it is a sketch and has not been run.

## API

`GET /api/donations?page=1&pageSize=25` returns one page of donations as JSON:
an `items` list of `DonationDto` (`Id`, `SupporterName`, `Amount`, `DonatedOn`)
plus `pageNumber`, `pageSize`, `totalCount` and `totalPages`. The page size is
limited to 100. The DTOs are mapped by hand in the service. The entity is never
returned, so the JSON contract is independent of the database model.

## Tests

There are two kinds, both in `DonationTracker.Tests`.

**Unit tests** cover `DonationService` with a mocked `IDonationRepository`:

- the campaign total is the figure the repository returns
- total pages round up, and out-of-range page numbers and sizes are corrected
- a future-dated donation is rejected and nothing is saved
- a donation for an unknown supporter is rejected and nothing is saved
- a valid donation calls the repository's add method exactly once
- the audit entry names who made the change and the old and new values
- supporter details add up that supporter's donations

**Integration tests** run the real `DonationRepository` against a real LocalDB
database, `DonationTracker_IntegrationTests`, which is created from the
migrations before the tests and dropped afterwards. They check that the total
is 0 with no rows, that it sums correctly, that the count is right and that
paging returns the newest rows first, that the stored procedure returns only
one supporter's donations, and that adding a donation also saves its audit
entry.

Run them all with `dotnet test`.

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
`sql/plans/`, with the write-up in `sql/PLAN-NOTES.md`. Once measured, the index
was moved into a migration so the EF Core model and the database agree.

## Project layout

```
DonationTracker.Web/
  Controllers/   MVC controllers and the API controller
  Data/          AppDbContext, repository interface and implementation
  Models/        Entities, the list view model and the API DTO
  Services/      Service interface, implementation and BusinessRuleException
  Views/         Razor views
  Migrations/    The five EF Core migrations
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
10. Documentation, home page and privacy policy
11. Campaign total as a SQL `SUM`, then paging
12. Integration tests
13. The index moved into a migration
14. Staff sign-in
15. Styling
16. Supporter page using the stored procedure
17. Audit trail
18. CI build

## Running locally

See the README. In short: apply the migrations, set the staff account in user
secrets, then
`dotnet run --project DonationTracker.Web --launch-profile http` and open
http://localhost:5216.

## Known limitations

- One staff account, created from configuration. There are no roles and no page
  for managing accounts.
- There are no delete pages.
- The audit trail is a plain-text summary, not a field-by-field history.
- The supporters list is not paged or searchable.
- The seed script gives each seeded supporter a single repeated amount.
