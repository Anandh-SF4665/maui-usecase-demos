# SPEC — Visitor Pass Management

> Source design: https://stitch.withgoogle.com/projects/7590268130354126103
>
> Companion documents:
> - [`HARNESS.md`](./HARNESS.md) — engineering harness, dependencies, conventions
> - [`TASKS.md`](./TASKS.md) — ordered implementation backlog
> - [`AGENTS.md`](./AGENTS.md) — AI agent workflow

---

## 1. Application Overview

### Purpose

Visitor Pass Management is a modern enterprise application that demonstrates
how organizations manage visitor registrations, generate visitor passes, track
visitor status, and monitor visitor activity within a facility.

This is a showcase use case for:

- .NET MAUI development
- Syncfusion MAUI control integration
- AI Harness-driven development workflows
- Stitch AI design generation

The application focuses on a complete visitor lifecycle while maintaining a
clean, visually appealing experience using local data only.

### Business Scenario

Offices, campuses, apartment communities, and facilities frequently need a
structured way to:

- Register visitors
- Track visit purpose
- Generate visitor passes
- Monitor visitor status
- Record arrivals and departures
- Review visitor activity trends

### Application Goals

**Functional**
- Register visitors
- Generate digital visitor passes
- Track visitor lifecycle
- Perform manual check-in / check-out
- View visitor records
- Analyze visitor activity

**Technical**
- Demonstrate Syncfusion controls
- Support both desktop and mobile runtimes from a **single** page per
  screen, using `OnIdiom` to express layout differences
- Use local sample data
- Follow AI Harness development practices
- Generate clean Stitch AI designs

---

## 2. Out of Scope

The following features are **intentionally excluded**:

- Authentication
- Role management
- QR code scanning
- Barcode scanning
- Security gate integration
- Email / SMS / push notifications
- Host approvals
- Visitor self-registration
- Identity verification / facial recognition
- Real-time backend services
- Multi-facility management

---

## 3. Application Navigation Structure

The application uses **one page per screen**, shared across desktop and
mobile. There are no separate `DesktopUI/` / `MobileUI/` folders; the same
`DashboardPage`, `DirectoryPage`, and `VisitorDetailsPage` are inflated on
every platform. Layout differences are expressed through MAUI's `OnIdiom`
markup extension and named style resources (see `HARNESS.md §7`).

A single `AppShell` (with `FlyoutBehavior=Disabled`) hosts the three screens
and surfaces them via a top navigation bar injected into every page.

| Surface | Form factor | Type | Screen / Action |
|---|---|---|---|
| Dashboard | All | `ContentPage` | Visitor Command Center |
| Directory | All | `ContentPage` | Visitor Directory |
| Details | All | `ContentPage` | Digital Pass & Visit Details |
| Register Visitor | Desktop / Tablet | `SfPopup` opened over Dashboard | Step 1 → Step 2 → Generate Pass |
| Register Visitor | Phone | `SfBottomSheet` opened over Dashboard | Step 1 → Step 2 → Generate Pass |

Navigation between Dashboard → Directory → Details uses Shell
`GoToAsync`. From Details, the user returns with `GoToAsync("..")`. The
**Register Visitor** surface is a single `RegisterVisitorPage` rendered
as `SfPopup` on desktop/tablet or as `SfBottomSheet` on phone — selected
by `OnIdiom` inside the page XAML.

---

## 4. Visitor Lifecycle

```
Register Visitor
        ↓
  Generate Pass
        ↓
    Expected
        ↓
   Checked In
        ↓
  Checked Out
```

Alternative paths:
- `Expected` → `Cancelled`
- `Expected` → `Overdue` (computed when current time > expected end time and status still Expected)

---

## 5. Data Model

### Visitor

