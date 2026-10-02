# DonationTracker

[![CI](https://github.com/AJWCTech/donation-tracker-aspnet-mvc/actions/workflows/ci.yml/badge.svg)](https://github.com/AJWCTech/donation-tracker-aspnet-mvc/actions/workflows/ci.yml)

A small ASP.NET Core MVC practice app that records supporters and their
donations and shows a running campaign total. It uses EF Core with SQL Server
LocalDB, a repository and service layer, ASP.NET Core Identity for staff
sign-in, an audit trail, NUnit + Moq tests, hand-written SQL (stored procedure,
index, execution plans), a paged JSON API and a CI build.

## Run it

Needs the .NET 10 SDK and SQL Server LocalDB.

```
dotnet tool install --global dotnet-ef
dotnet ef database update --project DonationTracker.Web
dotnet user-secrets set "SeedStaff:Email" "you@example.com" --project DonationTracker.Web
dotnet user-secrets set "SeedStaff:Password" "YourPassword1!" --project DonationTracker.Web
dotnet run --project DonationTracker.Web --launch-profile http
```

Open http://localhost:5216. Anyone can view; sign in with the staff account
above to add or edit. The password needs upper case, lower case, a digit and a
symbol. The API is at `/api/donations?page=1&pageSize=25`.

Run the tests with `dotnet test`. See `docs/DEVELOPMENT.md`,
`docs/STUDY-NOTES.md` and `sql/PLAN-NOTES.md`.

## What I would do next

- Add roles, so an admin can manage staff accounts in the app.
- Add search on the supporters list.
- Add delete, recorded in the audit trail.
- Deploy it to Azure with Azure SQL.
