# Skyworks Windows screensaver

C#/.NET 8 WinForms screensaver. Every visible screen contains a **static image**, never an interactive webpage. Backgrounds and the `Assets\Links.json` webpage list are compiled into the application; webpage screenshots are read from local files. Original URLs and source images are preserved in the developer-managed `Assets` folder.

## Build and publish

Use Windows 10/11 x64, the .NET 8 or newer SDK, and Visual Studio 2022 with the **.NET desktop development** workload, or VS Code with the C# extension. Open `Skyworks.ScreenSaver.sln`.

From this folder in PowerShell:

```powershell
dotnet build .\Skyworks.ScreenSaver.sln -c Release
dotnet publish .\Skyworks.ScreenSaver\Skyworks.ScreenSaver.csproj -c Release -o .\publish
```

The publish target produces `publish\Skyworks.ScreenSaver.scr` alongside an equivalent `.exe` and `build-revision.txt`. Backgrounds and `Assets\Links.json` are embedded resources in the application, including the self-contained single-file executable; no external Assets folder or URL file is needed or read at runtime. The published application is self-contained: the target machine does not need .NET installed. Keep the **entire publish folder** together. Visual Studio's `FolderProfile` also publishes to this folder. Development builds produce a `.scr` alongside their executable and dependencies.

Packaged .NET satellite resources are filtered to English with `SatelliteResourceLanguages=en`. Neutral English resources remain embedded; normal globalization and native/runtime dependencies are unchanged. This also keeps the settings button's copied deployment English-only. The separately installed WebView2 Runtime manages its own language resources and is not filtered by this project property.

Install the **Microsoft Edge WebView2 Evergreen Runtime (x64)** separately if absent: <https://developer.microsoft.com/microsoft-edge/webview2/>. Edge being installed does not itself guarantee that the WebView2 Runtime is available. Assets still display if WebView2 initialization or networking fails; details go to the diagnostic log.

## Install and run

Open settings from either the built or published `.exe`/`.scr` and click **Set as Windows screensaver**. The button validates the complete deployment, copies **all** application files/dependencies to a new user-local version directory, and invokes the installed `.scr` with Windows' shell **install** verb. This is the same Windows integration used by the existing SkySpec screensaver: Windows selects the saver and opens native **Screen Saver Settings**, where you adjust the wait time and confirm with **Apply/OK**. No administrator rights or machine-wide installation are needed. The app does not set password-on-resume or change the wait time itself.

Stable installed files live under `%LOCALAPPDATA%\Skyworks.ScreenSaver\Installed\Deployments\<version-id>`. Backgrounds and URLs travel inside the compiled application; old external Assets folders and default URL files are not copied or used. Existing installed user data is left untouched. Old code versions are retained so running apps are not overwritten; after switching versions and closing old apps, unused deployment folders can be removed manually. Clicking from an already installed version reuses that deployment. Missing dependencies and copy/shell errors are shown explicitly.

Alternatively, move the entire publish folder to a stable local directory, right-click its `.scr` and choose **Install** (or **Show more options > Install**), then select it in Windows Screen Saver Settings. Do not copy just the `.scr` into System32. Registration occurs **only when you click the button**; builds and tests never configure the user's Windows screensaver.

```powershell
& .\publish\Skyworks.ScreenSaver.scr /c
& .\publish\Skyworks.ScreenSaver.scr /s
```

No arguments opens settings. `/s` runs across all attached monitors. `/c`, `/c:12345`, and `/c 12345` open settings, optionally owned by that window. `/p 12345` or `/p:12345` embeds a miniature preview into a valid Windows parent HWND. Switches are case-insensitive and also accept a leading `-`; HWNDs may be decimal or `0x` hexadecimal. Invalid arguments are diagnosed rather than guessed.

Settings includes **Full preview** (save and run `/s`) and **Diagnostics**. A key press, mouse click, or mouse movement exceeding 10 physical pixels exits full-screen mode after a one-second startup grace period. The initial mouse position and keys already held during startup do not immediately exit. Embedded previews ignore input, track the parent's client size, use local images only, and close when the preview parent disappears.

## URLs, images, and settings

`Assets\Links.json` is the developer-maintained webpage list, linked under **Assets** in Visual Studio's Solution Explorer and compiled as an embedded resource. Its format is a JSON string array, such as `["https://example.com", "example.org"]`. Edit this source file and rebuild/publish to change pages; there is no user-configurable URL path or external runtime override. Invalid entries get indexed diagnostics and are skipped. Credentials embedded in URLs are rejected. Duplicate normalized URLs are skipped. Scheme-less entries try HTTPS first, then HTTP if needed; explicit HTTPS URLs do not downgrade. The supplied file preserves the original URL strings and order.

