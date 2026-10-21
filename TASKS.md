# TASKS — Visitor Pass Management

> Companion documents:
> - [`SPEC.md`](./SPEC.md) — what to build
> - [`HARNESS.md`](./HARNESS.md) — engineering harness
> - [`AGENTS.md`](./AGENTS.md) — AI agent workflow
>
> Task ids follow `VP-NNN`. Each task lists:
> - **Goal** — single sentence outcome
> - **Files** — files to create / modify
> - **Done when** — verifiable acceptance condition
> - **Parallel** — yes / no + safety note
> - **Reference** — section in SPEC.md / HARNESS.md

---

## Phase 0 — Bootstrap

### VP-001 — Create MAUI project skeleton

- **Goal:** MAUI project builds end-to-end with no app logic.
- **Files:**
  - `SfDataGrid/VisitorPass/VisitorPass.slnx`
  - `SfDataGrid/VisitorPass/VisitorPass/VisitorPass.csproj`
  - `SfDataGrid/VisitorPass/VisitorPass/MauiProgram.cs`
  - `SfDataGrid/VisitorPass/VisitorPass/App.xaml(.cs)`
  - `SfDataGrid/VisitorPass/VisitorPass/AppShell.xaml(.cs)`
  - `SfDataGrid/VisitorPass/VisitorPass/MainPage.xaml(.cs)`
  - `SfDataGrid/VisitorPass/VisitorPass/Platforms/*` (copy from HotelManagerSample)
  - `SfDataGrid/VisitorPass/VisitorPass/Resources/{AppIcon,Splash,Fonts,Images,Styles}/*`
- **Done when:** `dotnet build … -f net10.0-android` produces 0 errors.
- **Parallel:** No — prerequisite for everything else.
- **Reference:** HARNESS §2, §3.

### VP-002 — Syncfusion + DI registration

- **Goal:** Syncfusion MAUI hosts registered; theme resources merged.
- **Files:**
  - `MauiProgram.cs` — add `ConfigureSyncfusionCore()` +
    `ConfigureSyncfusionToolkit()`; register pages + view-models in DI
  - `App.xaml` — merge `SyncfusionThemeResourceDictionary` + palette overrides
- **Done when:** `dotnet build` still clean; `Syncfusion.Maui.Themes` resources
  resolve at inflation time (verified by adding a temporary `SfBadgeView` to
  `MainPage` and rebuilding).
- **Parallel:** No — depends on VP-001.

### VP-003 — Theme & resource dictionaries

- **Goal:** Colours, status brushes, base styles, KPI card styles live in one
  place.
- **Files:**
  - `Resources/Styles/Colors.xaml` — palette tokens
  - `Resources/Styles/Styles.xaml` — base page/label/button styles
  - `Resources/Styles/VisitorPassCard.xaml` — `KpiCard`, `PanelCard`,
    `SectionTitle` styles
  - `Resources/Styles/VisitorPassTheme.xaml` — Syncfusion colour overrides
- **Done when:** Static lookups for `Primary`, `Surface`, `TextPrimary`, and
  every status brush succeed; new pages pick up `KpiCard` style automatically.
- **Parallel:** After VP-002.

---

## Phase 1 — Domain Model & Data

### VP-010 — Visitor model + status/purpose enums

- **Goal:** POCO `Visitor` model + `VisitorStatus` / `VisitPurpose` enums with
  display + colour helpers.
- **Files:**
  - `Models/Visitor.cs`
  - `Models/VisitorStatus.cs`
  - `Models/VisitorPurpose.cs`
  - `Models/Host.cs` (static list of 5 hosts)
  - `Models/VisitorStats.cs`
  - `Models/StatusBreakdownPoint.cs`
  - `Models/PurposeBreakdownPoint.cs`
  - `Models/TimelineStep.cs`
- **Done when:** All types compile, no XAML references yet.
- **Parallel:** Yes — independent of UI.
- **Reference:** SPEC §5.

### VP-011 — In-memory visitor store + seed data

- **Goal:** `IVisitorStore` + `VisitorStore` singleton with ~20 seeded visitors
  spanning every status/purpose.
- **Files:**
  - `Services/IVisitorStore.cs`
  - `Services/VisitorStore.cs` (mutation API + `VisitorsChanged` event)
  - `Services/VisitorData.cs` (seed method `BuildVisitors()`)
  - `Services/PassCodeGenerator.cs` (`VP-{date}-{####}` generator)
- **Done when:** Registering `VisitorStore` in DI returns an instance with ≥18
  visitors; every status and purpose appears at least once.
