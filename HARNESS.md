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

Every screen has **one shared `ContentPage`** used by desktop and mobile runtimes.
The same XAML handles both form factors; layout differences (KPI grid columns,
chart stacking, registration dialog vs bottom sheet, padding, etc.) are expressed
through MAUI's `OnIdiom`, `OnPlatform`, `OnSizeAllocated`, and value converters
— **not** by duplicating pages into `DesktopUI/` and `MobileUI/` folders like the
`HotelManagerSample` reference does. This keeps a single source of truth, avoids
two-view-model drift, and ensures the desktop and mobile experiences stay in
lockstep as the design evolves.

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
        ├── AppShell.xaml / AppShell.xaml.cs   # Single Shell (FlyoutBehavior=Disabled, no sidebar)
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
        ├── Views/                             # One page per screen — desktop AND mobile share these
        │   ├── DashboardPage.xaml(.cs)        # Screen 1 — KPI + donut + column + today's grid + register CTA
        │   ├── DirectoryPage.xaml(.cs)        # Screen 2 — search + filter chips + SfDataGrid
        │   ├── VisitorDetailsPage.xaml(.cs)   # Screen 3 — digital pass + barcode + timeline + actions
        │   └── RegisterVisitorPage.xaml(.cs)  # Registration surface (renders as SfPopup on desktop, BottomSheet on mobile)
        │
        ├── Controls/                          # Reusable presentation primitives (no Shell/sidebar here)
        │   ├── VisitorStatusBadge.xaml(.cs)   # Shared SfBadgeView wrapper
        │   ├── VisitorAvatar.xaml(.cs)        # Shared SfAvatarView wrapper
        │   ├── TopNavBar.xaml(.cs)            # Title + "Register Visitor" pill button (uses OnIdiom for sizing)
        │   └── EmptyState.xaml(.cs)           # Used when filters return nothing
        │
        ├── Converters/
        │   ├── VisitorStatusToColorConverter.cs
        │   ├── VisitorStatusToBackgroundConverter.cs
        │   ├── StringNotEmptyToBoolConverter.cs
        │   ├── DateToShortLabelConverter.cs
        │   ├── InitialsConverter.cs
        │   ├── VisitorStatusToBrushConverter.cs
        │   └── IdiomToBoolConverter.cs        # Helps XAML branches read idioms (Phone → True)
        │
        ├── Resources/
        │   ├── AppIcon/                       # App icon (SVG)
        │   ├── Splash/                        # Splash screen (SVG)
        │   ├── Fonts/                         # OpenSansRegular/Semibold + Material icons
        │   ├── Images/                        # PNGs for nav + actions
        │   └── Styles/
        │       ├── Colors.xaml                # Theme colour palette + status brushes
        │       ├── Styles.xaml                # Page/Label/Button base styles + OnIdiom padding/sizing
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

> Layout decisions (KPI column counts, chart stacking order, registration
> surface style, padding) live **inside the page XAML** through `OnIdiom`
> branches, value converters, and named style resources — not in separate
> `DesktopUI/` / `MobileUI/` folders. There is exactly **one** `DashboardPage`,
> exactly **one** `DirectoryPage`, exactly **one** `VisitorDetailsPage`, and
> exactly **one** `RegisterVisitorPage`.

---

## 3. NuGet Packages

