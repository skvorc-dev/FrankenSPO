# E1 · SQLite PoC (phase P1)

Milestone **P1 — SQLite PoC**. Contract: `docs/POC_ACCEPTANCE.md`, `docs/architecture/*.md`, decisions D-29…D-36. All cards are `Backlog` until E0 is done;
the main agent completes each card (files, exact acceptance) before moving it to `Ready`.

### S-01 · Generator: entity declaration and stable metadata IDs
- Issue: #5 · Size: M · Status: Done (PR #53, sonnet)
- Goal: a class declared per D-37 compiles to generated, reflection-free metadata with its stable entity ID and
  property numbers; invalid declarations fail the build with clear diagnostics.
- Acceptance:
  - [x] `Sferix.SPO.Abstractions`: `SpoEntityAttribute(string id)`, `SpoFieldAttribute(int number)`, and immutable
        metadata types `EntityMetadata` (EntityId, CLR name, properties) and `PropertyMetadata` (number, CLR name,
        CLR type, nullable). XML docs per the code style rules.
  - [x] `Sferix.SPO.Generators` (incremental generator, Roslyn 5.0.0 per D-25): for each `[SpoEntity]` partial class it
        emits a `public static EntityMetadata SpoMetadata { get; }` member in the same partial class
  - [x] Supported property types: `bool`, `int`, `long`, `decimal`, `double`, `string`, `Guid`, `DateTime`,
        `DateTimeOffset`, `byte[]`, their nullable forms, and enums (stored as their underlying integer)
  - [x] Diagnostics (errors, IDs `SPO0001`…): class not `partial`; entity ID missing or not a GUID; same entity ID
        twice in a compilation; `[SpoField]` number ≤ 0 or duplicated within the entity; unsupported property type;
        `[SpoField]` on a property without both getter and setter
  - [x] New test project `tests/Sferix.SPO.Tests.Generators` (xUnit, added to `FrankenSPO.slnx`) runs the generator
        with `CSharpGeneratorDriver`: one test per diagnostic, plus a valid entity whose generated metadata is checked
        field by field; also a compile-and-run test that reads `SpoMetadata` without reflection
  - [x] `Core` still references no provider; no runtime reflection on the metadata path; build 0 warnings; CI green
  - [x] `features.json`: `source_generated_metadata` → `tested` only if all of the above pass
- Files: `src/Sferix.SPO.Abstractions/**`, `src/Sferix.SPO.Generators/**`, `tests/Sferix.SPO.Tests.Generators/**`,
  `FrankenSPO.slnx`, `docs/compatibility/features.json`
- Dependency allowed: `Microsoft.CodeAnalysis.CSharp` 5.0.0 in the generator test project (record in THIRD-PARTY-NOTICES)
- Not in scope: `SPOBaseObject` and its columns (S-03), schema (S-02), model revisions, the GUID code fix, tracking code
- Notes: D-25, D-35, D-37, `docs/architecture/modules.md` rule 11.

### S-01b · Generator edge cases (review of #53)
- Issue: #5 · Size: S · Status: Done (PR #59, sonnet-2)
- Goal: the generator fails closed on every declaration it cannot handle and never crashes.
- Acceptance:
  - [ ] `SPO0007` error: generic entity class (today it emits code that does not compile)
  - [ ] `SPO0008` error: `[SpoEntity]` on a `record`, `struct` or `record struct` (today a record is silently ignored)
  - [ ] Hint names cannot collide: `A.B_C` and `A_B.C` in one compilation both generate (test)
  - [ ] The SPO0002 message's suggested GUID no longer makes the model unequal between runs (generate it in the
        output step, not in the model)
  - [ ] Tests for each; build 0 warnings; CI green; update `AnalyzerReleases.Unshipped.md`
- Files: `src/Sferix.SPO.Generators/**`, `tests/Sferix.SPO.Tests.Generators/**`

### S-02 · Provider contract, capabilities and model activation
- Issue: #5 · Size: M · Status: Done (PR #66, sonnet-2)
- Goal: a provider declares its capabilities, Core builds a compiled model from generated metadata and rejects it at
  activation when a mandatory capability is missing (D-33, D-34, D-39); metadata carries the storage type of enums.
