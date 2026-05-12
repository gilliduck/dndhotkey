# DndHotkey

DndHotkey is a small Windows 11 tray app that toggles Do Not Disturb with a global hotkey. The default hotkey is `Ctrl+Alt+D`.

It shows its own topmost status overlay instead of relying on normal Windows notifications, so the confirmation still appears when Do Not Disturb is enabled.

## Install

Download the latest `.msi` from GitHub Releases once releases are available:

https://github.com/gilliduck/dndhotkey/releases

The MSI installs per user under `%LocalAppData%\Programs\DndHotkey` and registers the app to start with Windows. User configuration is stored separately and is preserved across updates.

## Configuration

The config file is created on first run:

`%AppData%\DndHotkey\config.json`

Default config:

```json
{
  "hotkey": "Ctrl+Alt+D",
  "showOverlay": true,
  "overlayMilliseconds": 1400,
  "startMinimized": true
}
```

Right-click the tray icon to open the config file or folder, reload config, toggle startup registration, toggle Do Not Disturb, or quit.

## Command Line

```powershell
DndHotkey.App.exe --toggle
DndHotkey.App.exe --enable
DndHotkey.App.exe --disable
DndHotkey.App.exe --open-config
DndHotkey.App.exe --open-config-folder
DndHotkey.App.exe --diagnose
```

## Development

Requirements:

- Windows 11
- .NET SDK 10

Common commands:

```powershell
dotnet restore DndHotkey.slnx
dotnet build src\DndHotkey.App\DndHotkey.App.csproj -c Release
dotnet test tests\DndHotkey.Core.Tests\DndHotkey.Core.Tests.csproj -c Release
```

Build a local MSI:

```powershell
dotnet publish src\DndHotkey.App\DndHotkey.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o artifacts\publish\win-x64
dotnet build src\DndHotkey.Installer\DndHotkey.Installer.wixproj -c Release
```

## Notes

DndHotkey targets Windows 11. The Windows DND adapter is intentionally isolated because public start/stop Focus APIs require limited access approval. If the adapter cannot toggle DND, the app opens Windows notification settings as a fallback.