The project's `Assets` folder contains developer-maintained `.jpg`, `.jpeg`, `.png`, `.bmp`, and `.gif` sources, including subdirectories. The original files stay in this folder and are linked under **Assets** in Visual Studio's Solution Explorer. They are compiled as embedded resources: add or replace source images and rebuild/publish to change backgrounds. Users cannot select an Assets directory or replace backgrounds by editing external files. The complete embedded image list is loaded and shuffled before any monitor displays it. Each image appears once per cycle, then the next cycle reshuffles; monitor targeting remains round-robin. All are displayed as still images, aspect-fitted against black. Image decoding releases its resource stream; retained screenshots have separate cache-protection leases. Broken resources are diagnosed and removed from the cycle. An original geometric fallback image is generated into this application's temp directory and used only when no readable embedded backgrounds remain.

The compiled version is painted inside the fitted image's bottom-right corner, with an inset and transparent background. Its baseline is 21 pixels at 96 DPI (50% larger than the previous 14 pixels), DPI-scaled and reduced only when a small preview requires it. A bounded 192-point sample underneath the label selects black or white by prioritizing the number of samples meeting 4.5:1 luminance contrast, rather than averaging away dark/light patches. An opposite-color 3-pixel outline at 96 DPI maintains readability on busy backgrounds; the outline scales down with small previews. Assets and stored screenshots are not modified.

When a monitor changes images, it crossfades the old image out and the new image in over 2 seconds. The first local background appears immediately at startup. The transition is independent of the shared image rotation and webpage refresh interval.

The version format is .NET's `major.minor.build.revision`. `Skyworks.ScreenSaver\build-revision.txt` stores the last allocated revision; each app Build (including a normal Publish's Build) atomically increments it once. The existing `1.0.0` prefix is unchanged. Restore, project evaluation, design-time compilation and `publish --no-build` do not increment it. Visual Studio's fast up-to-date shortcut is disabled so an explicit Build allocates a new revision even without source edits. A bounded interprocess file lock prevents lost increments; failed build attempts can consume a revision. Assembly, file, product and overlay versions use the allocated revision, and each output receives its own matching revision file. The counter fails explicitly at 65534 instead of wrapping. Keep the `.lock` coordination file in place.

Settings are user-scoped, atomically saved to:

```text
%LOCALAPPDATA%\Skyworks.ScreenSaver\settings.json
```

| Setting | Default | Valid range |
|---|---|---|
| Image and webpage refresh interval | 30 seconds | 5-86400 seconds |
| Per-page capture timeout | 45 seconds | 10-180 seconds |

Only the timing values are user-configurable. The shared interval controls both local image rotation and webpage recapture. Existing settings are migrated atomically: the previous image-rotation value becomes the shared interval, or the old webpage interval is used when no image-rotation value exists. Retired `AssetsPath` and `LinksPath` properties are removed without deleting external user data. Settings validates the embedded JSON rather than opening a user-selected file. Malformed settings are logged, defaults are used without overwriting the bad file, and settings displays a warning so they can be repaired.

## Capture and rotation behavior

1. Full-screen startup immediately fills each monitor from local assets, before creating a browser. Cached successful screenshots then join the local gallery.
2. A single background WebView2 visits valid URLs sequentially at startup and on the shared refresh cadence (30 seconds by default). Captures never overlap. An elapsed refresh tick during a long batch coalesces into one next refresh, not simultaneous batches. The compiled `Assets\Links.json` resource is deserialized on every batch.
3. Only successful **main-frame navigation with exactly HTTP 200 OK** is eligible. Other 2xx statuses, 403/404/500 responses, DNS/network failures, timeouts, and browser-generated error documents cannot create displayable screenshots. A 200 image, script, or iframe cannot make a failing main page eligible. Redirects are allowed only if the final main document returns 200. Font/image readiness and two animation frames are awaited for up to five seconds, plus a 750 ms settling interval, all within the per-page deadline. A page that navigates again before capture is rejected.
4. Each successful rendered viewport is saved as a unique PNG in `%TEMP%\Skyworks.ScreenSaver\Screenshots`, published by a same-directory rename only when complete. After the batch finishes, only its successful images replace the webpage rotation as one immutable snapshot. Presentation uses a Fisher-Yates shuffle: each eligible page appears once per cycle before the next cycle reshuffles. This randomizes **stored-image presentation**, not just request order. Failed URLs and their older screenshots do not enter the newly refreshed cycle.
5. Publishing a successful batch immediately loads its first shuffled stored image on one monitor; the independent image rotation timer shows the remaining shuffled images. Both updates share one round-robin monitor sequence. Each change affects **one monitor only**; every other monitor retains its current image. Display order is Windows' monitor enumeration order, not necessarily its displayed monitor numbering.

