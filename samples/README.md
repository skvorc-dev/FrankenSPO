# Samples

## SpoExample

Two projects that show FrankenSPO's working P1 features end to end on SQLite:

- `SpoExample.Module` (class library): a `Customer : SPOBaseObject` entity with a string, a decimal, an enum and a
  nullable UTC `DateTime`, and `CustomerModule : ISpoModule`. The source generator is referenced as an analyzer.
- `SpoExample.Console`: builds the model with `SqliteProvider`, opens a temporary SQLite file, adds a customer and saves,
  reads it back in a new ObjectSpace, updates it, reads it again and deletes the file. It exits with 0.

Run it from the repository root:

```bash
dotnet run --project samples/SpoExample.Console
```

Sample output (English; the sample's own text, not library messages). The ID and times differ on each run:

```text
Added    01a17d40-e601-739a-af90-c17042a40851 Ada Lovelace, tier Standard, credit 1250.75
Read     01a17d40-e601-739a-af90-c17042a40851 Ada Lovelace, tier Standard, credit 1250.75, last order 2026-10-24T12:06:07.6162301Z
Updated  01a17d40-e601-739a-af90-c17042a40851 Ada Lovelace, tier Gold, credit 1350.75
Checked  01a17d40-e601-739a-af90-c17042a40851 Ada Lovelace, tier Gold, credit 1350.75
```

### What it does not show yet

- Queries (`Where`, LINQ) and paging: S-05.
- Delete and soft delete: S-04 and S-10.
- Concurrency and version checks: S-10.
- Transactions after failure and uncertain commits: S-06.
- Schema mismatch on an existing database: S-02b and S-03c.
