# DonationTracker

A small ASP.NET Core MVC practice app that records supporters and their
donations and shows a running campaign total. It uses EF Core with SQL Server
LocalDB, a repository and service layer, ASP.NET Core Identity for staff
sign-in, NUnit + Moq tests, hand-written SQL (stored procedure, index,
execution plans) and a paged JSON API.

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
- Add search, and a supporter page listing that supporter's donations.
- Add delete with an audit trail of who changed what.
- Add a CI build that runs the tests on every push.
