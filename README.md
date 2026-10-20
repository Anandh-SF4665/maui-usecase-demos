# .NET MAUI Syncfusion Use-Case Demos

This repository contains a collection of .NET MAUI sample apps that showcase
practical, end-to-end use-cases built around the
[Syncfusion .NET MAUI](https://www.syncfusion.com/maui-controls) control suite.

Each sample is self-contained, uses mock/local data, and is designed to help
developers understand how to apply Syncfusion MAUI controls to a realistic
scenario.

---

## Table of Contents

- [Prerequisites](#prerequisites)
- [How to Run](#how-to-run)
- [Samples](#samples)
  - [SfDataGrid showcase](#sfdatagrid-showcase)
  - [Weather dashboard](#weather-dashboard)
- [Syncfusion License](#syncfusion-license)
- [Project Structure](#project-structure)

---

## Prerequisites

- **.NET SDK** `10.0.x` (`dotnet --version` should report `10.0.100` or newer)
- **.NET MAUI workload**:
  ```bash
  dotnet workload install maui
  ```
- **Visual Studio 2022 (17.10+)** or **Visual Studio Code** with the C# Dev Kit
  extension (for full IDE experience including XAML hot reload)
- For mobile targets, the platform-specific SDKs:
  - **Android**: `dotnet workload install android` and Android SDK 21+
  - **iOS / Mac Catalyst**: macOS with Xcode
  - **Windows**: Windows 10 1809+ and Windows App SDK

## How to Run

Pick a sample from the [Samples](#samples) table, then from the sample's folder:

```bash
# Restore NuGet packages
dotnet restore

# Build for Android (cross-platform, works on Windows / macOS / Linux)
dotnet build -f net10.0-android

# Build for Windows desktop (Windows only)
dotnet build -f net10.0-windows10.0.19041.0

# Run on Android emulator / device
dotnet build -t:Run -f net10.0-android
```

> All samples are configured for `Single Project` MAUI and target multiple
> platforms. On Linux only the `net10.0-android` target is selectable; iOS and
> Mac Catalyst targets require macOS.

---

## Samples

### SfDataGrid showcase

All samples under `SfDataGrid/` focus on the
[Syncfusion SfDataGrid](https://www.syncfusion.com/maui-controls/maui-datagrid)
control and demonstrate different real-world scenarios.

| Folder | Scenario | Controls demonstrated |
| --- | --- | --- |
| `SfDataGrid/AppointmentScheduler` | Doctor appointment booking | `SfDataGrid`, `SfButton`, search, sorting, custom columns |
| `SfDataGrid/BankingTransaction` | Banking transactions log | `SfDataGrid`, PDF / Excel export (`DataGridExport`) |
| `SfDataGrid/E-Commerce Orders` | Order analytics & detail drill-down | `SfDataGrid`, `SfListView`, `SfTabView`, `SfPicker`, `SfSlider`, `SfChart`, PDF / Excel export |
| `SfDataGrid/EmployeeDirectory` | Searchable, editable employee list | `SfDataGrid`, multi-column sorting, filtering, edit form |
| `SfDataGrid/FinancialTracker` | Personal finance tracker | `SfDataGrid`, `SfListView`, `SfChart` |
| `SfDataGrid/HospitalManagement` | Patient records management | `SfDataGrid`, search, status filtering, detail navigation, theme handling |
| `SfDataGrid/HotelBookingManagement` | Hotel reservations & rooms | `SfDataGrid`, `SfListView`, `SfChart`, `SfCalendar`, `SfPopup`, `SfTabView`, `SfExpander`, `SfGauge`, `SfButton` |
| `SfDataGrid/InventoryStockMonitor` | Stock-level monitoring | `SfDataGrid`, `SfButton`, `SfComboBox` |
| `SfDataGrid/LogisticTrackingApp` | Shipment tracking | `SfDataGrid`, `SfProgressBar`, status template columns |
| `SfDataGrid/ProductCatalogViewerApp` | Catalog browse + detail | `SfDataGrid`, `SfPicker`, search, filter, sort, paging |
| `SfDataGrid/SalesOrderTracker` | Local-first sales order management | `SfDataGrid`, custom cell templates, SQLite persistence |
| `SfDataGrid/StudentGradesDashboard` | Student grade dashboard | `SfDataGrid`, PDF / Excel export, popup dialogs |
| `SfDataGrid/TaskManagement` | Kanban-style task board | `SfDataGrid`, `SfKanban`, `SfChart` |

### Weather dashboard

| Folder | Scenario | Controls demonstrated |
| --- | --- | --- |
| `WeatherForecast` | Multi-page weather forecast dashboard (desktop & mobile layouts) | `SfCard`, `SfChart`, `SfGauge`, `SfListView`, `SfTabView`, `SfProgressBar`, `SfPullToRefresh`, `SfInputs` |

---

## Syncfusion License

The Syncfusion MAUI controls used by these samples are commercial. For
development and small-team use, Syncfusion offers a free
[Community License](https://www.syncfusion.com/products/communitylicense).
Otherwise, a paid Syncfusion license is required.

You may also generate a free trial key from your Syncfusion account if you do
not have a license yet.

---

## Project Structure

```
.
├── SfDataGrid/                # Showcase samples built around SfDataGrid
│   ├── AppointmentScheduler/
│   ├── BankingTransaction/
│   ├── …
│   └── TaskManagement/
├── WeatherForecast/           # Multi-page weather dashboard sample
└── README.md
```

Each sub-folder is an independent .NET MAUI project with its own
`.csproj`/`.slnx` and can be opened, built, and run separately.

---

## Notes

- All sample data is bundled with the application or generated locally — no
  live network services are required at runtime.
- The Syncfusion package version is pinned to `32.2.9` across all samples for
  reproducible builds.