- Acceptance:
  - [ ] `PropertyMetadata.EnumUnderlyingType` (`Type?`): the enum's underlying integer type, `null` for non-enum
        properties; the generator emits it as `typeof(...)` (no reflection). Update the class `<remarks>`
  - [ ] Generator error `SPO0009`: enum property whose underlying type is `ulong` (does not fit a signed 64-bit
        column); test; `AnalyzerReleases.Unshipped.md` updated
  - [ ] `Sferix.SPO.Abstractions/Providers`: `SpoCapability` enum with `AtomicSaveChanges` only, and `ISpoProvider`
        with `string Name` and `IReadOnlySet<SpoCapability> Capabilities`. Nothing else public in this card
  - [ ] `SpoCapabilityMissingException` (Abstractions, public, sealed) names the capability and the provider, no SQL;
        it has a stable code and its message comes from `.resx` resources in en, de, hr, es (D-40, D-41)
  - [ ] `Sferix.SPO.Core/Model`: internal `CompiledModel.Build(IEnumerable<EntityMetadata>, ISpoProvider,
        IReadOnlySet<SpoCapability> required)` = activation: rejects duplicate entity IDs, entity/property names that
        fail `SqlIdentifier`, and every required capability the provider lacks. `InternalsVisibleTo` the test projects
  - [ ] `Sferix.SPO.Provider.SQLite`: `SqliteProvider : ISpoProvider` declares `AtomicSaveChanges` and opens
        connections with `journal_mode=WAL` (file databases), `foreign_keys=ON` and a busy timeout (D-34);
        `SqliteConnectionFactory` is folded into it or kept internal
  - [ ] Tests: activation succeeds for a valid model; fails for each rejection above (Tests.Unit with a capability
        stub, which is not a database mock); SQLite pragmas read back from a real temp-file database (Tests.SQLite);
        enum underlying type for `byte`, `int` and `long` enums (Tests.Generators)
  - [ ] Build 0 warnings; CI green; PR notes the public API change of `PropertyMetadata` under
        "public API compatibility"
- Files: `src/Sferix.SPO.Abstractions/Metadata/PropertyMetadata.cs`, `src/Sferix.SPO.Abstractions/Providers/**`,
  `src/Sferix.SPO.*/Resources/**` (`.resx`, D-40),
  `src/Sferix.SPO.Core/Model/**`, `src/Sferix.SPO.Core/Sferix.SPO.Core.csproj`, `src/Sferix.SPO.Provider.SQLite/**`,
  `src/Sferix.SPO.Generators/**`, `tests/Sferix.SPO.Tests.{Unit,SQLite,Generators}/**`
- Not in scope: schema model and DDL (S-02b), policies (S-11), registration API `AddModule<T>()`, ObjectSpace.
- Notes: D-33, D-34, D-36, D-39, D-40, D-41; reuse the resource pattern and check from S-13; owner question 1 from #53. The resource and exception-code pattern set
  here is the one later cards copy; keep it small (one `.resx` per assembly, no localization framework).

### S-02b · Neutral schema model, SQLite DDL, create and reopen
- Issue: #5 · Size: M · Status: Done (PR #69, sonnet-2)
- Goal: create and reopen a SQLite database with deterministic first-time schema and validation against the model.
- Acceptance:
  - [ ] `Sferix.SPO.Abstractions/Schema`: immutable `SchemaModel` (tables, columns, optional primary-key columns)
        and `SpoStorageType` (Boolean, Int32, Int64, Decimal, Double, String, Guid, DateTime, DateTimeOffset, Binary);
        enums map to Int32 or Int64 by `EnumUnderlyingType`; `ISpoSchemaDialect` (separate file) renders DDL
  - [ ] `Sferix.SPO.Core/Schema`: builds the schema model from a `CompiledModel` (table = entity class name,
        column = property name, both through `SqlIdentifier`); output is identical across runs (ordered by field number)
  - [ ] Two entities whose class names give the same table name (e.g. `A.Customer`, `B.Customer`) fail when the
        schema model is built, before anything is written, with a coded `SpoModelException` (D-42); test
  - [ ] SQLite dialect: Boolean/Int32/Int64 → INTEGER, Double → REAL, Decimal/String/DateTimeOffset → TEXT,
        Guid/Binary → BLOB, DateTime → INTEGER; NOT NULL from nullability; quoted identifiers; DDL in one transaction
  - [ ] Ensure-schema: empty database → creates all tables; existing database that matches → no change; missing
        table, missing/extra column, wrong type or nullability → `SpoSchemaMismatchException` naming table and
        column, nothing changed (no migrations, providers.md); message from `.resx` (en, de, hr, es) with a stable code (D-40, D-41)
  - [ ] Tests on a real temp-file database: create, reopen, each mismatch; generated DDL snapshot per storage type
  - [ ] Build 0 warnings; CI green
