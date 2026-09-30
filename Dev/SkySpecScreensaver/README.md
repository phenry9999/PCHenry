# SkySpec Status Screen Saver

This native Windows screen saver displays Local, Dev, Stage, and Prod as four
status columns. A green check indicates an HTTP 200 response; any other status
or connection failure displays a red X.

## Build and test

Build the project in Visual Studio or run:

```powershell
dotnet build .\SkySpec.ScreenSaver.csproj
```

The build creates both `SkySpecStatus.exe` and `SkySpecStatus.scr` in the output
directory. Test each native screen saver mode with:

```powershell
.\bin\Debug\net8.0-windows\SkySpecStatus.scr /c
.\bin\Debug\net8.0-windows\SkySpecStatus.scr /s
```

`/c` opens the configuration window with its live preview. `/s` opens the
full-screen display. Embedded Windows Screen Saver Settings preview requests
exit without opening a window.

## Install

Publish a self-contained Windows build:

```powershell
dotnet publish .\SkySpec.ScreenSaver.csproj `
  -c Release -r win-x64 --self-contained true
```

Copy the publish directory to a stable location, then right-click the published
`SkySpecStatus.scr` file and select **Install**. Keep the other published files
beside the `.scr` file.

The Local URL uses HTTPS. Its ASP.NET Core development certificate must be
trusted on the computer or the health check will correctly report it as
unavailable.

## Settings

Default application settings are source controlled in
`SkySpecStatus.settings.json` and are copied beside the executable during build
and publish. The filename follows `{AssemblyName}.settings.json`. Edit this
project file to change the defaults distributed by the program creator. At
runtime, settings are loaded in this order:

1. `%LocalAppData%\SkySpec\StatusScreenSaver\SkySpecStatus.settings.json`.
2. `SkySpecStatus.settings.json` beside `SkySpecStatus.exe`.

Saving from the Settings window writes the per-user file and never modifies the
source-controlled application defaults.
