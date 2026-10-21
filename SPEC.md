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
- Support desktop and mobile layouts
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

### Desktop

| Surface | Type | Pages / Popups |
|---|---|---|
| Pages | In-page navigation via sidebar | 1. Visitor Command Center<br>2. Visitor Directory<br>3. Digital Pass & Visit Details |
| Popup | Centered modal | Register Visitor |

### Mobile

| Surface | Type | Pages / Popups |
|---|---|---|
| Pages | Bottom tab navigation | 1. Visitor Command Center<br>2. Visitor Directory<br>3. Digital Pass & Visit Details |
| Bottom Sheet | Slide-up sheet | Register Visitor |

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

Layout:

- **Desktop:** 4 cards in one row
- **Mobile:** 2 × 2 grid

#### Section 2 — Visitor Status Distribution (Donut)

Displays count of each status:

- Expected
- Checked In
- Checked Out
- Cancelled
- Overdue

Visualization: `SfCircularChart` with `DoughnutSeries`.

Legend with category + count + percentage on the right (desktop) or below
(mobile).

#### Section 3 — Visits by Purpose (Column)

Displays:

- Business Meetings
- Interviews
- Deliveries
- Maintenance Visits

Visualization: `SfCartesianChart` with `ColumnSeries`.

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

Prominent "Register Visitor" button (top-right on desktop, floating action on
mobile). Opens the registration surface:

- Desktop → `SfPopup` centred on screen
- Mobile → bottom sheet slide-up

### Syncfusion Controls Used

- `SfDataGrid`
- `SfBadgeView`
- `SfCartesianChart` (column)
- `SfCircularChart` (doughnut)

---

## 7. Register Visitor Popup / Bottom Sheet

### Purpose

Create a visitor registration without leaving the current screen.

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
  - Closes popup, navigates to **Digital Pass & Visit Details** for the new
    visitor
- **Cancel** — closes popup, discards the form

### Syncfusion Controls Used

- `DataForm` (alt) **or** manual `SfEntry` + `SfComboBox` grid
- `SfDateTimePicker`
- `SfStepProgressBar`
- `SfPopup` (desktop) / custom bottom sheet (mobile)

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

Visitor identity (left column):

- `SfAvatarView` (large, 96 dp)
- Visitor Name (large heading)
- Pass ID (mono, muted)

Visit information (right column):

- Host Name
- Visit Purpose
- Visit Date
- Visit Time
- Duration

Status badge displayed prominently at top-right of the card.

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
| `DataForm` | Visitor Registration |
| `DataGrid` | Dashboard & Directory |
| `BarcodeGenerator` | Digital Pass |
| `DateTimePicker` | Schedule Selection |
| `AvatarView` | Visitor Identity |
| `StepProgressBar` | Registration & Visit Timeline |
| `Charts` (Cartesian / Circular) | Dashboard Analytics |
| `BadgeView` | Status Indicators |
| `SfPopup` | Desktop Register Visitor dialog |

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

- Card padding: 16 px
- Section spacing: 18 px (desktop), 14 px (mobile)
- Page padding: 24 px (desktop), 14 px (mobile)
- Border radius: 12 px (cards), 8 px (badges), 24 px (chart panels)

---

## 14. Desktop Experience

- Pages: Visitor Command Center, Visitor Directory, Digital Pass & Visit
  Details
- Register Visitor popup: centred `SfPopup`
- Benefits: minimal navigation, modern enterprise appearance, strong desktop
  workflow, better use of screen space

## 15. Mobile Experience

- Pages: same three, presented inside `SfTabView` bottom bar
- Register Visitor bottom sheet
- Benefits: mobile-friendly workflow, reduced page transitions, modern app feel

---

## 16. Why This Design Works for AI Harness Development

- ✅ Clear business workflow
- ✅ Only 3 primary screens
- ✅ Minimal navigation complexity
- ✅ Popup / Bottom Sheet driven interactions
- ✅ Strong visual showcase through Digital Pass
- ✅ Demonstrates forms, grids, charts and workflow states
- ✅ Covers most commonly used Syncfusion controls
- ✅ Easy to implement within 1–2 days
- ✅ Generates high-quality Stitch AI designs
- ✅ Avoids feeling like a simple CRUD application