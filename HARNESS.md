# HARNESS — Visitor Pass Management

This document defines the **engineering harness** for the Visitor Pass Management
.NET MAUI application. It pins down the project skeleton, dependencies,
toolchain, build/run commands, conventions, and Syncfusion control surface so an
AI agent (or a human) can drive the project end-to-end without guessing.

> The accompanying documents are:
> - [`SPEC.md`](./SPEC.md) — what to build (product + functional + UI requirements)
> - [`TASKS.md`](./TASKS.md) — the ordered backlog of work
> - [`AGENTS.md`](./AGENTS.md) — how AI agents must work on this codebase

---

## 1. Project Identity

| Item | Value |
|---|---|
| Application Name | **VisitorPass** |
| .NET MAUI Project | `SfDataGrid/VisitorPass/VisitorPass/VisitorPass.csproj` |
| Solution | `SfDataGrid/VisitorPass/VisitorPass.slnx` |
| Root Namespace | `VisitorPass` |
| Target Frameworks | `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0` (Windows only on Windows hosts) |
| ApplicationId | `com.companyname.visitorpass` |
| Display Title | `Visitor Pass` |
| Display Version | `1.0` |
| Application Version | `1` |
| Theme | MaterialLight via `SyncfusionThemeResourceDictionary` |
| Data Source | Local in-memory seed data (no backend, no auth) |

This project follows the same desktop/mobile split as the in-repo
**HotelManagerSample** reference (see `SfDataGrid/HotelBookingManagement`). It
is the canonical pattern for a Syncfusion MAUI showcase with desktop and mobile
surfaces sharing the same store.

---

## 2. Solution & Project Layout

```
SfDataGrid/
└── VisitorPass/
    ├── VisitorPass.slnx                       # Solution entry
    └── VisitorPass/                           # MAUI project root
        ├── VisitorPass.csproj
        ├── App.xaml / App.xaml.cs             # Theme + DI bootstrap
        ├── MauiProgram.cs                     # Syncfusion + DI registration
        ├── AppShell.xaml / AppShell.xaml.cs   # Desktop Shell (sidebar nav)
        │
        ├── Models/                            # POCO records (Visitor, etc.)
        │   ├── Visitor.cs
        │   ├── VisitorStatus.cs               # Enum + helpers
        │   ├── VisitPurpose.cs                # Enum + helpers
        │   ├── Host.cs                        # Predefined host list
        │   ├── VisitorStats.cs                # KPI snapshot
        │   ├── StatusBreakdownPoint.cs        # Donut chart row
        │   ├── PurposeBreakdownPoint.cs       # Column chart row
        │   └── TimelineStep.cs                # Registration + visit journey
        │
        ├── Services/
        │   ├── IVisitorStore.cs               # Singleton in-memory store
        │   ├── VisitorStore.cs                # Seed data + mutation API
        │   ├── VisitorData.cs                 # Static seed of ~20 visitors
        │   ├── PassCodeGenerator.cs           # Local pass-id generator
        │   └── ThemeService.cs                # Light/dark toggle
        │
        ├── ViewModels/
        │   ├── BaseViewModel.cs               # INotifyPropertyChanged helper
        │   ├── DashboardViewModel.cs          # KPIs + chart data + today list
        │   ├── DirectoryViewModel.cs          # Search + status filter
        │   ├── VisitorDetailsViewModel.cs     # Selected visitor + actions
        │   └── RegisterVisitorViewModel.cs    # Multi-step registration form
        │
        ├── Views/
        │   ├── DashboardPage.xaml(.cs)        # Screen 1 — KPI + donut + column + today's grid + register CTA
        │   ├── DirectoryPage.xaml(.cs)        # Screen 2 — search + filter chips + SfDataGrid
        │   └── VisitorDetailsPage.xaml(.cs)   # Screen 3 — digital pass + barcode + timeline + actions
        │
        ├── Controls/
        │   ├── AppSidebar.xaml(.cs)           # Desktop sidebar with active tab
        │   ├── VisitorStatusBadge.xaml(.cs)   # Shared SfBadgeView wrapper
        │   ├── VisitorAvatar.xaml(.cs)        # Shared SfAvatarView wrapper
        │   └── EmptyState.xaml(.cs)           # Used when filters return nothing
        │
        ├── Popups/
        │   ├── RegisterVisitorPopup.xaml(.cs) # Desktop popup (SfPopup)
        │   └── RegisterVisitorBottomSheet.xaml(.cs) # Mobile bottom sheet
        │
        ├── Converters/
        │   ├── VisitorStatusToColorConverter.cs
        │   ├── VisitorStatusToBackgroundConverter.cs
        │   ├── StringNotEmptyToBoolConverter.cs
        │   ├── DateToShortLabelConverter.cs
        │   ├── InitialsConverter.cs
        │   └── VisitorStatusToBrushConverter.cs
        │
        ├── Resources/
        │   ├── AppIcon/                       # App icon (SVG)
        │   ├── Splash/                        # Splash screen (SVG)
        │   ├── Fonts/                         # OpenSansRegular/Semibold + Material icons
        │   ├── Images/                        # PNGs for nav + actions
        │   └── Styles/
        │       ├── Colors.xaml                # Theme colour palette
        │       ├── Styles.xaml                # Page/Label/Button base styles
        │       ├── VisitorPassCard.xaml       # KPI + panel card styles
        │       └── VisitorPassTheme.xaml      # Syncfusion custom palette overrides
        │
        ├── Platforms/
        │   ├── Android/
        │   ├── iOS/
        │   ├── MacCatalyst/
        │   └── Windows/
        │
        └── Properties/
```

