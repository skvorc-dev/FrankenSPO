# Error codes

Stable codes for FrankenSPO errors (D-42). A code never changes meaning and is never reused or renumbered. The message
text is localized (en, de, hr, es, D-40, D-41); the code is the same in every language.

- `SPO0001`–`SPO0999`: generator diagnostics, listed in the analyzer release notes.
- `SPO1001` and up: runtime exceptions. Each has a `Code` property and an `ErrorCode` constant.

## Runtime codes

| Code | Exception type | Visibility | Meaning |
|------|----------------|------------|---------|
| `SPO1001` | `SpoCapabilityMissingException` (Abstractions) | public | A model requires a capability the provider does not declare; activation fails (D-33). |
| `SPO1002` | `SpoModelException` (Core) | public | Two entities share an entity ID. |
| `SPO1003` | `SpoModelException` (Core) | public | An entity or property name is not a valid SQL identifier. |
| `SPO1004` | `SpoSchemaMismatchException` (SQLite provider) | internal | An existing database does not match the model; nothing was changed. |
| `SPO1005` | `SpoModelException` (Core) | public | Two entities map to the same table name. |
| `SPO1006` | `SpoModelException` (Core) | public | A property has no storage type in the schema. |
| `SPO1007` | `SpoValueException` (SQLite provider) | internal | A `DateTime` that is not UTC was passed for saving; nothing was written (D-44). |
| `SPO1008` | `SpoObjectSpaceException` (Core) | public | The entity type is not part of the model, or the model cannot create or key it. |
| `SPO1009` | `SpoObjectSpaceException` (Core) | public | A save updated no row (deleted or changed elsewhere); the save was rolled back. |
| `SPO1010` | `SpoObjectSpaceException` (Core) | public | An ObjectSpace call was made while another call on the same ObjectSpace was running. |
| `SPO1011` | `SpoObjectSpaceException` (Core) | public | The key of a tracked entity was changed after it was added or loaded; nothing was saved. |
| `SPO1012` | `SpoObjectSpaceException` (Core) | public | `Delete` was called with an entity this ObjectSpace does not track. |
| `SPO1013` | `SpoQueryNotSupportedException` (Core) | public | A query uses an operator outside the supported subset, or a `decimal`, `DateTimeOffset` or `byte[]` column in Where or OrderBy (D-32, D-46). |

Codes planned by later cards are added here by the card that introduces them.

Messages name the entity, table, column or capability only. They never contain SQL text or values.