| Field | Type | Description |
|---|---|---|
| `PassId` | string | Unique local identifier (format: `VP-yyyyMMdd-####`) |
| `VisitorName` | string | Visitor name |
| `Host` | string | Employee being visited (from predefined list) |
| `Purpose` | string | Visit purpose (from predefined list) |
| `ContactNumber` | string | Reference contact phone |
| `VehicleNumber` | string? | Optional vehicle info |
| `VisitDate` | DateTime | Scheduled date |
| `VisitTime` | TimeSpan | Scheduled time |
| `ExpectedDurationMinutes` | int | Planned duration |
| `Status` | `VisitorStatus` enum | Expected / Checked In / Checked Out / Cancelled / Overdue |
| `CheckInTime` | DateTime? | Arrival timestamp |
| `CheckOutTime` | DateTime? | Departure timestamp |
| `PassCode` | string | Barcode text (`VP-…`) |
| `AvatarColor` | string | Hex used by `SfAvatarView` background |
| `Notes` | string? | Optional |

### Predefined Hosts

- Sarah Johnson
- Michael Lee
- David Wilson
- Emma Parker
- John Anderson

### Visit Purposes

- Business Meeting
- Interview
- Package Delivery
- Maintenance Visit

### Status Enumeration

```csharp
public enum VisitorStatus { Expected, CheckedIn, CheckedOut, Cancelled, Overdue }
```

Helper methods live in `Models/VisitorStatusExtensions.cs`:

- `Display()` — "Expected", "Checked In", "Checked Out", "Cancelled", "Overdue"
- `Brush()` — status colour brush
- `Background()` — pale badge background brush

---

## 6. Screen 1 — Visitor Command Center (Dashboard)

### Purpose

Primary dashboard providing a complete overview of visitor activity and
analytics. This is the application's landing page.

### Sections

#### Section 1 — KPI Cards

Display:

- Expected Visitors
- Checked In
- Checked Out
- Overdue Visitors

Visual elements:

- Status colour indicator stripe at the bottom of each card
- Count display (large number)
- Status icon / badge

Layout (responsive via `OnIdiom`):

- **Desktop / Tablet:** 4 cards in one row — `ColumnDefinitions="*,*,*,*"`,
  `RowDefinitions="Auto"`
- **Phone:** 2 × 2 grid — `ColumnDefinitions="*,*"`, `RowDefinitions="Auto,Auto"`

Both layouts come from a single `DashboardPage` XAML whose KPI `Grid`
declares both sets of column / row definitions under `OnIdiom`. The four
cards are placed in cells using a uniform `Grid.Row` / `Grid.Column`
mapping; only the column widths and row counts change between idioms.

#### Section 2 — Visitor Status Distribution (Donut)

Displays count of each status:

- Expected
- Checked In
- Checked Out
- Cancelled
- Overdue

Visualization: `SfCircularChart` with `DoughnutSeries`.

Legend layout (responsive via `OnIdiom`):

- **Desktop / Tablet:** legend on the right of the donut (two-column: chart
  + legend)
- **Phone:** legend below the donut (stacked: chart then legend)

#### Section 3 — Visits by Purpose (Column)

Displays:

- Business Meetings
- Interviews
- Deliveries
- Maintenance Visits

Visualization: `SfCartesianChart` with `ColumnSeries`.

Panel layout (responsive via `OnIdiom`):

- **Desktop / Tablet:** donut and column charts sit in two `PanelCard`s
  side-by-side (`Grid ColumnDefinitions="*,*" ColumnSpacing="14"`)
- **Phone:** the same cards stack vertically
  (`Grid RowDefinitions="Auto,Auto" RowSpacing="14"`)

#### Section 4 — Today's Visitor Activity

Compact `SfDataGrid` showing today's visitors (5–8 rows max):

| Column | Source |
|---|---|
| Visitor Name | `VisitorName` |
| Host | `Host` |
| Purpose | `Purpose` |
| Visit Time | `VisitTime` formatted |
| Status | `Status` badge |

Selecting a row navigates to Screen 3 (Digital Pass).

#### Quick Action

Prominent "Register Visitor" pill button. Placement (responsive via
`OnIdiom`):

- **Desktop / Tablet:** in the page header (top-right)
- **Phone:** as a floating action button anchored bottom-right above
  the page content

