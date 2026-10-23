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
>
> **Architectural rule:** every screen is **one page** shared by desktop
> and mobile. Layout differences live in `OnIdiom` branches inside that
> page's XAML — not in separate `Views/DesktopUI/` or `Views/MobileUI/`
> folders. See `HARNESS.md §7` for the decision matrix.

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
  - `SfDataGrid/VisitorPass/VisitorPass/Platforms/*` (copy from `EmployeeDirectory`)
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
  place, including per-idiom named styles.
- **Files:**
  - `Resources/Styles/Colors.xaml` — palette tokens + status brushes
  - `Resources/Styles/Styles.xaml` — base page/label/button styles + per-idiom
    padding (`PagePaddingDesktop`, `PagePaddingMobile`), per-idiom spacing
    (`SectionSpacingDesktop`, `SectionSpacingMobile`), per-idiom font sizes
  - `Resources/Styles/VisitorPassCard.xaml` — `KpiCard`, `PanelCard`,
    `SectionTitle` styles
  - `Resources/Styles/VisitorPassTheme.xaml` — Syncfusion colour overrides
- **Done when:** Static lookups for `Primary`, `Surface`, `TextPrimary`,
  `PagePaddingDesktop`, `PagePaddingMobile` and every status brush succeed;
  new pages pick up `KpiCard` and per-idiom padding styles automatically.
- **Parallel:** After VP-002.
- **Reference:** HARNESS §7.

---

## Phase 1 — Domain Model & Data

### VP-010 — Visitor model + status/purpose enums

- **Goal:** POCO `Visitor` model + `VisitorStatus` / `VisitPurpose` enums with
  display + colour helpers.
- **Files:**
  - `Models/Visitor.cs`
  - `Models/VisitorStatus.cs`
  - `Models/VisitPurpose.cs`
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
  correct colour; compact label on phones via `OnIdiom` + label converter.
- **Files:**
  - `Controls/VisitorStatusBadge.xaml(.cs)` — accepts `Status` property,
    uses `OnIdiom` on the inner `Label` to switch between full and
    compact text
- **Done when:** Dropping the control into a page renders the right brush;
  status changes are reflected automatically (`INotifyPropertyChanged`); on
  phones the label is shortened (`Checked In` → `In`).
- **Parallel:** Yes.
- **Reference:** SPEC §14.

### VP-021 — `VisitorAvatar` control

- **Goal:** Reusable `SfAvatarView` with auto-generated initials + colour from
  visitor name.
- **Files:** `Controls/VisitorAvatar.xaml(.cs)`
- **Done when:** Avatar shows correct initials + colour for any `Visitor`
  bound.
- **Parallel:** Yes.

### VP-022 — `TopNavBar` control

- **Goal:** Top navigation bar used by every screen; exposes a screen title,
  optional subtitle, and a "Register Visitor" trigger whose placement (header
  pill on desktop / tablet, floating action on phone) is chosen via `OnIdiom`.
- **Files:**
  - `Controls/TopNavBar.xaml(.cs)` — accepts `Title`, `Subtitle`,
    `ShowRegisterButton`; raises `RegisterClicked`
- **Done when:** The bar renders identically on both idioms except for the
  register-trigger placement; tapping the trigger raises the event.
- **Parallel:** Yes.
- **Reference:** HARNESS §7, SPEC §3.

### VP-023 — `EmptyState` control

- **Goal:** Placeholder shown when Directory filters return 0 rows.
- **Files:** `Controls/EmptyState.xaml(.cs)`
- **Done when:** Control renders in isolation with title + body text.
- **Parallel:** Yes.

### VP-024 — Converters (incl. idiom helpers)

- **Goal:** All XAML converters live in one folder, including idiom-aware
  helpers used by `OnIdiom` fallbacks in code-behind.