- **Parallel:** Yes (after VP-010).
- **Reference:** SPEC §5, §12.

### VP-012 — DI registration for store + services

- **Goal:** Singleton store + generator registered in `MauiProgram`.
- **Files:** `MauiProgram.cs`
- **Done when:** Pages can resolve `IVisitorStore` from DI; build still clean.
- **Parallel:** After VP-011.

---

## Phase 2 — Reusable Visual Components

### VP-020 — `VisitorStatusBadge` control

- **Goal:** Reusable `SfBadgeView` wrapper that renders a status with the
  correct colour.
- **Files:**
  - `Controls/VisitorStatusBadge.xaml(.cs)` — accepts `Status` property
- **Done when:** Dropping the control into a page renders the right brush;
  status changes are reflected automatically (INotifyPropertyChanged).
- **Parallel:** Yes.

### VP-021 — `VisitorAvatar` control

- **Goal:** Reusable `SfAvatarView` with auto-generated initials + colour from
  visitor name.
- **Files:** `Controls/VisitorAvatar.xaml(.cs)`
- **Done when:** Avatar shows correct initials + colour for any `Visitor`
  bound.
- **Parallel:** Yes.

### VP-022 — Empty-state + AppSidebar controls

- **Goal:** `EmptyState` placeholder used by Directory when filters return 0
  rows; `AppSidebar` desktop navigation.
- **Files:**
  - `Controls/EmptyState.xaml(.cs)`
  - `Controls/AppSidebar.xaml(.cs)` — accepts `ActiveTab`, raises `TabSelected`
- **Done when:** Both controls render in isolation; sidebar selection
  navigates to the requested page on desktop.
- **Parallel:** Yes.

### VP-023 — Converters

- **Goal:** All XAML converters live in one folder.
- **Files:**
  - `Converters/VisitorStatusToColorConverter.cs`
  - `Converters/VisitorStatusToBackgroundConverter.cs`
  - `Converters/StringNotEmptyToBoolConverter.cs`
  - `Converters/DateToShortLabelConverter.cs`
  - `Converters/InitialsConverter.cs`
  - `Converters/VisitorStatusToBrushConverter.cs`
- **Done when:** Converters compile and are reusable across pages.
- **Parallel:** Yes.

---

## Phase 3 — Desktop Shell & Navigation

### VP-030 — Desktop `AppShell` + sidebar wiring

- **Goal:** Desktop Shell with sidebar navigation across the 3 pages.
- **Files:**
  - `App.xaml.cs` — pick `DesktopShell` on non-mobile
  - `Views/DesktopUI/DesktopShell.xaml(.cs)` — hosts `AppSidebar` + current
    page area
  - Update `Controls/AppSidebar.xaml` — add Commands for navigation
- **Done when:** Sidebar tabs navigate between Dashboard / Directory /
  Details-placeholder; current tab is highlighted.
- **Parallel:** After VP-022.
- **Reference:** SPEC §3, HARNESS §7.

### VP-031 — Mobile shell with `SfTabView`

- **Goal:** Mobile bottom tab shell hosting the three mobile pages.
- **Files:**
  - `Views/MobileUI/MobileShell.xaml(.cs)` — `SfTabView` with TabBarPlacement=Bottom
  - `Views/MobileUI/MobileShellViewModel.cs`
- **Done when:** Tabs switch content; tab icons render; bottom bar visible.
- **Parallel:** Yes (after VP-002).

---

## Phase 4 — Dashboard (Screen 1)

### VP-040 — `DashboardViewModel` + KPI computation

- **Goal:** ViewModel exposes KPI counts, donut + column chart series, today's
  visitor list.
- **Files:** `ViewModels/DashboardViewModel.cs`
- **Done when:** Setting `DashboardViewModel` as BindingContext shows correct
  values for a sample dataset.
- **Parallel:** After VP-011.

### VP-041 — Desktop dashboard XAML — KPI cards + CTA

- **Goal:** Desktop dashboard with KPI cards + "Register Visitor" button.
- **Files:** `Views/DesktopUI/DashboardPage.xaml(.cs)`
- **Done when:** KPI cards render with correct counts; CTA opens register
  popup (wired in VP-060).
- **Parallel:** After VP-040, VP-020.

### VP-042 — Desktop dashboard XAML — donut + column charts

- **Goal:** Side-by-side donut + column chart panels.
- **Files:** `Views/DesktopUI/DashboardPage.xaml`
- **Done when:** Charts render with correct legend; both bound to
  `DashboardViewModel`.
- **Parallel:** After VP-041.

### VP-043 — Desktop dashboard XAML — today's activity grid + status badge

