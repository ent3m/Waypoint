; Unshipped analyzer release
; https://github.com/dotnet/roslyn/blob/main/docs/Adding%20Optional%20Parameters%20in%20Public%20API.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WYPT01   | DialogResultGenerator | Error | View does not inherit from Avalonia.StyledElement
WYPT02   | DialogResultGenerator | Error | Generic classes are not supported
WYPT03   | DialogResultGenerator | Error | Event is not accessible from the view
WYPT04   | DialogResultGenerator | Error | Event uses an unsupported delegate type