- Files: `src/Sferix.SPO.Abstractions/Schema/**`, `src/Sferix.SPO.Core/Schema/**`, `src/Sferix.SPO.*/Resources/**`,
  `src/Sferix.SPO.Provider.SQLite/Schema/**`, `tests/Sferix.SPO.Tests.SQLite/**`
- Not in scope: primary key from `SPOBaseObject` (S-03 sets it), indexes, foreign keys between entities, migrations.
- Notes: D-32 (63-character identifiers), D-34.

### S-03a · `SPOBaseObject`, inherited fields and generated accessors
- Issue: #5 · Size: M · Status: Done (PR #73, sonnet-2)
- Goal: entities derive from `SPOBaseObject`, the generator includes its fields, and generated metadata can create
  instances and read/write every property without reflection (D-35, D-37, D-43).
- Acceptance:
  - [ ] `Sferix.SPO.Abstractions/Entities/SPOBaseObject.cs`: public abstract class with `[SpoField]` properties
        `Id` (Guid, 1000001), `CreatedAt` (DateTime, 1000002), `ModifiedAt` (DateTime, 1000003), `IsDeleted` (bool,
        1000004), `DeletedAt` (DateTime?, 1000005), `Version` (long, 1000006); XML docs say the behaviour arrives in
        S-03c, S-04 and S-10 and is not active yet
  - [ ] Generator: includes inherited `[SpoField]` properties of `SPOBaseObject` first, then the entity's own, each
        ordered by number; an entity may derive only from `object` or `SPOBaseObject` → else error `SPO0010`;
        an entity field number ≥ 1000000 → error `SPO0011` (reserved for the framework, D-43); both in en, de, hr, es
  - [ ] Generated metadata: `EntityMetadata` gains `Func<object> CreateInstance`; `PropertyMetadata` gains
        `Func<object, object?> GetValue` and `Action<object, object?> SetValue`, emitted as lambdas (no reflection);
        a compile-and-run test round-trips every supported type, including nullable and enum
  - [ ] `SchemaBuilder`: an entity with `SPOBaseObject` gets primary key `Id`; test on real SQLite
  - [ ] Build 0 warnings; CI green; PR notes the public API additions
- Files: `src/Sferix.SPO.Abstractions/Entities/**`, `src/Sferix.SPO.Abstractions/Metadata/**`,
  `src/Sferix.SPO.Generators/**`, `src/Sferix.SPO.Core/Schema/**`, `tests/Sferix.SPO.Tests.Generators/**`,
  `tests/Sferix.SPO.Tests.SQLite/**`
- Not in scope: setting Id/timestamps/version (S-03c, S-04, S-10), declared keys without `SPOBaseObject`, modules.
- Notes: D-35, D-37, D-42, D-43.

### S-03b · Modules and public model activation
- Issue: #5 · Size: S · Status: Done (PR #71, haiku-2)
- Goal: an application registers its entity modules and activates the model through a small public API (D-36, D-43).
- Acceptance:
  - [ ] `Sferix.SPO.Abstractions/Modules/ISpoModule.cs`: `IEnumerable<EntityMetadata> Entities { get; }`
  - [ ] `Sferix.SPO.Core/Model`: public sealed `SpoModelBuilder` with `AddModule<TModule>() where TModule :
        ISpoModule, new()` (fluent, a module added twice counts once) and `SpoModel Build(ISpoProvider provider)`, which
        runs `CompiledModel.Build` (activation) with the provider's required capabilities (`AtomicSaveChanges`)
  - [ ] public sealed `SpoModel` exposes nothing but the activated entity list (`IReadOnlyList<EntityMetadata>`)
  - [ ] `SpoModelException` becomes public (D-42: a public API now throws it) with `Code`; its XML docs list
        SPO1002, SPO1003, SPO1005, SPO1006
  - [ ] Tests (Tests.Unit): two modules activate; duplicate entity across modules fails with SPO1002; missing
        capability fails with SPO1001; a module added twice is registered once
  - [ ] Build 0 warnings; CI green; PR notes the public API additions