- **Goal:** Compact `SfDataGrid` of today's visitors using `VisitorStatusBadge`
  in status column.
- **Files:** `Views/DesktopUI/DashboardPage.xaml`
- **Done when:** Tapping a row navigates to `VisitorDetailsPage`.
- **Parallel:** After VP-020.

### VP-044 — Mobile dashboard XAML

- **Goal:** Mobile-friendly dashboard with 2×2 KPI grid, stacked charts, today's
  list, and floating action button.
- **Files:** `Views/MobileUI/DashboardPage.xaml(.cs)`,
  `Views/MobileUI/DashboardViewModel.cs`
- **Done when:** Page renders inside `MobileShell`; CTA opens bottom sheet.
- **Parallel:** Yes (after VP-040).

---

## Phase 5 — Directory (Screen 2)

### VP-050 — `DirectoryViewModel` (search + status filter)

- **Goal:** Live search + status chip filtering.
- **Files:** `ViewModels/DirectoryViewModel.cs`
- **Done when:** Setting `SearchText` filters in < 50 ms for 20-row dataset;
  chip selection filters by status.
- **Parallel:** After VP-011.

### VP-051 — Desktop directory page — toolbar, filter chips, SfDataGrid

- **Goal:** Search box, status chips, `SfDataGrid` with all columns + row
  template (avatar + name + status badge).
- **Files:** `Views/DesktopUI/DirectoryPage.xaml(.cs)`
- **Done when:** Search filters grid live; chip selection filters; row tap
  navigates to `VisitorDetailsPage`.
- **Parallel:** After VP-050, VP-020, VP-021.

### VP-052 — Mobile directory page

- **Goal:** Mobile directory with compressed grid columns.
- **Files:** `Views/MobileUI/DirectoryPage.xaml(.cs)`
- **Done when:** Renders inside mobile shell; same interactions as desktop.
- **Parallel:** Yes (after VP-050).

---

## Phase 6 — Digital Pass & Details (Screen 3)

### VP-060 — `VisitorDetailsViewModel` (with check-in / check-out actions)

- **Goal:** Exposes selected visitor + commands for `CheckIn`, `CheckOut`,
  `CancelVisit`.
- **Files:** `ViewModels/VisitorDetailsViewModel.cs`
- **Done when:** Commands transition the visitor's status correctly and update
  derived collections (timeline step index, action button visibility).
- **Parallel:** After VP-011.

### VP-061 — Desktop Digital Pass — identity card + status badge

- **Goal:** Large `SfAvatarView` + visitor details + status badge.
- **Files:** `Views/DesktopUI/VisitorDetailsPage.xaml(.cs)`
- **Done when:** Card renders all 8 visitor fields; status badge colour
  matches `VisitorStatus`.
- **Parallel:** After VP-060.

### VP-062 — Desktop Digital Pass — barcode + label

- **Goal:** `SfBarcodeGenerator` rendering `PassCode` with correct label.
- **Files:** `Views/DesktopUI/VisitorDetailsPage.xaml`
- **Done when:** Barcode renders; label reads "Visitor Pass Identifier".
- **Parallel:** After VP-061.

### VP-063 — Desktop Digital Pass — visit journey timeline

- **Goal:** `SfStepProgressBar` reflecting `Visitor.Status` (Registered →
  Expected → Checked In → Checked Out) with cancelled/overdone terminators.
- **Files:** `Views/DesktopUI/VisitorDetailsPage.xaml`
- **Done when:** Current stage highlighted; subsequent stages de-emphasised.
- **Parallel:** After VP-061.

### VP-064 — Desktop Digital Pass — visit summary + action buttons

- **Goal:** Summary facts panel + action buttons whose visibility follows
  status.
- **Files:** `Views/DesktopUI/VisitorDetailsPage.xaml`
- **Done when:** Buttons appear/hide correctly; pressing them updates status
  and re-renders the page.
- **Parallel:** After VP-060, VP-063.

### VP-065 — Mobile Digital Pass page

- **Goal:** Mobile-friendly layout with stacked sections.
- **Files:** `Views/MobileUI/VisitorDetailsPage.xaml(.cs)`
- **Done when:** Same content + behaviour as desktop; one column on small
  widths.
- **Parallel:** Yes (after VP-060).

---

## Phase 7 — Registration

### VP-070 — `RegisterVisitorViewModel` (multi-step + validation)

- **Goal:** Two-step form with step progress + validation + `GeneratePass`
  command.
- **Files:** `ViewModels/RegisterVisitorViewModel.cs`
- **Done when:** Step transitions work; validation blocks `GeneratePass` if
  required fields empty; command creates a `Visitor` with status `Expected`
  and a unique pass code.
