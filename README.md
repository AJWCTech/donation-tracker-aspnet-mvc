# DonationTracker

A small ASP.NET Core MVC practice app that records supporters and their
donations and shows a running campaign total. It uses EF Core with SQL Server
LocalDB, a repository and service layer, NUnit + Moq tests, hand-written SQL
(stored procedure, index, execution plans) and one JSON API endpoint.

## Run it

Needs the .NET 10 SDK and SQL Server LocalDB.

```
dotnet tool install --global dotnet-ef
dotnet ef database update --project DonationTracker.Web
dotnet run --project DonationTracker.Web --launch-profile http
```

Then open http://localhost:5216. The API is at
http://localhost:5216/api/donations.

Run the tests with `dotnet test`. The SQL scripts and measured plans are in
`sql/` (see `sql/PLAN-NOTES.md`); interview notes are in `docs/STUDY-NOTES.md`.

## What I would do next

- Page the donations list and the API instead of capping both at 100 rows.
- Compute the campaign total with a SQL `SUM` rather than loading every row.
- Add integration tests that run the repository against a real database.
- Add authentication so only signed-in staff can add or edit records.
- Move the hand-made index into an EF migration so the model and database agree.
