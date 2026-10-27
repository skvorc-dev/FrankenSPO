; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SPO0001 | Sferix.SPO | Error | Entity class must be partial
SPO0002 | Sferix.SPO | Error | Entity ID missing or not a GUID
SPO0003 | Sferix.SPO | Error | Entity ID used twice
SPO0004 | Sferix.SPO | Error | Invalid [SpoField] number
SPO0005 | Sferix.SPO | Error | Unsupported property type
SPO0006 | Sferix.SPO | Error | Persistent property needs getter and setter
SPO0007 | Sferix.SPO | Error | Generic entity not supported
SPO0008 | Sferix.SPO | Error | Entity must be a class