Tapping it opens a single `RegisterVisitorPage` that renders as either
`SfPopup` (desktop / tablet) or `SfBottomSheet` (phone) — chosen inside
the page XAML via `OnIdiom`, **not** via two different XAML files.

### Syncfusion Controls Used

- `SfDataGrid`
- `SfBadgeView`
- `SfCartesianChart` (column)
- `SfCircularChart` (doughnut)

---

## 7. Register Visitor Surface (single page, two idioms)

### Purpose

Create a visitor registration without leaving the current screen. There is
**one** `RegisterVisitorPage`; it renders as `SfPopup` on desktop/tablet and
as `SfBottomSheet` on phone. The page itself is opened by the Dashboard
quick-action button via `Navigation.PushModalAsync`.

### Form Fields

**Visitor Information**

| Field | Input Type | Required |
|---|---|---|
| Visitor Name | `SfEntry` | Yes |
| Host | `SfComboBox` (predefined list) | Yes |
| Visit Purpose | `SfComboBox` (predefined list) | Yes |
| Contact Number | `SfEntry` (phone) | Yes |
| Vehicle Number | `SfEntry` | No |

**Schedule Information**

| Field | Input Type | Required |
|---|---|---|
| Visit Date | `SfDateTimePicker` (date) | Yes (default: today) |
| Visit Time | `SfDateTimePicker` (time) | Yes (default: next hour) |
| Expected Duration | `SfComboBox` (30 / 60 / 120 / 240 minutes) | Yes (default: 60) |

### Registration Progress — 3 steps

```
Visitor Details → Schedule → Generate Pass
```

`SfStepProgressBar` shows current step.

### Actions

- **Generate Pass** — primary action
  - Validates required fields
  - Creates a `Visitor` with status `Expected` and a unique `PassCode`
  - Adds to `IVisitorStore`
  - Dismisses the surface (`SfPopup.Dismiss` on desktop, `SfBottomSheet.Dismiss` on phone) and
    navigates to **Digital Pass & Visit Details** for the new visitor
- **Cancel** — dismisses the surface and discards the form

### Syncfusion Controls Used

- `DataForm` (alt) **or** manual `SfEntry` + `SfComboBox` grid
- `SfDateTimePicker`
- `SfStepProgressBar`
- `SfPopup` on desktop / tablet idiom, `SfBottomSheet` on phone idiom
  (selected by `OnIdiom` inside the same `RegisterVisitorPage`)

---

## 8. Screen 2 — Visitor Directory

### Purpose

Central visitor management screen. Provides full visibility into all visits.

### Search

Free-text search matching:

- Visitor Name
- Host
- Pass ID

Search updates the grid live (debounced 250 ms).

### Status Filters

Chip row: All / Expected / Checked In / Checked Out / Cancelled / Overdue

Single-select chip — selecting one filters the grid; "All" clears the filter.

### Visitor Grid (`SfDataGrid`)

Columns:

| Column | Source | Notes |
|---|---|---|
| Pass ID | `PassId` | Mono font, muted |
| Visitor | `VisitorName` + avatar | Custom row template with `SfAvatarView` |
| Host | `Host` | |
| Purpose | `Purpose` | |
| Date | `VisitDate` formatted | |
| Time | `VisitTime` formatted | |
| Status | `Status` | `SfBadgeView` in cell |

### Row Presentation

Each row displays:

- Visitor avatar (initials on coloured background)
- Visitor name + contact number (small grey text below)
- Status badge

### Row Action

Selecting a row navigates to Screen 3 (Digital Pass) for that visitor.

### Syncfusion Controls Used

- `SfDataGrid`
- `SfAvatarView`
- `SfBadgeView`

---

## 9. Screen 3 — Digital Pass & Visit Details

> **Hero screen** — visual centrepiece of the application.

### Section 1 — Digital Visitor Pass

Visitor identity (left column on desktop / tablet, top on phone):