> The folder structure mirrors `SfDataGrid/HotelBookingManagement/HotelManagerSample`
> so patterns transfer one-for-one.

---

## 3. NuGet Packages

Pin to the same Syncfusion version family used elsewhere in the repository.

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.Maui.Controls` | `$(MauiVersion)` | MAUI core |
| `Microsoft.Extensions.Logging.Debug` | `10.0.0` | Debug logging |
| `CommunityToolkit.Mvvm` | `8.4.0` | `ObservableObject`, `RelayCommand` helpers |
| `Syncfusion.Maui.Core` | `33.2.15` | SfBadgeView, SfAvatarView, base controls |
| `Syncfusion.Maui.Toolkit` | `33.2.15` | SfPopup, SfStepProgressBar, popup host |
| `Syncfusion.Maui.DataGrid` | `33.2.15` | SfDataGrid |
| `Syncfusion.Maui.Charts` | `33.2.15` | SfCartesianChart + SfCircularChart (donut) |
| `Syncfusion.Maui.Barcode` | `33.2.15` | SfBarcodeGenerator |
| `Syncfusion.Maui.Inputs` | `33.2.15` | DateTimePicker, SfEntry |
| `Syncfusion.Maui.Popup` | `33.2.15` | SfPopup for desktop register dialog |

> `Syncfusion.Maui.Toolkit` already includes the DataForm/StepProgressBar
> controls. Do **not** add a separate `Syncfusion.Maui.DataForm` package unless
> the code explicitly fails to resolve `DataForm`.

---

## 4. Toolchain

| Tool | Required | Notes |
|---|---|---|
| .NET SDK | `10.0.x` | Verify with `dotnet --list-sdks` |
| MAUI workload | `dotnet workload install maui` | Required for build/run |
| MAUI Windows workload | `dotnet workload install maui-windows` | Windows only |
| Git | any | For commits |
| Syncfusion license key | not required for Community/evaluation | Remove telemetry warnings if any |

Agents **MUST** verify the toolchain before writing code:

```bash
dotnet --list-sdks
dotnet workload list
```

If a workload is missing, report it and stop — never `dotnet workload install`
during a task (it can take minutes and the agent cannot tell the user apart from
the build output).

---

## 5. Build & Run Commands

The MAUI project lives at `SfDataGrid/VisitorPass/VisitorPass`. Use absolute
paths in the sandbox.

```bash
# Restore
dotnet restore "SfDataGrid/VisitorPass/VisitorPass.slnx"

# Build (Android target — fastest on Linux sandboxes)
dotnet build "SfDataGrid/VisitorPass/VisitorPass/VisitorPass.csproj" -f net10.0-android -c Debug

# Build (Windows target — requires Windows)
dotnet build "SfDataGrid/VisitorPass/VisitorPass/VisitorPass.csproj" -f net10.0-windows10.0.19041.0 -c Debug
```

> Sandbox environments usually cannot run a MAUI app interactively (no display).
> A green `dotnet build` is the strongest evidence the harness is healthy.

---

## 6. Architecture Rules

1. **Single source of truth.** All visitor records live in
   `Services/VisitorStore.cs` (singleton). Every ViewModel reads from it
   through `IVisitorStore`. The store raises a `VisitorsChanged` event after
   mutations; ViewModels subscribe and rebuild derived collections.
2. **No code-behind business logic.** Code-behind only wires XAML events to
   ViewModel commands. All state transitions happen in the ViewModel.
3. **MVVM only.** ViewModels inherit `ObservableObject` (CommunityToolkit.Mvvm)
   and use `[ObservableProperty]` / `[RelayCommand]` source generators.
4. **Desktop vs Mobile split.** `App.CreateWindow` chooses between
   `NavigationPage(new DesktopShell(...))` and `MobileShell(...)` exactly like
   `HotelManagerSample`. Do not share XAML between desktop and mobile pages —
   they live under `Views/DesktopUI/` and `Views/MobileUI/` respectively.
5. **Syncfusion hosted in DI.** Call `ConfigureSyncfusionCore()` and
   `ConfigureSyncfusionToolkit()` in `MauiProgram.cs` so every control theme
   resolves correctly.
6. **Theme resources.** All colours live in `Resources/Styles/Colors.xaml`. Pages
   reference them via `StaticResource`; code-behind must not hard-code hex
   colours.
7. **Status colours.** The five statuses map to fixed brushes:
   - `Expected` → `#3B82F6` (blue)
   - `Checked In` → `#10B981` (green)
   - `Checked Out` → `#6B7280` (slate)
   - `Cancelled` → `#EF4444` (red)
   - `Overdue` → `#F59E0B` (amber)
   These live as `<SolidColorBrush>` entries in `Colors.xaml` so XAML and
   converters share one source.