- Files: `src/Sferix.SPO.Abstractions/Modules/**`, `src/Sferix.SPO.Core/Model/**`, `tests/Sferix.SPO.Tests.Unit/Model/**`
- Not in scope: the module-name analyzer (`*Module` suffix, D-36: later card), ObjectSpace, DI integration.
- Notes: D-36, D-42, D-43. Parallel to S-03a: do not touch `Generators`, `Abstractions/Metadata`, `Core/Schema`.

### S-03c · Provider data contract and SQLite row access
- Issue: #5 · Size: M · Status: In Progress (sonnet-2)
- Goal: Core can insert, update and read one row by key through a provider-neutral contract; SQLite executes it with
  parameters inside a transaction, with exact value conversion (D-29, D-34, D-44).
- Acceptance:
  - [ ] `Sferix.SPO.Abstractions/Providers`: `ISpoProvider.Open(string connectionString)` returns `ISpoConnection`
        (`IDisposable`, `IAsyncDisposable`) with `EnsureSchema`, `BeginTransaction` (→ `ISpoTransaction` with
        `Commit`/`Rollback`), `ReadRow(SchemaTable, Guid key)`, `Insert(transaction, SchemaTable, values)`,
        `Update(transaction, SchemaTable, Guid key, changed columns and values)` returning rows affected. Each as a
        sync method and an `…Async` twin with `CancellationToken` last; values in schema column order
  - [ ] SQLite: SQL built only from validated, quoted identifiers; **every value is a parameter**; conversions per
        D-44 both ways; a non-UTC `DateTime` fails with a coded exception (SPO1007, en/de/hr/es), nothing written
  - [ ] Tests on a real temp-file database: round-trip of every storage type incl. null, enum, `decimal` scale
        (`1.50m`), UUIDv7 byte order (later Guid sorts later), `DateTimeOffset` offset kept; update changes only the
        given columns; rollback leaves the table unchanged; values with quotes, `' OR 1=1 --`, `;` and `%`/`_` are
        stored and read back literally
  - [ ] Build 0 warnings; CI green; PR lists the public API additions
- Files: `src/Sferix.SPO.Abstractions/Providers/**`, `src/Sferix.SPO.Provider.SQLite/**` (new folder `Data/`),
  `tests/Sferix.SPO.Tests.SQLite/**`
- Not in scope: ObjectSpace (S-03d), queries (S-05), version check (S-10), uncertain commit (S-06).
- Notes: D-29, D-34, D-42, D-44. Core uses one internal code path for sync and async (`bool async` pattern), never
  blocking on async; the provider implements both. S-14 runs in parallel: do not edit `SqliteMessages*.resx` keys it
  might audit — add new keys only.

### S-03d · ObjectSpace: add, get, update, identity map, UUIDv7
- Issue: #5 · Size: M · Status: Backlog · after S-03c
- Goal: an application opens an ObjectSpace from `SpoModel`, adds, loads and updates `SPOBaseObject` entities on real
  SQLite in one atomic SaveChanges; identity map by entity ID + key; single-threaded misuse detected (D-29, D-35).
  Card completed when S-03c is merged (it settles the API shape).

