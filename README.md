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
| **VisitorPass** | `SfDataGrid/VisitorPass` *(planned)* | Visitor pass management with KPI dashboard, directory, and digital pass details. **One page per screen**, shared across desktop and mobile via `OnIdiom` — no `DesktopUI/` / `MobileUI/` folder split. |

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