- **Parallel:** After VP-011.

### VP-071 — Desktop `SfPopup` registration surface

- **Goal:** Centred popup with two-column form layout + step progress + CTA
  buttons.
- **Files:**
  - `Popups/RegisterVisitorPopup.xaml(.cs)`
- **Done when:** Popup opens/closes via dashboard CTA; form binds to
  `RegisterVisitorViewModel`; `GeneratePass` navigates to Digital Pass.
- **Parallel:** After VP-070.

### VP-072 — Mobile bottom sheet registration surface

- **Goal:** Slide-up bottom sheet with same form, responsive to small screens.
- **Files:**
  - `Popups/RegisterVisitorBottomSheet.xaml(.cs)`
- **Done when:** Sheet opens from mobile dashboard FAB; same `GeneratePass`
  behaviour.
- **Parallel:** Yes (after VP-070).

---

## Phase 8 — Polish & Verification

### VP-080 — Status-aware KPIs refresh

- **Goal:** Dashboard refreshes KPIs/charts after a registration or status
  change.
- **Files:** `DashboardViewModel.cs`, `DirectoryViewModel.cs` (subscribe to
  `VisitorsChanged`)
- **Done when:** Registering or checking-in/out a visitor updates dashboard
  counts and charts without a manual refresh.
- **Parallel:** Yes.

### VP-081 — Light / Dark theme toggle

- **Goal:** ThemeService toggle wired into AppShell toolbar (desktop) and
  mobile header overflow.
- **Files:** `Services/ThemeService.cs`, `AppShell.xaml(.cs)`,
  `MobileShell.xaml(.cs)`
- **Done when:** Toggle switches the active `UserAppTheme`; palette brushes
  swap via `AppThemeBinding`.
- **Parallel:** Yes.

### VP-082 — Build verification (Android target)

- **Goal:** Final build is green for Android.
- **Files:** none
- **Done when:** `dotnet build … -f net10.0-android` succeeds with 0 errors
  and 0 warnings related to project code (third-party warnings ignored).
- **Parallel:** No — final gate.
- **Reference:** HARNESS §5, §10.

### VP-083 — Visual smoke test screenshot list

- **Goal:** Document the exact screens + states a reviewer must see.
- **Files:** `docs/screens.md` (new)
- **Done when:** File lists every screen × device-form-factor × critical state
  (e.g. "Dashboard desktop — empty today's list", "Mobile registration — step
  2").
- **Parallel:** Yes.

---

## Phase 9 — Documentation

### VP-090 — VisitorPass README

- **Goal:** Standalone README for the new app describing build, run, controls
  demonstrated.
- **Files:** `SfDataGrid/VisitorPass/README.md`
- **Done when:** A reviewer can follow the README to build the project from a
  clean clone.
- **Parallel:** Yes.

### VP-091 — Top-level README link update

- **Goal:** Reference the new app + governance files from the repo root.
- **Files:** `README.md`
- **Done when:** README mentions `VisitorPass` and links to
  `SPEC.md` / `HARNESS.md` / `TASKS.md` / `AGENTS.md`.
- **Parallel:** Yes (after VP-090).

---

## Dependency Graph (high-level)

```
VP-001 → VP-002 → VP-003 ─┐
                            ↓
       VP-010 → VP-011 → VP-012 ─┐
                                  ↓
       VP-020, VP-021, VP-022, VP-023 (independent)
                                  ↓
       VP-040 → VP-041 → VP-042 → VP-043 (desktop dashboard)
                ↓
                VP-044 (mobile dashboard, parallel)

       VP-050 → VP-051 → VP-052 (directory)

       VP-060 → VP-061 → VP-062 → VP-063 → VP-064 (desktop details)
                ↓
                VP-065 (mobile details, parallel)

       VP-070 → VP-071 (desktop popup)
                ↓
                VP-072 (mobile bottom sheet, parallel)

       VP-080, VP-081, VP-082, VP-083 (polish)
       VP-090, VP-091 (docs)
```

Agents may claim parallel tasks as long as the dependency arrows above are
respected.

---

## How an Agent Picks a Task

1. Open `TASKS.md`; find the lowest-numbered task with status `pending`
   whose dependencies are `completed`.
2. Read every `Reference:` link — open the cited section in `SPEC.md` or
   `HARNESS.md` before writing code.
3. Follow `AGENTS.md` (worktree, commit cadence, evidence of completion).
4. On completion, mark the task `completed` in `TASKS.md` (agent updates the
   checkbox) and commit with message `VP-NNN: short summary`.