Pin to the same Syncfusion version family used elsewhere in the repository.

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.Maui.Controls` | `10.0.0` (literal — `$(MauiVersion)` resolves to 10.0.110 which is unavailable on NuGet) | MAUI core |
| `Microsoft.Extensions.Logging.Debug` | `10.0.0` | Debug logging |
| `CommunityToolkit.Mvvm` | `8.4.0` | `ObservableObject`, `RelayCommand` helpers |
| `Syncfusion.Maui.Core` | `33.2.15` | SfBadgeView, SfAvatarView, base controls + theme resource dictionary |
| `Syncfusion.Maui.Charts` | `33.2.15` | SfCartesianChart + SfCircularChart (donut) |
| `Syncfusion.Maui.DataGrid` | `33.2.15` | SfDataGrid |
| `Syncfusion.Maui.Barcode` | `33.2.15` | SfBarcodeGenerator |
| `Syncfusion.Maui.Inputs` | `33.2.15` | SfEntry, SfComboBox, SfDateTimePicker |
| `Syncfusion.Maui.Popup` | `33.2.15` | SfPopup |
| `Syncfusion.Maui.Picker` | `33.2.15` | SfPicker (used for date/time selection in registration) |
| `Syncfusion.Maui.Calendar` | `33.2.15` | SfCalendar (date selection) |
| `Syncfusion.Maui.DataForm` | `33.2.15` | DataForm (optional registration form) |
| `Syncfusion.Maui.Toolkit` | `1.0.11` (separate product — pinned to its own version line) | SfStepProgressBar, SfChips, SfEffectsView, SfTabView, SfNavigationDrawer, SfPullToRefresh, SfCarousel, SfTextInputLayout, SfShimmer, SfSegmentedControl |

> Two Syncfusion product lines ship under separate version numbers:
> - The **MAUI** controls (`Syncfusion.Maui.*`) version in lockstep at
>   `33.2.15` / `34.2.x` / `35.x.x`. They live in NuGet under their
>   per-control package names.
> - The **MAUI Toolkit** (`Syncfusion.Maui.Toolkit`) is a separate product
>   shipping under its own version line (`1.0.x`); it bundles the
>   supplementary toolkit controls (`SfStepProgressBar`, `SfChips`, etc.)
>   and hosts `ConfigureSyncfusionToolkit()`.
>
> **Note on `SfBottomSheet`:** Syncfusion does **not** ship an MAUI
> bottom-sheet control. The mobile-idiom registration container in
> `RegisterVisitorPage` is implemented with a MAUI-native
> `Page`-based slide-up layout (a `Grid` inside a `Navigation.PushModalAsync`
> page with a swipe-to-dismiss handle), **not** `SfBottomSheet`. The
> desktop idiom continues to use `SfPopup`. The choice is encapsulated
> inside the single `RegisterVisitorPage` XAML via `OnIdiom` so callers
> don't branch by `DeviceInfo.Idiom`.

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
4. **Single page, two form factors.** Every screen has one `ContentPage`
   shared by desktop and mobile. Layout differences are expressed in XAML
   via `OnIdiom`/`OnPlatform` and named style resources (see §7).
   Do **not** create `Views/DesktopUI/` or `Views/MobileUI/` folders.
   Do **not** fork pages by `DeviceInfo.Idiom` in code-behind — keep the
   page tree single-rooted and let MAUI's value providers do the branching.
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

## 7. Adaptive Layout with `OnIdiom`

Every screen is a single `ContentPage`. Layout differences between desktop
(tablet/desktop, Windows, Mac Catalyst) and mobile (Android phones, iOS
phones) come from `OnIdiom` markup extensions in XAML and a small set of
style helpers in `Resources/Styles/Styles.xaml`.

### Decision matrix

| Concern | Desktop / Tablet | Mobile (Phone) | How it's expressed |
|---|---|---|---|
| Page padding | 24 px | 14 px | `Styles.xaml` named styles (`PagePaddingDesktop`, `PagePaddingMobile`) consumed via `Style="{StaticResource …}"` |
| KPI grid | 4 columns × 1 row | 2 columns × 2 rows | `Grid ColumnDefinitions` set per-idiom with two `x:Double` resources (`KpiGridColumnWidthDesktop`, `KpiGridColumnWidthMobile`) |
| Charts panel | Two side-by-side panels | Stacked vertically | A `Grid` whose `ColumnDefinitions`/`RowDefinitions` come from `OnIdiom` |
| DataGrid columns | All columns visible | PassID hidden on narrow widths | `DataGrid.Columns` declaratively selects `IsVisible="{OnIdiom Phone=False, Default=True}"` |
| Register surface | Centered `SfPopup` | Slide-up bottom sheet (`SfBottomSheet`) | A single `RegisterVisitorPage` opened via `Navigation.PushModalAsync`; the page itself picks popup vs sheet style from `OnIdiom` (see §8) |
| Status badge width | Full label | Compact (`Checked In` → `In`) | `VisitorStatusBadge` applies a short-label converter when `OnIdiom Phone` |
| Font sizes | Body 13 px | Body 12 px | Named font-size styles in `Styles.xaml` |
| Top nav | `TopNavBar` shows full title + subtitle | Same `TopNavBar`, smaller height + shorter subtitle | Styles applied per-idiom |

### `OnIdiom` usage examples

```xml
<!-- Padding that shrinks on phones -->
<VerticalStackLayout Padding="{OnIdiom Desktop='24,0,24,28',
                                    Phone='14,8,14,14'}"
                     Spacing="14">

