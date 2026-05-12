# Contributing

Thanks for helping improve DndHotkey.

## Branches

- `main` is for release-ready code.
- `dev` is the integration branch for active work.

## Commits

Use Conventional Commits:

```text
feat: add tray menu
fix: handle invalid config
docs: update install notes
test: cover hotkey parsing
chore: update tooling
```

Release automation uses commit subjects to choose the next semantic version:

- `feat:` creates a minor release.
- `fix:` and `perf:` create a patch release.
- `!` or `BREAKING CHANGE:` creates a major release.
- `[skip-release]` skips release calculation for that commit.

## Development Flow

Prefer red/green work for core behavior:

1. Add or update a focused test.
2. Run it and confirm it fails for the expected reason.
3. Implement the smallest useful fix.
4. Run tests and build.

Avoid new interfaces unless the class must be polymorphic or wraps an OS boundary that needs mocking.

## Verification

Run before opening a pull request:

```powershell
dotnet build src\DndHotkey.App\DndHotkey.App.csproj -c Release
dotnet test tests\DndHotkey.Core.Tests\DndHotkey.Core.Tests.csproj -c Release
```