- `SfAvatarView` (large, 96 dp)
- Visitor Name (large heading)
- Pass ID (mono, muted)

Visit information (right column on desktop / tablet, below identity on
phone):

- Host Name
- Visit Purpose
- Visit Date
- Visit Time
- Duration

Status badge displayed prominently at top-right of the card.

The two-column / stacked split is expressed in the single
`VisitorDetailsPage` XAML via `OnIdiom` on a `Grid ColumnDefinitions` —
identity in column 0, visit info in column 1, both spanning the full
width on phone.

### Section 2 — Visitor Pass Identifier

`SfBarcodeGenerator` rendering the visitor's `PassCode`.

Label: **"Visitor Pass Identifier"** (or **"Demonstration Pass Code"**).

> No scan-related wording. Barcode is display-only.

### Section 3 — Visit Journey

`SfStepProgressBar` showing timeline:

```
Registered → Expected → Checked In → Checked Out
```

Current stage highlighted automatically based on `Visitor.Status`. Steps after
the current stage are de-emphasised.

Cancelled shows a separate "Cancelled" terminator; Overdue shows an
"Overdue" indicator on the Expected step.

### Section 4 — Visit Summary

A small facts panel:

- Scheduled Time (visit date + time)
- Arrival Time (check-in, blank if not yet)
- Departure Time (check-out, blank if not yet)
- Visit Duration (actual = check-out − check-in)
- Current Status

### Section 5 — Actions

Buttons update according to current status:

| Status | Buttons Shown |
|---|---|
| Expected | Check In, Cancel Visit |
| Checked In | Check Out, Cancel Visit |
| Checked Out | (no actions, informational state) |
| Cancelled | (no actions) |
| Overdue | Check In, Cancel Visit |

All actions call into `IVisitorStore` and update the visitor; the page
re-evaluates buttons and the timeline automatically.

### Syncfusion Controls Used

- `SfAvatarView`
- `SfBarcodeGenerator`
- `SfBadgeView`
- `SfStepProgressBar`

---

## 10. Visit Analytics

Analytics are intentionally embedded into the Dashboard rather than a separate
page.

**Dashboard Analytics**

- Visitor Status Distribution → Doughnut Chart
- Visits by Purpose → Column Chart

Benefits:

- ✅ Reduces navigation
- ✅ Keeps the experience focused
- ✅ Prevents screen overload
- ✅ Creates a modern dashboard-first workflow

---

## 11. Syncfusion Control Mapping

| Control | Usage |
|---|---|
| `DataForm` | Visitor Registration (alternative to manual `SfEntry` grid) |
| `DataGrid` | Dashboard "Today's activity" + Directory |
| `BarcodeGenerator` | Digital Pass — visual demonstration only |
| `DateTimePicker` | Schedule selection in Registration surface |
| `AvatarView` | Visitor identity in Directory rows + Digital Pass |
| `StepProgressBar` | Registration progress + Visit Journey timeline |
| `Charts` (Cartesian / Circular) | Dashboard analytics (column + donut) |
| `BadgeView` | Status indicators across all screens |
| `SfPopup` | Register Visitor dialog on desktop / tablet idiom |
| MAUI-native modal `ContentPage` (slide-up) | Register Visitor sheet on phone idiom — Syncfusion does not ship `SfBottomSheet` for MAUI, so the phone idiom uses a MAUI-native `ContentPage` pushed via `Navigation.PushModalAsync` and styled to slide up |

---

## 12. Sample Data Boundary

| Item | Value |
|---|---|
| Facilities | 1 |
| Visitors | ~20 |
| Hosts | 5 |
| Visit Purposes | 4 |
| Status Types | 5 |
| Data Source | Local in-memory data |
| Pass Generation | Local |
| Barcode Usage | Visual demonstration only |

---

## 13. Design System

### Colour Palette

