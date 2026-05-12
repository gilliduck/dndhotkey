# Codex Notes

- Use `DndHotkey.slnx`; do not create a `.sln` file.
- Keep implementation on `dev` and use Conventional Commits for checkpoints.
- Prefer test-first changes for core behavior. Run the focused test first, make it red, then make it green.
- Avoid interfaces for pure code. Use interfaces only for OS-facing seams such as hotkeys, Windows DND control, and shell/process launching.
- Build/test with `dotnet build src/DndHotkey.App/DndHotkey.App.csproj` and `dotnet test tests/DndHotkey.Core.Tests/DndHotkey.Core.Tests.csproj`.
- Build the MSI only after publishing the app to `artifacts/publish/win-x64`.