<!-- Column count that switches between 4-up and 2-up -->
<Grid ColumnDefinitions="{OnIdiom Desktop='*,*,*,*',
                                  Phone='*,*'}">

<!-- Column that hides on phones -->
<sfDataGrid:DataGridTextColumn HeaderText="Pass ID"
                               MappingName="PassId"
                               IsVisible="{OnIdiom Phone=False, Default=True}" />
```

### Runtime idiom reference in C#

When a code-behind value converter needs the idiom (rare — prefer XAML),
read it from the bound element:

```csharp
var isPhone = DeviceInfo.Current.Idiom == DeviceIdiom.Phone;
```

Do **not** store this in a static flag on `App` and switch root pages on
it. Use it inside the page-level view-model only when XAML cannot express
the variation (e.g. data shaping for a chart that needs different ranges).

### Forbidden patterns

| ❌ | Why |
|---|---|
| `Views/DesktopUI/...` and `Views/MobileUI/...` folders | Duplicates logic; the spec mandates a single page per screen |
| `App.IsMobileRuntime()` choosing root page | Same reason — produces two parallel UI trees |
| `SfTabView` bottom navigation for "mobile shell" | The bottom nav adds nothing here; Shell's tab bar already supports desktop windows; use a single Shell + Flyout disabled |
| `SfPopup` for desktop and a different `BottomSheet` for mobile as **two different files** | The visual differences are surface-level; one `RegisterVisitorPage` should render the right surface via `OnIdiom` |

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
| `SfPopup` | Register Visitor (desktop idiom) | Centered modal dialog |
| MAUI-native modal page | Register Visitor (phone idiom) | Slide-up registration container — Syncfusion does not ship a `SfBottomSheet` for MAUI, so the mobile idiom uses a `ContentPage` pushed modally from the Dashboard and styled to slide up over a translucent scrim. |
| `SfStepProgressBar` | Registration popup (3 steps) + Visit Journey timeline | Step / timeline UI |
| `SfChips` | Directory status filter row | Single-select status filter |
| `DataForm` (optional) | Registration popup visitor details | Quick form binding (alternative to manual SfEntry grid) |
| `SfDateTimePicker` / `SfCalendar` | Registration popup schedule | Date + time selection |

**Hard rule:** if a control is not in this table, do not introduce it. The
`SfTabView` from the HotelManagerSample reference is **not** used here —
navigation is handled by a single `AppShell` with `FlyoutBehavior=Disabled`,
and the three screens are reachable from a `TopNavBar` injected into every
page.

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
2. Every screen documented in `SPEC.md` exists as **one** page under `Views/`
   (no `DesktopUI/`/`MobileUI/` split, no `Popups/` split).
3. Layout differences are expressed via `OnIdiom` in XAML or named style
   resources — verified by toggling the MAUI designer idiom between
   `Phone` and `Desktop` (or by changing the device-size override).
4. Every Syncfusion control listed in `SPEC.md §13` is referenced in XAML.
5. Status colours match the palette in `Resources/Styles/Colors.xaml`.
6. Pass ID generator produces a unique code on each `Register`.
7. Commit message references the task id (e.g. `VP-007: dashboard donut chart`).

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

### Patterns explicitly excluded from this harness

The following patterns from the in-repo `HotelManagerSample` reference are
**intentionally not** carried over. They make sense for that sample's
three-tab domain (Dashboard / Bookings / Rooms) but produce double-the-work
and drift for Visitor Pass's three-screen linear flow.

| Excluded pattern | Reason |
|---|---|
| `Views/DesktopUI/` + `Views/MobileUI/` folders | We use `OnIdiom` branches inside a single page |
| `App.IsMobileRuntime()` choosing root page | Same reason — produces two parallel UI trees |
| `MobileShell` (`SfTabView` bottom bar) | Shell + `TopNavBar` + in-page navigation is sufficient |
| Two separate registration files (popup + phone-modal variants) | One `RegisterVisitorPage` renders as `SfPopup` (desktop) or a MAUI-native modal slide-up page (phone) via `OnIdiom` |
| Sidebar / `AppSidebar` control | The three screens are linear; a horizontal `TopNavBar` covers the navigation surface on both form factors |