| Token | Light | Dark | Use |
|---|---|---|---|
| `Primary` | `#5146E5` | `#7C77FF` | Buttons, accents, active nav |
| `PrimaryDark` | `#3F38B8` | `#6653FF` | Primary hover/press |
| `PrimaryLight` | `#EEF2FF` | `#1E1B4B` | Tint backgrounds |
| `Surface` | `#FFFFFF` | `#161B2C` | Cards, panels |
| `SurfaceAlt` | `#F6F8FD` | `#0F1322` | Page background |
| `Border` | `#E5E9F2` | `#2A3045` | Card borders |
| `TextPrimary` | `#17243A` | `#F1F4FB` | Headings |
| `TextSecondary` | `#3A4658` | `#A4ADC2` | Body / labels |
| `TextMuted` | `#6B7589` | `#6E7894` | Captions, helper text |

### Status Colours

| Status | Brush | Background |
|---|---|---|
| Expected | `#3B82F6` | `#DBEAFE` |
| Checked In | `#10B981` | `#D1FAE5` |
| Checked Out | `#6B7280` | `#E5E7EB` |
| Cancelled | `#EF4444` | `#FEE2E2` |
| Overdue | `#F59E0B` | `#FEF3C7` |

### Typography

- Body: `OpenSansRegular`
- Headings: `OpenSansSemibold`
- Iconography: `MaterialAssets` (Syncfusion MAUI Material font) for in-XAML
  icons, PNG/SVG only for app logo

### Spacing Scale

- Card padding: 16 px (all idioms)
- Section spacing: 18 px desktop / tablet, 14 px phone (via `OnIdiom` on
  parent `VerticalStackLayout`)
- Page padding: 24 px desktop / tablet, 14 px phone (via named style
  `PagePaddingDesktop` / `PagePaddingMobile`)
- Border radius: 12 px (cards), 8 px (badges), 24 px (chart panels)

---

## 14. Form Factor Differences

The application uses **one page per screen**. The following layout
differences between desktop / tablet and phone idioms are produced
inside the same XAML through `OnIdiom`, named style resources, and
named font/spacing values.

| Concern | Desktop / Tablet | Phone | How |
|---|---|---|---|
| Page padding | 24 px | 14 px | Named style |
| KPI grid | 4 cols × 1 row | 2 cols × 2 rows | `OnIdiom` on `Grid ColumnDefinitions` |
| Donut + column panels | Side-by-side | Stacked | `OnIdiom` on parent `Grid` |
| Donut legend | Right of chart | Below chart | `OnIdiom` on legend container |
| Register Visitor | `SfPopup` centred | MAUI-native modal slide-up page | `OnIdiom` on the registration root |
| Register Visitor trigger | Header pill | Floating action button | `OnIdiom` visibility / position on the trigger |
| DataGrid columns | All columns visible | Pass ID hidden on narrow widths | `OnIdiom` on `DataGridTextColumn.IsVisible` |
| Status badge label | Full label | Compact (`Checked In` → `In`) | Converter + `OnIdiom` |
| Body font size | 13 px | 12 px | Named font-size style |
| Section spacing | 18 px | 14 px | `OnIdiom` on `VerticalStackLayout.Spacing` |

Benefits of this approach:

- ✅ Minimal navigation (no separate mobile/desktop shells)
- ✅ Modern enterprise appearance on desktop, modern app feel on mobile
- ✅ Strong desktop workflow with better use of screen space
- ✅ Mobile-friendly workflow with reduced page transitions
- ✅ Single source of truth — changes to a screen update both form
  factors simultaneously

---

## 16. Why This Design Works for AI Harness Development

- ✅ Clear business workflow
- ✅ Only 3 primary screens
- ✅ Minimal navigation complexity
- ✅ `SfPopup` / `SfBottomSheet` driven registration — a single page
  that picks the right surface per idiom
- ✅ Strong visual showcase through Digital Pass
- ✅ Demonstrates forms, grids, charts and workflow states
- ✅ Covers most commonly used Syncfusion controls
- ✅ Easy to implement within 1–2 days
- ✅ Generates high-quality Stitch AI designs
- ✅ Avoids feeling like a simple CRUD application