- **Files:**
  - `Converters/VisitorStatusToColorConverter.cs`
  - `Converters/VisitorStatusToBackgroundConverter.cs`
  - `Converters/StringNotEmptyToBoolConverter.cs`
  - `Converters/DateToShortLabelConverter.cs`
  - `Converters/InitialsConverter.cs`
  - `Converters/VisitorStatusToBrushConverter.cs`
  - `Converters/StatusToShortLabelConverter.cs` (returns compact label
    used by `VisitorStatusBadge` on phone)
  - `Converters/IdiomToBoolConverter.cs` (rare — used only when XAML
    cannot express an idiom branch)
- **Done when:** Converters compile and are reusable across pages.
- **Parallel:** Yes.

---

## Phase 3 — App Shell & Navigation

### VP-030 — Single `AppShell` + `TopNavBar` wiring

- **Goal:** One `AppShell` (Flyout disabled) hosts the three screens; every
  screen hosts a `TopNavBar` that drives navigation and exposes the
  Register Visitor trigger.
- **Files:**
  - `App.xaml.cs` — `CreateWindow` returns `new Window(new AppShell())`
    (no `IsMobileRuntime()` switch)
  - `AppShell.xaml` — declares the three routes:
    - `dashboard` → `DashboardPage`
    - `directory` → `DirectoryPage`
    - `details` → `VisitorDetailsPage`
  - `MauiProgram.cs` — register `DashboardPage`, `DirectoryPage`,
    `VisitorDetailsPage` (singletons) + their view-models
- **Done when:** `Shell.Current.GoToAsync("dashboard")` etc. works on every
  platform; the three pages inflate without errors.
- **Parallel:** After VP-022, VP-040/050/060 view-models.
- **Reference:** SPEC §3, HARNESS §2, §7.

---

## Phase 4 — Dashboard (Screen 1, single page)

### VP-040 — `DashboardViewModel` + KPI computation

- **Goal:** ViewModel exposes KPI counts, donut + column chart series, today's
  visitor list.
- **Files:** `ViewModels/DashboardViewModel.cs`
- **Done when:** Setting `DashboardViewModel` as BindingContext shows correct
  values for a sample dataset; subscribing to `IVisitorStore.VisitorsChanged`
  refreshes all derived collections.
- **Parallel:** After VP-011.

### VP-041 — Dashboard XAML — `TopNavBar` + KPI grid + register trigger