8. **Pass ID format.** `VP-{yyyyMMdd}-{0000}` (zero-padded 4-digit sequence per
   day) generated by `Services/PassCodeGenerator.cs`.

---

## 7. Desktop vs Mobile Surfacing

| Concern | Desktop | Mobile |
|---|---|---|
| Root page | `NavigationPage(new DesktopShell())` | `MobileShell` (single ContentPage with `SfTabView` bottom bar) |
| Navigation | Sidebar (`AppSidebar` control) + `Shell.FlyoutBehavior=Flyout` is **disabled**; sidebar is in-page | Bottom tab bar |
| Register dialog | `SfPopup` centered | Bottom sheet slide-up |
| KPI grid | 4 columns × 1 row | 2 columns × 2 rows |
| Charts | Two side-by-side panels | Stacked vertically |
| DataGrid columns | All columns visible | Compressed columns, hidden PassID on narrow widths |
| Font sizes | Body 13px | Body 12px |
| Padding | 24 px | 14 px |

`App.IsMobileRuntime()` is the gate (mirrors `HotelManagerSample.App`):

```csharp
if (DeviceInfo.Idiom == DeviceIdiom.Phone) return true;
if (DeviceInfo.Platform == DevicePlatform.iOS
 || DeviceInfo.Platform == DevicePlatform.Android) return true;
return false;
```

---

## 8. Syncfusion Control Surface

| Control | Where | Why |
|---|---|---|
| `SfDataGrid` | Dashboard "Today's activity" + Directory | Tabular display |
| `SfBadgeView` | Status badges across all screens | Status indicator |
| `SfAvatarView` | Directory rows + Digital Pass identity card | Visitor photo / initials |
| `SfCartesianChart` + `ColumnSeries` | Dashboard "Visits by purpose" | Bar visualisation |
| `SfCircularChart` + `DoughnutSeries` | Dashboard "Visitor status distribution" | Donut visualisation |
| `SfBarcodeGenerator` | Digital Pass screen | Demonstration pass code |
| `SfPopup` | Desktop "Register Visitor" | Modal dialog |
| `SfStepProgressBar` | Registration popup (3 steps) + Visit Journey timeline | Step / timeline UI |
| `DataForm` | Registration popup visitor details | Quick form binding (alternative to manual entry) |
| `DateTimePicker` / `SfDateTimePicker` | Registration popup schedule | Date + time selection |
| `SfTabView` | Mobile shell bottom navigation | Tab switching |

**Hard rule:** if a control is not in this table, do not introduce it.

---

## 9. Sample Data Contract

`Services/VisitorData.cs` must return **between 18 and 22** visitors with:

- All 5 statuses represented (≥1 each)
- All 4 visit purposes represented (≥1 each)
- Hosts drawn from the predefined list: Sarah Johnson, Michael Lee, David
  Wilson, Emma Parker, John Anderson
- Dates clustered around "today" (mostly today/yesterday; a couple in past week
  for context)
- Realistic phone numbers (`+1-555-xxxx`) and vehicle numbers (`KA-01-XX-1234`
  style) where populated

`Hosts` collection is exposed separately so the registration popup's host
`SfComboBox` can bind directly.

---

## 10. Evidence of Completion

A task is **not done** until:

1. `dotnet build` succeeds for the Android target (fastest check on Linux).
2. Every screen documented in `SPEC.md` exists under `Views/DesktopUI/` and
   `Views/MobileUI/`.
3. Every Syncfusion control listed in `SPEC.md §13` is referenced in XAML.
4. Status colours match the palette in `Resources/Styles/Colors.xaml`.
5. Pass ID generator produces a unique code on each `Register`.
6. Commit message references the task id (e.g. `VP-007: dashboard donut chart`).

The accompanying [`TASKS.md`](./TASKS.md) maps each rule above to a discrete
task that agents can pick up.

---

## 11. What This Harness Will *Not* Do

Out of scope (carried over from `SPEC.md` §4):

- Authentication, role management
- QR / barcode *scanning* (barcodes are display-only)
- Real-time backend services
- Push, email, SMS notifications
- Multi-facility support
- Visitor self-registration
- Host approval workflows

If a feature request falls in this list, refuse and link to the spec.