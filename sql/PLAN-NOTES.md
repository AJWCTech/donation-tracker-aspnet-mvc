# Plan notes: usp_GetDonationsBySupporter

Measured on 2 October 2026 against SQL Server 2022 LocalDB (16.0.1000.6) with
1,001 supporters and 100,001 donations. Every figure below is copied from the
captured output in `sql/plans/`; nothing is estimated.

## Result

| | Before the index | After the index |
|---|---|---|
| Logical reads on `Donations` (STATISTICS IO) | 402 | 4 |
| Operators (actual plan) | Index Seek on `IX_Donations_SupporterId` → Nested Loops → Clustered Index Seek on `PK_Donations` (key lookup) → Sort | Index Seek on `IX_Donations_SupporterId_DonatedOn` |
| Rows returned | 100 | 100 |
| Plan file | `plans/before.sqlplan` | `plans/after.sqlplan` |

STATISTICS IO lines, as printed:

- Before: `Table 'Donations'. Scan count 1, logical reads 402, physical reads 1, ... read-ahead reads 100`
- After: `Table 'Donations'. Scan count 1, logical reads 4, physical reads 0, ... read-ahead reads 0`

## What the plans show

**Before.** This was not a table scan. EF Core's first migration already created
`IX_Donations_SupporterId` for the foreign key, so SQL Server seeks that index
to find the supporter's 100 rows. That index only holds `SupporterId` and `Id`,
so each row needs a key lookup into the clustered index `PK_Donations` to fetch
`Amount` and `DonatedOn`, and then a Sort operator orders the rows by
`DonatedOn DESC`.

**After.** The new index is keyed on `(SupporterId, DonatedOn DESC)` and
includes `Amount`. One Index Seek returns everything the query needs, already
in the right order, so the key lookup, the Nested Loops join and the Sort all
disappear.

## How to reproduce

go-sqlcmd on this machine could not resolve `(localdb)\MSSQLLocalDB` by name, so
connect with the instance pipe name printed by `sqllocaldb info MSSQLLocalDB`
(shown here as `<pipe>`).

```
sqlcmd -S <pipe> -i sql\01_seed_volume.sql
sqlcmd -S <pipe> -i sql\02_usp_GetDonationsBySupporter.sql
sqlcmd -S <pipe> -d DonationTracker -y 0 -o sql\plans\before.output.txt -Q "SET STATISTICS IO ON; SET STATISTICS XML ON; EXEC dbo.usp_GetDonationsBySupporter @SupporterId = 500;"
sqlcmd -S <pipe> -i sql\03_ix_Donation_SupporterId_DonatedOn.sql
sqlcmd -S <pipe> -d DonationTracker -y 0 -o sql\plans\after.output.txt -Q "SET STATISTICS IO ON; SET STATISTICS XML ON; EXEC dbo.usp_GetDonationsBySupporter @SupporterId = 500;"
```

Each `.output.txt` file holds the result rows, the STATISTICS IO message and the
plan XML. The `.sqlplan` files are the `<ShowPlanXML>` element cut out of that
output, so they open as graphical plans in SSMS.

## Things to know

- The before run was the first read of the table after seeding (physical reads
  1, read-ahead reads 100), so its timing is not comparable; logical reads are.
- The seed script gives each supporter one repeated amount. That does not
  affect the plans but the data is not realistic.
- The index was created by hand, so EF Core's model snapshot does not know
  about it.