### S-15 · Generator: abstract entities and hidden base members
- Issue: #5 · Size: S · Status: Backlog · after S-14
- Goal: the generator rejects what the ObjectSpace cannot handle, before runtime (review of #73).
- Acceptance:
  - [ ] Error `SPO0012`: `[SpoEntity]` on an `abstract` class (no inheritance mapping in P1, D-35)
  - [ ] Error `SPO0013`: an entity member hides a `SPOBaseObject` member (`new` or same name)
  - [ ] SPO0006's message also names init-only setters (since #73 they are rejected); en, de, hr, es
  - [ ] Tests for each; `AnalyzerReleases.Unshipped.md`; build 0 warnings; CI green
- Files: `src/Sferix.SPO.Generators/**`, `tests/Sferix.SPO.Tests.Generators/**`
- Notes: D-35, D-43.

### S-04 · Scalar tracking, audit timestamps, SoftDelete
- Issue: #5 · Size: M · Status: Backlog · after S-03d
- Goal: per-type comparers, failed save keeps values dirty (D-30); UTC timestamps via `TimeProvider`; Deleted state,
  `IncludeDeleted()` (D-35).

### S-05 · LINQ core subset with parameters and validated identifiers
- Issue: #6 · Size: M · Status: Backlog · after S-03
- Goal: Where, string StartsWith/EndsWith/Contains, OrderBy/ThenBy, Skip/Take, Select to DTO, First/Single/Count/Any
  (D-32); `SpoQueryNotSupportedException` for the rest.

### S-05b · GroupBy and aggregates
- Issue: #6 · Size: M · Status: Backlog · after S-05
- Goal: GroupBy and Sum/Min/Max/Average with the same parameter and identifier rules (D-32).

### S-06 · Transactions: commit, rollback, uncertain commit, tracking state after failure
- Issue: #6 · Size: M · Status: Backlog · after S-04
- Goal: SaveChanges owns its transaction; `SpoCommitUncertainException` and a discarded ObjectSpace (D-29).

### S-10 · Optimistic concurrency
- Issue: #6 · Size: M · Status: Backlog · after S-06
- Goal: integer version column on `SPOBaseObject`, incremented per update; conflict exception, nothing written (D-31).

### S-11 · Policy requirements fail closed at activation
- Issue: #6 · Size: S · Status: Done (PR #68, haiku-2)
- Goal: a model that requires tenant, row or field policy enforcement is rejected when it is activated, because no
  P1 provider can enforce it (D-33).
- Acceptance:
  - [ ] `SpoCapability` gains `TenantPolicy`, `RowPolicy`, `FieldPolicy`; `SqliteProvider` declares none of them
  - [ ] `CompiledModel.Build` with any of them in `required` throws `SpoCapabilityMissingException` naming the
        capability; a test per capability, plus one proving that a model without them still activates
  - [ ] No public way to declare a policy is added (D-33: attributes come later; `.claude/rules/public-api.md`)
  - [ ] `docs/architecture/query-security.md` rule 10: one sentence naming the test that proves it
  - [ ] Build 0 warnings; CI green
- Files: `src/Sferix.SPO.Abstractions/Providers/SpoCapability.cs`, `src/Sferix.SPO.Core/Model/**`,
  `tests/Sferix.SPO.Tests.Unit/**`, `docs/architecture/query-security.md`
- Not in scope: policy attributes, a policy engine, raw SQL rejection (no raw SQL in P1).
- Notes: D-33, D-39, D-42. Does not touch the files of S-02b; `src/Sferix.SPO.Core/Model/**` is yours while S-02b runs.

### S-12 · Generator diagnostics from resources
- Issue: #5 · Size: S · Status: Done (PR #64, sonnet-2)
- Goal: all generator diagnostic titles and messages come from `.resx` in en, de, hr, es (D-40, D-41), IDs unchanged.
- Acceptance:
  - [ ] `Diagnostics.cs` uses `LocalizableResourceString` for title and message of every `SPO0001`… descriptor
  - [ ] Diagnostic IDs, severities and message arguments unchanged; existing diagnostic tests pass unchanged
  - [ ] `de`, `hr`, `es` resources for every diagnostic; `scripts/validate_repository.py` passes
  - [ ] Test: with `CurrentUICulture` set to a culture without resources, the English text is reported; with `de`,
        the German text
  - [ ] Build 0 warnings (RS1xxx analyzer rules included); CI green
- Files: `src/Sferix.SPO.Generators/**`, `tests/Sferix.SPO.Tests.Generators/**`
- Not in scope: language packages (P2 or later).
- Notes: D-40, D-41. Done by the worker that owns the generator (sonnet-2), before S-02, so S-02's SPO0009 is
  added in resources directly. Not in parallel with another card that touches the generator.

### S-13 · Localization baseline: four languages and a CI check
- Issue: #5 · Size: S · Status: Done (PR #61, haiku-2)
- Goal: the resource pattern every later card copies exists in Core with en, de, hr, es, and CI rejects a missing
  language or key (D-40, D-41).
- Acceptance:
  - [ ] `src/Sferix.SPO.Core/Resources/CoreMessages.resx` plus `.de.resx`, `.hr.resx`, `.es.resx`, with an internal
        accessor class; the `SqlIdentifier` exception messages come from it; behaviour otherwise unchanged
  - [ ] `scripts/validate_repository.py`: for every neutral `src/**/*.resx`, the three culture files exist, have
        exactly the same keys, and no empty values; a clear error names the file and key
  - [ ] Test (Tests.Unit, new file under `Localization/`): the `SqlIdentifier` message is English under an unsupported
        culture and German under `de`; satellite assemblies for de, hr, es are produced by the Release build
  - [ ] `CONTRIBUTING.md`: three lines on the four-language rule and the check
  - [ ] Build 0 warnings; CI green on all three OSes
- Files: `src/Sferix.SPO.Core/Resources/**`, `src/Sferix.SPO.Core/Sql/SqlIdentifier.cs`,
  `tests/Sferix.SPO.Tests.Unit/Localization/**`, `scripts/validate_repository.py`, `CONTRIBUTING.md`
- Not in scope: generator diagnostics (S-12), Abstractions resources (S-02 adds its own), packaging.
- Notes: D-40, D-41. Runs in parallel with sonnet-2 (S-01b, S-02): do not touch `src/Sferix.SPO.Generators/**`,
  `src/Sferix.SPO.Abstractions/**`, `src/Sferix.SPO.Core/Model/**`, any `*.csproj` or `FrankenSPO.slnx`,
  `src/Sferix.SPO.Provider.SQLite/**`, `tests/Sferix.SPO.Tests.{Generators,SQLite}/**`.

### S-14 · Localization audit
- Issue: #5 · Size: S · Status: In Progress (haiku-2)
- Goal: a check that no user-visible string escaped the four-language rule (D-41).
- Acceptance:
  - [ ] Search `src/**` for string literals in `throw`, `Exception(` and `DiagnosticDescriptor` calls and in console or
        log output; each one is either a resource or listed with a reason (e.g. parameter names)
  - [ ] Read the de, hr, es texts against en: same meaning, same placeholders, consistent terms; fix wording
  - [ ] `scripts/validate_repository.py` also fails when a culture value's `{n}` placeholders differ from the neutral's
  - [ ] `EntityId`'s constructor message moves to `AbstractionsMessages` (four languages)
  - [ ] `docs/error-codes.md` lists `SPO1001`… with exception type and meaning (D-42)
  - [ ] The codes `SPO1005`/`SPO1006` (today constants in `SchemaBuilder`) move to `SpoModelException` and its XML
        docs list them; `SchemaBuilder` uses those constants
  - [ ] `<NeutralResourcesLanguage>`/`<NeutralLanguage>en</NeutralLanguage>` set once in `Directory.Build.props`, so
        English is served from the main assembly without probing for an `en` satellite
  - [ ] Findings that need code changes outside resources: listed in the PR for the main agent, not fixed
  - [ ] `python3 scripts/validate_repository.py` passes; build 0 warnings; CI green
- Files: `src/**/Resources/**`, `src/Sferix.SPO.Abstractions/Metadata/EntityId.cs`, `scripts/validate_repository.py`,
  `Directory.Build.props`, `docs/error-codes.md`, `src/Sferix.SPO.Core/Model/SpoModelException.cs`,
  `src/Sferix.SPO.Core/Schema/SchemaBuilder.cs`
- Notes: D-40, D-41. Each card localizes its own strings; this card only catches what was missed.

### S-07 · SQL injection regression suite
- Issue: #6 · Size: M · Status: Backlog · after S-05
- Goal: the cases listed in `docs/POC_ACCEPTANCE.md` (quotes, tautologies, comments, semicolons, sort names, LIKE).

### S-08 · `SpoExample.Module` and `SpoExample.Console`
- Issue: #7 · Size: M · Status: Backlog · after S-04
- Goal: compilable sample with run instructions; built in CI.

### S-09 · Capability manifest and docs for the PoC
- Issue: #1 · Size: S · Status: Backlog · last card of E1
- Goal: `features.json`, README, AGENTS.md and samples describe exactly the tested features.