- **Goal:** Single `DashboardPage` with `TopNavBar` (title "Visitor Command
  Center", register button) and KPI cards. KPI grid uses `OnIdiom` to switch
  between 4-up (desktop/tablet) and 2×2 (phone).
- **Files:** `Views/DashboardPage.xaml(.cs)`
- **Done when:**
  - Toggling the MAUI designer idiom between `Phone` and `Desktop` switches
    the KPI layout without changing anything else.
  - The "Register Visitor" trigger raises a page event the code-behind
    routes to `Navigation.PushModalAsync(new NavigationPage(new RegisterVisitorPage()))`.
- **Parallel:** After VP-020, VP-022, VP-040.

### VP-042 — Dashboard XAML — donut + column chart panels

- **Goal:** Add a donut + column chart row to the same `DashboardPage`. The
  panels sit side-by-side on desktop/tablet and stack on phone (via
  `OnIdiom` on the parent `Grid`).
- **Files:** `Views/DashboardPage.xaml`
- **Done when:** Both charts render with the correct legend; toggling idiom
  reorders the panels; `DashboardViewModel` series properties drive them.
- **Parallel:** After VP-041.

### VP-043 — Dashboard XAML — today's activity grid + status badge

- **Goal:** Compact `SfDataGrid` of today's visitors using `VisitorStatusBadge`
  in the status column. The `Pass ID` column hides on phone via
  `IsVisible="{OnIdiom Phone=False, Default=True}"`.
- **Files:** `Views/DashboardPage.xaml`
- **Done when:** Tapping a row navigates to `VisitorDetailsPage` with the
  right visitor id; the badge updates when status changes.
- **Parallel:** After VP-020, VP-041.

---

## Phase 5 — Directory (Screen 2, single page)

### VP-050 — `DirectoryViewModel` (search + status filter)

- **Goal:** Live search + status chip filtering.
- **Files:** `ViewModels/DirectoryViewModel.cs`
- **Done when:** Setting `SearchText` filters in < 50 ms for the 20-row
  dataset; chip selection filters by status; `VisitorsChanged` resets the
  filtered list.
- **Parallel:** After VP-011.

### VP-051 — Directory XAML — `TopNavBar`, search box, status chips

- **Goal:** Single `DirectoryPage` with `TopNavBar` (title "Visitor Directory"),
  search entry, and status filter chips (`All / Expected / Checked In /
  Checked Out / Cancelled / Overdue`). The chip row's font size + spacing use
  per-idiom named styles; the search entry is full-width on phone.
- **Files:** `Views/DirectoryPage.xaml(.cs)`
- **Done when:** Search filters grid live (debounced 250 ms); chip
  selection filters; chips highlight the active selection.
- **Parallel:** After VP-050, VP-022.

### VP-052 — Directory XAML — `SfDataGrid` with row template

- **Goal:** Add `SfDataGrid` to the same `DirectoryPage` with custom
  `DataGridTemplateColumn` for the Visitor column (`VisitorAvatar` + name +
  contact) and Status column (`VisitorStatusBadge`).
- **Files:** `Views/DirectoryPage.xaml`
- **Done when:** Row tap navigates to `VisitorDetailsPage`; avatar initials
  and colours render; Pass ID column hides on phone.
- **Parallel:** After VP-020, VP-021, VP-051.

---

## Phase 6 — Digital Pass & Visit Details (Screen 3, single page)

### VP-060 — `VisitorDetailsViewModel` (check-in / check-out / cancel)

- **Goal:** Exposes selected visitor + commands for `CheckIn`, `CheckOut`,
  `CancelVisit`.
- **Files:** `ViewModels/VisitorDetailsViewModel.cs`
- **Done when:** Commands transition the visitor's status correctly and
  update derived state (timeline step index, action button visibility,
  summary fields).
- **Parallel:** After VP-011.

### VP-061 — Details XAML — identity card + status badge

- **Goal:** Add Section 1 (digital visitor pass) to the single
  `VisitorDetailsPage`: large `SfAvatarView`, name, Pass ID, host/purpose/
  date/time/duration, status badge. Identity column on left + info column on
  right (desktop/tablet); stacked on phone (via `OnIdiom` on `Grid
  ColumnDefinitions`).
- **Files:** `Views/VisitorDetailsPage.xaml(.cs)`
- **Done when:** All 8 visitor fields render; status badge colour matches
  `VisitorStatus`; toggling idiom switches layout between two-column and
  stacked.
- **Parallel:** After VP-060, VP-020, VP-021.

### VP-062 — Details XAML — barcode + label

- **Goal:** Add Section 2 (`SfBarcodeGenerator` rendering `PassCode`) +
  label "Visitor Pass Identifier" (or "Demonstration Pass Code").
- **Files:** `Views/VisitorDetailsPage.xaml`
- **Done when:** Barcode renders; label is correct; `PassCode` updates when
  visitor changes.
- **Parallel:** After VP-061.

### VP-063 — Details XAML — visit journey timeline

- **Goal:** Add Section 3 (`SfStepProgressBar` Registered → Expected →
  Checked In → Checked Out) with Cancelled / Overdue terminators.
- **Files:** `Views/VisitorDetailsPage.xaml`
- **Done when:** Current stage highlighted; subsequent stages de-emphasised;
  Cancelled and Overdue display correctly.
- **Parallel:** After VP-060, VP-061.

### VP-064 — Details XAML — summary panel + action buttons

- **Goal:** Add Section 4 (summary facts) + Section 5 (action buttons whose
  visibility follows status). Buttons use the same label / colour on both
  idioms; their row layout (horizontal vs stacked) is chosen via `OnIdiom`
  on the action container's `Grid`.
- **Files:** `Views/VisitorDetailsPage.xaml`
- **Done when:** Buttons appear / hide correctly; pressing them updates
  status and re-renders the page (badge, timeline, summary all refresh).
- **Parallel:** After VP-060, VP-063.

---

## Phase 7 — Registration Surface (single page, two idioms)

### VP-070 — `RegisterVisitorViewModel` (multi-step form)

- **Goal:** ViewModel for the registration form: validates required fields,
  drives the 3-step progress, and on submit creates the new `Visitor`.
- **Files:** `ViewModels/RegisterVisitorViewModel.cs`
- **Done when:** Step 1 → Step 2 requires visitor name + host + purpose +
  contact; Step 2 → submit requires date + time + duration; submit generates
  a unique `PassCode` and appends to `IVisitorStore`.
- **Parallel:** After VP-011.

### VP-071 — Register page XAML — `SfStepProgressBar` + Step 1 fields

- **Goal:** Single `RegisterVisitorPage` with `SfStepProgressBar` (3 steps)
  and Step 1 (visitor info: name, host `SfComboBox`, purpose `SfComboBox`,
  contact, vehicle number).
- **Files:** `Views/RegisterVisitorPage.xaml(.cs)`
- **Done when:** Step 1 renders; "Next" validates required fields and
  advances the step.
- **Parallel:** After VP-070.

### VP-072 — Register page XAML — Step 2 schedule fields + actions

- **Goal:** Add Step 2 (schedule: date `SfDateTimePicker`, time
  `SfDateTimePicker`, duration `SfComboBox`) plus the "Generate Pass" and
  "Cancel" action buttons.
- **Files:** `Views/RegisterVisitorPage.xaml`
- **Done when:** "Generate Pass" creates a visitor with status `Expected`;
  "Cancel" dismisses the surface without saving.
- **Parallel:** After VP-071.

### VP-073 — Register page — idiom-aware host (`SfPopup` vs MAUI modal slide-up)

- **Goal:** Same `RegisterVisitorPage` is wrapped by `SfPopup` on
  desktop/tablet and by a MAUI-native modal slide-up page on phone. The
  wrapping is chosen by an `OnIdiom` branch in the **container** that
  opens the page (the Dashboard's register trigger). The page itself is
  identical on both idioms; only the outer surface differs. (Syncfusion
  does not ship an MAUI bottom-sheet control, so the phone idiom is
  implemented as `Navigation.PushModalAsync` to a `ContentPage` styled
  with a top handle bar, rounded top corners and a translucent scrim
  backdrop.)
- **Files:**
  - `Views/DashboardPage.xaml` / code-behind — branch on idiom:
    - Desktop / Tablet → `SfPopup` whose `ContentTemplate` inflates
      `RegisterVisitorPage`
    - Phone → `Navigation.PushModalAsync(RegisterVisitorPage)` to a
      `ContentPage` styled as a slide-up sheet
- **Done when:** Toggling the designer idiom switches the registration
  trigger between popup and slide-up sheet without duplicating XAML or
  view-model.
- **Parallel:** After VP-072.

### VP-074 — Registration → Digital Pass navigation

- **Goal:** After "Generate Pass", the registration surface dismisses and
  the app navigates to `VisitorDetailsPage` for the new visitor.
- **Files:** `Views/RegisterVisitorPage.xaml.cs`,
  `MauiProgram.cs` (route registration if needed)
- **Done when:** Submitting a valid form lands the user on the new
  visitor's Digital Pass screen.
- **Parallel:** After VP-073.

---

## Phase 8 — Polish

### VP-080 — Theme toggle (light / dark)

- **Goal:** Light / dark mode toggle exposed via `ThemeService`; resources
  re-resolve through `AppThemeBinding`.
- **Files:** `Services/ThemeService.cs`, `Resources/Styles/Colors.xaml`
  (add dark-mode tokens)
- **Done when:** Toggling theme updates every page without rebuild.
- **Parallel:** After VP-003.

### VP-081 — Sample data refresh

- **Goal:** Final pass on `VisitorData.BuildVisitors()` so the dashboard's
  every chart category is populated, today's list has 5–8 rows, and status
  colours cover all five states.
- **Files:** `Services/VisitorData.cs`
- **Parallel:** After VP-011.

### VP-082 — Empty / edge states

- **Goal:** `EmptyState` wired into Directory when filters return 0 rows;
  friendly copy on Digital Pass when no visitor is selected (route guard
  redirects to Dashboard).
- **Files:** `Views/DirectoryPage.xaml.cs`, `Views/VisitorDetailsPage.xaml.cs`
- **Parallel:** After VP-023, VP-051.

---

## Phase 9 — Documentation & Final Verification

### VP-090 — Build verification on all targets

- **Goal:** `dotnet build` succeeds for `net10.0-android`, `net10.0-ios`,
  `net10.0-maccatalyst`, and `net10.0-windows10.0.19041.0` (Windows only).
- **Files:** none — verification only.
- **Done when:** Each target produces 0 errors and 0 new warnings introduced
  by VisitorPass code.
- **Reference:** HARNESS §5, §10.

### VP-091 — Update README with VisitorPass entry

- **Goal:** README points to `SPEC.md`, `HARNESS.md`, `TASKS.md`, and
  `AGENTS.md`, and notes the unified-page / `OnIdiom` pattern.
- **Files:** `README.md`
- **Done when:** README renders the application table + governance
  documentation links clearly.

---

## Phase 10 — UI Design Alignment

### VP-100 — Dashboard Visual Alignment

- **Goal:** Match DashboardPage exactly with the reference design.
- **Reference Images:**
  - [DashboardPageDesktop](/VisitorPassDesign/DashboardPageDesktop.png)
  - [DashbooardpageMobile](/VisitorPassDesign/DashboardPageMobile.png)
- **Files:**
  - `Views/DashboardPage.xaml`
  - `Resources/Styles/*`
  - `Controls/TopNavBar.xaml`
- **Requirements:**
  - Match spacing, padding, typography, colors, shadows, border radius.
  - Use only MAUI Material Assets icons.
  - Preserve ViewModel logic.
  - Preserve existing bindings.
- **Done when:**
  - Visual differences between implementation and design are minimal.
  - Layout matches design on Phone and Desktop.
  - All icons come from MAUI Material Assets.
- **Parallel:** No

### VP-101 — Directory Visual Alignment

- **Goal:** Match DashboardPage exactly with the reference design.
- **Reference Images:**
  - [VisitorDirectoryDesktop](/VisitorPassDesign/VisitorDirectoryDesktop.png)
  - [VisitorDetailsMobile](/VisitorPassDesign/VisitorDetailsMobile.png)

- **Requirements:**
  - Match search bar styling.
  - Match filter chips.
  - Match grid appearance.
  - Use Material icons only.

### VP-102 — Visitor Details Visual Alignment

- **Goal:** Match DashboardPage exactly with the reference design.
- **Reference Images:**
  - [VisitorDetailsMobile](/VisitorPassDesign/VisitorDetailsMobile.png)
  - [VisitorPassDesktop](/VisitorPassDesign/VisitorPassDesktop.png)
  - [VisitorPassMobile](/VisitorPassDesign/VisitorPassMobile.png)

- **Requirements:**
  - Match the UI exactly like the reference images for both desktop and mobile
  - Use SfBarcodeGenerator for the QRCode and Barcode.

### VP-103 — Registration Form Visual Alignment

- **Goal:** Match DashboardPage exactly with the reference design.
- **Reference Images:**
  - [RegistrationDesktop](/VisitorPassDesign/RegistrationDesktop.png)
  - [RegistrationMobile](/VisitorPassDesign/RegistrationMobile.png)

### VP-104 — Global Styling Review

- **Goal:** Ensure application-wide visual consistency.

- **Requirements:**
  - Popup alignment should be in center for Desktop and in bottom for mobile as per the reference image. 
  - Consistent corner radius.
  - Consistent shadows.
  - Consistent spacing scale.
  - Consistent typography scale.
  - Material asset icons only.
  - Remove temporary styling.