# maui-usecase-demos
This repository contains Syncfusion .NET MAUI use case demos that showcase practical scenarios and sample implementations to help developers understand how to apply Syncfusion MAUI controls in real-world applications.

## Applications

| Application | Folder | Description |
|---|---|---|
| AppointmentScheduler | `SfDataGrid/AppointmentScheduler` | Scheduling, bookings, and appointment details showcase. |
| BankingTransaction | `SfDataGrid/BankingTransaction` | Transaction monitoring use case. |
| E-Commerce Orders | `SfDataGrid/E-Commerce Orders` | Order management grid. |
| EmployeeDirectory | `SfDataGrid/EmployeeDirectory` | Employee listing + add/edit. |
| FinancialTracker | `SfDataGrid/FinancialTracker` | Personal finance dashboard. |
| HospitalManagement | `SfDataGrid/HospitalManagement` | Patient records + dashboard. |
| HotelBookingManagement | `SfDataGrid/HotelBookingManagement` | Hotel reservations with desktop/mobile split. |
| InventoryStockMonitor | `SfDataGrid/InventoryStockMonitor` | Stock monitoring. |
| LogisticTrackingApp | `SfDataGrid/LogisticTrackingApp` | Shipment tracking. |
| ProductCatalogViewerApp | `SfDataGrid/ProductCatalogViewerApp` | Product catalogue browser. |
| SalesOrderTracker | `SfDataGrid/SalesOrderTracker` | Sales order tracking. |
| StudentGradesDashboard | `SfDataGrid/StudentGradesDashboard` | Student grades dashboard. |
| TaskManagement | `SfDataGrid/TaskManagement` | Task list with charts. |
| WeatherForecast | `WeatherForecast` | Weather forecast sample. |
| **VisitorPass** | `SfDataGrid/VisitorPass` | Visitor pass management with KPI dashboard, directory, and digital pass details. **One page per screen**, shared across desktop and mobile via `OnIdiom` — no `DesktopUI/` / `MobileUI/` folder split. **Status:** MVP shipped (Phases 2–9 complete). |

## Governance Files

The Visitor Pass application is driven by four governance documents at the repo root. They are the contract between the requirements, the engineering harness, the task backlog, and the AI agents that implement it.

| File | Purpose |
|---|---|
| [`SPEC.md`](./SPEC.md) | What to build — business scenario, screens, data model, control mapping. |
| [`HARNESS.md`](./HARNESS.md) | Engineering harness — project skeleton, dependencies, build/run, conventions, adaptive-layout rules. |
| [`TASKS.md`](./TASKS.md) | Ordered backlog (`VP-NNN` tasks) with goal, files, done-when, dependencies. |
| [`AGENTS.md`](./AGENTS.md) | AI agent workflow — claim → implement → verify → commit, plus conventions. |

Read these four files in order before working on the Visitor Pass application.

### Architectural rule of thumb

> **One page per screen, two form factors.** Every screen has a single
> `ContentPage` shared by desktop and mobile runtimes. Layout
> differences — padding, grid columns, chart stacking, registration
> surface (`SfPopup` vs `SfBottomSheet`), status-badge compactness, and
> column visibility — are expressed through MAUI's `OnIdiom` markup
> extension and per-idiom named styles in `Resources/Styles/Styles.xaml`.
> There are **no** `Views/DesktopUI/` or `Views/MobileUI/` folders, no
> `App.IsMobileRuntime()` switch, and no `AppSidebar` / `MobileShell`
> (`SfTabView`) components. See `HARNESS.md §7` for the decision matrix.

### Visitor Pass feature set (MVP)

| Screen | XAML | ViewModel |
|---|---|---|
| Dashboard | `Views/DashboardPage.xaml` | `ViewModels/DashboardViewModel.cs` — KPI counts, donut + column chart series, today's-activity grid (8-row cap) |
| Directory | `Views/DirectoryPage.xaml` | `ViewModels/DirectoryViewModel.cs` — live search + status filter chips |
| Digital Pass | `Views/VisitorDetailsPage.xaml` | `ViewModels/VisitorDetailsViewModel.cs` — Check In / Check Out / Cancel commands, 4-step timeline |
| Registration | `Views/RegisterVisitorPage.xaml` + `Views/RegisterVisitorView.xaml` | `ViewModels/RegisterVisitorViewModel.cs` — 3-step wizard with validation |

Shared controls (`Controls/`) and value converters (`Converters/`) are reused
across every screen. Theme toggle (light / dark) is exposed via the
`TopNavBar` sun/moon pill and applies through `AppThemeBinding` on the
palette brushes — no per-page code.

### Build verification (VP-090)

| Target | Status | Notes |
|---|---|---|
| `net10.0-android` | ✅ Verified on E2B Linux sandbox | `dotnet build VisitorPass.csproj -f net10.0-android -c Debug` — 0 warnings, 0 errors |
| `net10.0-ios` | ⚠️ Requires macOS host | iOS workload not installable on Linux; per HARNESS §12 the agent doesn't run this target |
| `net10.0-maccatalyst` | ⚠️ Requires macOS host | Mac Catalyst workload not installable on Linux |
| `net10.0-windows10.0.19041.0` | ⚠️ Requires Windows host | Windows workload not installable on Linux |

Verified Android build on the E2B Linux sandbox using the HARNESS §12 install
sequence (workload install via `DOTNET_CLI_WORKLOAD_INSTALL_PATH=/var/dotnet-work`,
Android SDK at `/var/dotnet-work/android-sdk`). The other three targets are
expected to build cleanly because the project code is platform-agnostic MAUI
XAML + .NET 10 — no per-target `#if` branches, no platform-specific API
references.