Assets remain available as fallback. Previously verified cached images can rotate while startup refresh is loading, and the prior successful cycle can continue while a refresh is in progress. A failed capture never substitutes an error-page image. Once a completed batch has no successful pages, no stale pages are added to that refreshed rotation: current monitor images are left intact, and subsequent timer changes use Assets. If the embedded URL list cannot be loaded or the batch is cancelled, the previous valid cycle remains available. A website's **soft-404** returning HTTP 200 cannot reliably be identified; no content-based error-page heuristics are applied.

Monitor windows use native screen bounds, including negative coordinates, and PerMonitorV2 DPI awareness. Captures use a common viewport based on the largest monitor width and height, capped at 3840x2160 (minimum 320x240); screenshots are aspect-fitted to each destination monitor. This avoids reserving a monitor during slow navigation while image rotation continues on the shared interval. Rendering stays on the STA UI thread; awaits leave the message loop responsive.

The browser lives in a borderless, off-desktop, no-activate tool window. It remains logically visible so WebView2 can render, but is never on a monitor or in the taskbar. It is not opacity-zero or minimized, which can prevent rendering. Popups, downloads, default script dialogs, permissions, and audio are suppressed. No insecure browser switches, certificate bypasses, or authentication workarounds are enabled.

Cancellation stops navigation, bounds initialization/script/capture waits, and disposes the browser after the refresh task unwinds. The capture task owns its stream until the native screenshot API completes, including on cancellation. Its unfinished file is never displayed. Startup loads at most 100 cached screenshots; a completed refresh retains one screenshot per successful URL so every eligible page gets a turn. Disk cleanup removes app-owned screenshots older than seven days or beyond the newest 200, excluding retained images. Read leases protect active, pending, and displayed screenshots against deletion by other running saver instances. Per-session browser profiles are isolated, removed on shutdown when possible, and old unlocked profiles are cleaned on later startups. No other application's temp files are touched.

Diagnostics are written to `%LOCALAPPDATA%\Skyworks.ScreenSaver\diagnostics.log` (rotated at approximately 2 MB). Failures are logged without modal dialogs interrupting full-screen mode. Screenshots and browser profiles may contain sensitive webpage content: use appropriate URLs and remove this application's temp data when no longer needed.

## Validation and limitations

```powershell
dotnet run --project .\Skyworks.ScreenSaver.Tests -c Release
dotnet run --project .\Skyworks.ScreenSaver.SmokeTests -c Release
```

The package-free core runner covers command-line parsing, JSON URL diagnostics and HTTPS-first fallback policy, settings validation/persistence and retired-path migration, shuffle permutations/no duplicates/refresh replacement, and independent monitor sequencing. The Windows smoke runner needs an interactive desktop and installed WebView2; it checks embedded background and URL resources, actual rendered screenshot pixels using loopback HTTP fixtures, strict main-document status gating, redirects, a reserved `.invalid` DNS failure, cancellation, offscreen/no-focus capture, static image resource lifetime, shuffled webpage/background cycles and embedded preview behavior. It does not make requests to the user's supplied websites or change Windows screensaver registration.

Capture-window activation always fails the smoke run. If another application changes foreground during the run, the separate unchanged-foreground observation is reported as **INCONCLUSIVE**, not mistaken for capture activation. Tests also cover published `.scr` cross-process preview and application deployment copying while ensuring external URL/image directories are unnecessary, without invoking Windows installation.

Webpages requiring login, bot challenges, denied certificates, unavailable network access, or a non-200 main response retain local images and generate diagnostics. Screenshots capture the viewport, not the entire scrollable page or offline HTML. Highly dynamic pages can change after the bounded settling period. The app is not a security lock; configure Windows' **On resume, display logon screen** separately. Real mixed-DPI/multi-monitor layouts and the Windows Screen Saver Settings integration still need device-level acceptance testing.
