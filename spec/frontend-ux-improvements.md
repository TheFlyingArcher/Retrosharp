# Retrosharp Frontend UX Improvements

## Overview

This spec processes [raw/front-end-changes.md](../raw/front-end-changes.md) — the human's notes after running the prototype built per [frontend-prototype.md](./frontend-prototype.md) against six real imported seasons — into concrete root-cause analysis, decisions, and an implementation plan. Each item below traces the reported symptom to the actual line of code causing it (not just the described symptom), since several of the original items turned out to have a different root cause than the raw note assumed.

**This is a living document, same as its source.** The raw doc's own note ("This document will be evolving as changes are implemented") applies here too: as items ship and the human lives with them in the real app, new findings get appended (numbered rather than inserted, so existing cross-references stay valid) and each finding's **Status** line is kept current, rather than rewriting this spec from scratch each pass.

**In scope**: Players page pagination and UX, the "no final game on record" bold treatment, the Player Detail page's name display, the shared statistics table's column width, and the Material theme.

**Out of scope**: the Home page (raw doc explicitly defers this to a separate, still-evolving spec) and the Teams page ([teams.ts](../src/ui/Retrosharp.UI.Web/src/app/teams/teams.ts) is an empty stub — "not yet built" per the raw doc). Data-quality gaps that surface during this work (e.g. a player missing a surname) are noted where found but are [person.md](./person.md)/import territory, not a frontend fix.

## Findings

### 1. Players Page: Numbered Paginator

**Status: Complete** — see [Step E](#implementation-plan) below.

[players.ts](../src/ui/Retrosharp.UI.Web/src/app/players/players.ts) uses `MatPaginatorModule` as-is, which only ever renders first/previous/next/last controls — Angular Material has no built-in numbered-page-link UI, so "there needs to be a paginator with numbers" requires a custom control, not a configuration change. At 25/page, the "M" surname alone (2,751 players per the raw doc) is on the order of 110 pages, so the control needs to handle a large page count gracefully (a windowed range around the current page, not 110 rendered buttons).

**Decision**: build a small standalone numbered-paginator component (e.g. `PageNumberPaginator`) that takes `totalCount`/`pageIndex`/`pageSize` the same shape `mat-paginator` already takes, and renders First/Prev, a windowed run of page numbers (current ± 2, always including page 1 and the last page, with an ellipsis for the gap — the same pattern the existing A-Z letter nav already uses stylistically), a page-size select (reuse `pageSizeOptions`), and Next/Last. It emits the same `(page)` event shape as `MatPaginator` so `Players.onPage()` doesn't need to change. Because a 110-page list still makes "jump to page 60" tedious with windowed buttons alone, add a small "Go to page" number input next to the control.

**Implemented as**: [`PageNumberPaginator`](../src/ui/Retrosharp.UI.Web/src/app/shared/page-number-paginator/page-number-paginator.ts), exactly as decided above — windowed page numbers (with a single-page gap shown as the number itself rather than an ellipsis, since hiding one page behind "…" saves no room), a "Go to page" input, and the existing page-size select, wired into the Players page in place of `<mat-paginator>`. See Findings 8 and 9 below for two follow-on issues the human found after living with this in the real app.

### 2. Players Page: Next/Previous Scrolling the Page

**Status: Complete** — see [Step D](#implementation-plan) below. Confirmed fixed by the human: "the abrupt page jumping has disappeared."

The raw doc describes this as disorienting, but it is not a router or programmatic scroll — [players.html](../src/ui/Retrosharp.UI.Web/src/app/players/players.html) lines 27–41 swap the *entire* results block between a fixed ~40px `mat-spinner` and the full table+rows based on `loading()`:

```html
@if (loading()) {
  <div class="row"><div class="col players-spinner"><mat-spinner diameter="40"></mat-spinner></div></div>
} @else if (error()) { ... } @else if (players().length === 0) { ... } @else {
  <div class="row"><div class="col players-table-wrapper"><table ...>...</table></div></div>
}
```

`Players.onPage()` calls `load()`, which immediately sets `loading.set(true)` ([players.ts:72-76](../src/ui/Retrosharp.UI.Web/src/app/players/players.ts), [players.ts:142-144](../src/ui/Retrosharp.UI.Web/src/app/players/players.ts)). That collapses a full page of table rows down to a ~40px spinner for the duration of the request, which shrinks the document height under a scroll position that hasn't moved — so the browser's scroll position, still measured from the top of the document, now points partway up the (much shorter) page. Visually this reads exactly as "clicking next/previous scrolls the screen up," even though nothing called `scrollTo`. The same pattern exists on [player-detail.html](../src/ui/Retrosharp.UI.Web/src/app/player-detail/player-detail.html) and would hit any future page built the same way.

This is also the still-unbuilt "Page Loading" behavior from [frontend-prototype.md](./frontend-prototype.md#page-loading) — a central spinner over a dimmed, non-interactive background — which every page has instead reimplemented ad hoc as "replace the content with a spinner."

**Decision**: build the shared loading-overlay component described in frontend-prototype.md once (dim scrim + centered spinner, absolutely positioned over the existing content, `aria-busy`/pointer-events blocking interaction) and use it in place of the `@if (loading())` content-swap on the Players page (and Player Detail, and any future list/detail page). The previous page's rows stay laid out and visible under the scrim while the next page loads, so the document height — and therefore scroll position — doesn't move. This single component fixes the scroll-jump *and* finally implements the loading pattern the prototype spec already called for.

**Implemented as**: [`LoadingOverlay`](../src/ui/Retrosharp.UI.Web/src/app/shared/loading-overlay/loading-overlay.ts), projecting content via `<ng-content>` with an `initialLoad` signal on each page distinguishing "nothing to show yet" (plain spinner) from a reload (overlay on top of the still-visible previous page).

### 3. Players Page: "Active" Bold Text

**Status: Complete** — see [Step C](#implementation-plan) below.

[frontend-prototype.md](./frontend-prototype.md#resolved-determining-is-active--reframed-as-no-final-game-on-record) already reframed the original "bold if active" requirement once, keeping the bold treatment but re-labeling it "no final game on record" with a caveat tooltip ([players.ts:87-92](../src/ui/Retrosharp.UI.Web/src/app/players/players.ts), `.players-no-final-game` in [players.css](../src/ui/Retrosharp.UI.Web/src/app/players/players.css)). Having now seen it against six real seasons of data, the raw doc asks for the bold treatment to be removed outright — the tooltip caveat wasn't enough to stop the bold text from reading as an active/retired claim at a glance, which is exactly the false signal the data can't back up.

**Decision**: drop the bold treatment (and its tooltip) from the Name column entirely — remove `hasNoFinalGame()`'s use in [players.html](../src/ui/Retrosharp.UI.Web/src/app/players/players.html)'s `matColumnDef="name"` cell and the `.players-no-final-game` CSS rule. No replacement indicator is needed: the **Player Debut** and **Player Last** columns already show the raw dates (or a blank/"—" for a null last-game date) plainly, without asserting a status the data can't support. The deceased-marker (`†`) is unrelated and stays. The identical bold treatment on the Player Detail page's Player Last row ([player-detail.html:55-60](../src/ui/Retrosharp.UI.Web/src/app/player-detail/player-detail.html)) should be removed for the same reason and for consistency between the two pages.

**Implemented as decided**, with one refinement the human confirmed: Player Detail's "Player Last" row keeps a plain, unbolded "No final game on record" fallback string (rather than going blank like the Players table) since it's a neutral fact, not an active-status claim.

### 4. Player Detail Page: Name Only Shows First Name

**Status: Complete** — see [Step B](#implementation-plan) below. Spot-checked by the human against Manny Machado and Jim Abbott.

This is a real bug, not a data gap. [player-detail.ts](../src/ui/Retrosharp.UI.Web/src/app/player-detail/player-detail.ts)'s `displayName`:

```ts
readonly displayName = computed(() => {
  const player = this.player();
  return player ? (player.useName ?? player.fullName ?? player.retroSheetId) : '';
});
```

only ever falls back through `useName` → `fullName` → id — it never appends the surname the way the Players page's own `displayName()` correctly does ([players.ts:78-85](../src/ui/Retrosharp.UI.Web/src/app/players/players.ts): `` `${player.useName} ${player.surname}` ``). `PlayerDetail` already carries `surname` ([player-detail.model.ts:6](../src/ui/Retrosharp.UI.Web/src/model/player-detail.model.ts)) — the field exists and is fetched, it's just unused here. So "Manny Machado" renders as "Manny."

**Decision**: fix `displayName` to match the Players page's logic exactly: `"[useName] [surname]"` when both exist, falling back to whichever of `useName`/`surname`/`fullName`/`retroSheetId` is available. Then add the requested "full legal name" line: render `player.fullName` in smaller, muted text directly under the `<h1>` display name, **only when it differs** from the constructed "useName surname" string (e.g. show "Manuel Machado" under "Manny Machado", but don't show a redundant second line for a player whose legal and use names are already identical). `fullName` is already part of `PlayerDetail` and already fetched — no API change needed.

**Data-quality note, not a frontend fix**: the raw doc separately flags "if the player's surname is missing, that's an issue that needs to be addressed immediately." That's correct, but it isn't fixable from this component — a null `surname` is an import/biofile gap, not a rendering choice. The fallback chain above ensures the UI never renders blank, but a systematic null-surname case should be raised against the Person import ([person.md](./person.md)) separately from this frontend work.

### 5. Player Detail Page: Batting Table Side-Scroll

**Status: Deferred** — blocked on data, see [Step G](#implementation-plan) below. Still open per the raw doc's latest revision ("The batting statistics table currently has side scrolling...").

[statistics-table.css](../src/ui/Retrosharp.UI.Web/src/app/shared/statistics-table/statistics-table.css) already wraps the table in `overflow-x: auto`, which is exactly what's producing the reported side-scroll — it's a symptom of the table being wider than its container, not a separate bug. `BATTING_COLUMNS` ([batting-columns.ts](../src/ui/Retrosharp.UI.Web/src/app/shared/statistics-table/batting-columns.ts)) has 24 columns, almost all single-to-four-character abbreviations (G, GS, AB, R, H, 2B, 3B, HR, RBI, SB, CS, K, BB, IBB, AVG, OBP, SLG, OPS, HBP, SH, SF, TB, GIDP). Two things inflate that far past its actual content width:

- Every header/cell gets `padding: 0 0.75rem` (12px each side) regardless of content — 24px of pure padding per column, ~576px total across 24 columns, dwarfing the 1-4 characters of actual content in most of them.
- Every header additionally has `mat-sort-header` ([statistics-table.html:8](../src/ui/Retrosharp.UI.Web/src/app/shared/statistics-table/statistics-table.html)), and Material's sort header reserves fixed layout space for its arrow indicator on **every** column, not just the currently-sorted one — so a "G" header column reserves roughly as much width as a real sort arrow plus its own padding needs, again wildly out of proportion to the one character it displays.

**Decision**: tighten `.statistics-table th/td` padding (e.g. `0 0.375rem`, roughly half the current value) and override the sort-header arrow's reserved spacing/margin so unsorted columns don't pay for an indicator they aren't showing (Material exposes this via the `sort` component tokens/CSS custom properties rather than requiring `::ng-deep`). Keep the `overflow-x: auto` wrapper as a narrow-viewport safety net — the goal is that a 24-column career table fits inside a normal desktop content width (~1000-1100px) without scrolling, not that horizontal scroll is impossible on a phone. Verify against a real player with a long career (20+ seasons, several team changes) after the CSS change, not just against a short mock career, since real column content width (e.g. "Team(s)" holding two team abbreviations in a trade year) is part of what the padding fix needs to accommodate.

### 6. Theme Colors Not Appearing

**Status: Complete** (the original finding — M3 regeneration + deliberate color application), **superseded in part by the human's own revised approach** — see the addendum below. **Dark mode: on hold**, no base color chosen yet.

[material-theme.scss](../src/ui/Retrosharp.UI.Web/src/material-theme.scss) calls `mat.theme()` with `primary: mat.$azure-palette, tertiary: mat.$blue-palette` — these are two of Angular Material's own **predefined** M3 system palettes (see the exported list in [`@angular/material/_index.scss`](../src/ui/Retrosharp.UI.Web/node_modules/@angular/material/_index.scss)), generated by Google's Material Theme Builder from its own reference hues. "Azure" here is Material's own palette name, not a hook for the seven brand hex values in the raw doc (`#D1E2FD` … `#01102D`) — nothing in the current setup ever reads those hex values at all, which is exactly why the human isn't seeing them.

There's a second, independent reason the app reads as mostly gray even where a palette is wired up: Material 3 deliberately keeps most chrome — the toolbar, page background, and table surfaces — on neutral/surface color tokens rather than tinting them with the primary color, unlike Material 2's more liberal use of primary-colored app bars. `mat-toolbar` in [nav-menu.html](../src/ui/Retrosharp.UI.Web/src/app/core/nav-menu/nav-menu.html) and the table headers render in system neutral tones by design; only components explicitly given `color="primary"` (today, just the selected A-Z letter button on the Players page) pick up the brand color at all. So even a perfectly-generated custom palette will still look mostly neutral until primary color is deliberately applied to more of the chrome the human actually wants colored.

**Decision**:
1. Regenerate the theme from the actual brand hex using Angular Material's M3 theme schematic (`ng generate @angular/material:m3-theme`, seeded with `#196DE6` as the primary color), which produces a full 0-100 tonal palette derived from that exact hue via the M3 HCT color algorithm — the seven listed swatches read as roughly evenly-spaced tones along a single hue ramp (light `#D1E2FD` down to near-black `#01102D`), which is precisely the shape an M3 tonal palette takes, so this is very likely the exact ramp the human already had in mind rather than a coincidence. Swap the generated palette map(s) in for `mat.$azure-palette`/`mat.$blue-palette` in `material-theme.scss`.
2. Deliberately extend `color="primary"` (or the equivalent CSS custom property override where a component has no `color` input) to the chrome the human wants colored: the toolbar/nav bar, the active nav link, primary actions (e.g. the new numbered-paginator's current-page indicator), and links — rather than assuming the `mat.theme()` swap alone will retint the app.
3. After the swap, do a visual check in the running app against the seven listed hex values (compiling cleanly doesn't guarantee the ramp visually matches) — include this as an explicit acceptance step below rather than treating a successful build as sufficient.

**Implemented as decided**: [theme-colors.scss](../src/ui/Retrosharp.UI.Web/src/theme-colors.scss), generated via `ng generate @angular/material:m3-theme --primary-color=#196DE6`, wired into [material-theme.scss](../src/ui/Retrosharp.UI.Web/src/material-theme.scss) in place of `mat.$azure-palette`/`mat.$blue-palette`, plus `mat.toolbar-overrides()` to color the nav bar itself (M3 keeps toolbars neutral by default) and an active-route indicator on [nav-menu.html](../src/ui/Retrosharp.UI.Web/src/app/core/nav-menu/nav-menu.html)/[nav-menu.scss](../src/ui/Retrosharp.UI.Web/src/app/core/nav-menu/nav-menu.scss). Confirmed by the human: "Header bar looks mostly right, matches the brand blue."

**Addendum — the human's own theme approach has since moved past the seven-swatch idea.** The raw doc's Theme and Colors section was rewritten (not struck through, replaced) to read: *"I previously intended to use a seven-color swatch however since learning of Angular's M3 theming, I have chosen to use that around a base color. Also, a dark theme will be used and be system aware."* Recorded for history, in the order it actually happened this session:

1. [brand-colors.scss](../src/ui/Retrosharp.UI.Web/src/brand-colors.scss) was created as a Sass partial holding all seven original hand-picked swatches as named variables (`$blue-100`...`$blue-700`), for anywhere that wanted one of those *exact* hex values rather than a theme-generated tone. It was used once, for the nav bar's active-link highlight (`rgba(brand.$blue-300, 0.5)`).
2. That fixed-constant approach turned out to be exactly what the human wanted to move away from: it doesn't react to the theme at all (a plain Sass-time constant compiles to a static value — re-seeding the M3 theme, or adding a dark variant later, would do nothing to it). Reworked the active-link styling to use M3's own semantic tokens instead — `var(--mat-sys-secondary-container)` / `var(--mat-sys-on-secondary-container)`, the standard "light tint of the theme hue, with matching readable text" role pairing, generated from `theme-colors.scss`'s palettes rather than hardcoded. (Required a `!important` on the button-label color override — verified against the actual compiled bundle that Angular Material's own `MatToolbar` styles set the same custom property at higher, unencapsulated CSS specificity.)
3. Two more hardcoded colors were found by the same audit and fixed the same way, since they'd also look wrong under a future dark theme: the error-message color in [players.css](../src/ui/Retrosharp.UI.Web/src/app/players/players.css)/[player-detail.css](../src/ui/Retrosharp.UI.Web/src/app/player-detail/player-detail.css) (`#b3261e` → `var(--mat-sys-error, #b3261e)`), and the [`LoadingOverlay`](../src/ui/Retrosharp.UI.Web/src/app/shared/loading-overlay/loading-overlay.ts) scrim (a fixed white `rgb(255 255 255 / 70%)` → `color-mix(in srgb, var(--mat-sys-surface) 70%, transparent)`, so dimming works against whichever theme is active rather than always painting white).
4. `brand-colors.scss` is no longer referenced anywhere as of this addendum. Left in place rather than deleted (it's still a legitimate tool for a genuinely fixed, deliberately non-reactive brand hex), but per the human's own revised direction above, the M3 system tokens are now the default choice for anything new, not the seven-swatch file.

**Dark mode is explicitly out of scope for now**: the raw doc lists a "Retrosharp Dark Mode" section with "Color WIP," and the human separately confirmed *"I have yet to find a base color for dark mode."* `material-theme.scss` still hardcodes `color-scheme: light` and calls `mat.theme()` exactly once — there is no dark palette generated at all yet. The theme-reactive cleanup above (points 2-3) is prep work that ensures nothing in the app's own custom CSS fights a dark theme once one exists; it does not itself add one. Resume this once the human has a dark-mode base/primary color (see the raw doc's own placeholder table: light mode `#196DE6`, dark mode still blank).

### 7. Bonus Finding: Nav Links Force Full Page Reloads

**Status: Complete** — see [Step A](#implementation-plan) below.

Not from the raw doc, but found while investigating the scroll-jump issue above and squarely part of the same "navigating around the app feels janky" territory: [nav-menu.html](../src/ui/Retrosharp.UI.Web/src/app/core/nav-menu/nav-menu.html) uses plain `<a mat-button href="/players">`-style anchors for Home/Players/Teams/About instead of `[routerLink]`. Every nav click therefore triggers a full browser navigation/reload instead of an Angular Router client-side transition, discarding all in-memory app state on every click.

**Decision**: swap each nav anchor to `[routerLink]="['/players']"` (etc.), keeping `RouterLink` already imported in [nav-menu.ts](../src/ui/Retrosharp.UI.Web/src/app/core/nav-menu/nav-menu.ts) — it's imported but currently unused by the header links themselves. One-line change per link.

### 8. Players Page: "Per Page" Label Truncated

**Status: Open** — new, from the raw doc's latest revision: *"the 'Per page' drop down displays as 'Per pa'. This makes for a low quality UX."*

Root cause identified while reviewing this update: [page-number-paginator.css](../src/ui/Retrosharp.UI.Web/src/app/shared/page-number-paginator/page-number-paginator.css) fixes both the "Go to page" and "Per page" `mat-form-field`s to the same `width: 6.5rem`, which isn't wide enough for the "Per page" label at its normal font size, so Material clips it.

**Decision**: give `.page-number-paginator-size` enough width to fit its own label without truncating (either a wider fixed width sized to the longer of the two labels, or drop the fixed width and let each field size to its content/`mat-select` value instead of forcing both to match). Needs an actual visual check against the rendered label, not just "wide enough in theory," since Material's outline form-field label sizing has its own font/letter-spacing that a quick mental estimate can get wrong.

### 9. Players Page: Pagination Should Appear Above and Below the Table

**Status: Open** — new, from the raw doc's latest revision: *"The pagination should be on the both top and bottom of the table so to reduce the amount of scrolling the user needs to do"*, citing [Fangraphs' leaderboard](https://www.fangraphs.com/leaders/major-league?pos=all&stats=bat&lg=all&qual=y&type=8&season=2026&month=0&season1=2026&ind=0&pagenum=4&pageitems=30) as the reference pattern.

Today [players.html](../src/ui/Retrosharp.UI.Web/src/app/players/players.html) renders exactly one `<app-page-number-paginator>`, below the table. On a long results page this means a user who wants to jump to a different page (or a different page size) after scanning the table has to scroll all the way back down to find the control again — the exact "amount of scrolling" complaint.

**Decision**: render a second `<app-page-number-paginator>` instance above the table (below the A-Z letter nav), bound to the same `pageIndex`/`pageSize`/`totalCount` signals and the same `onPage()` handler as the existing bottom one, so the two stay in sync automatically — no new state needed, just a second instance of the same component pointed at the same inputs/output. Since `PageNumberPaginator` already has no internal state of its own (it's a pure function of its inputs, emitting events rather than mutating anything locally), two instances bound to the same signals can't drift out of sync with each other the way two independent copies of Material's own stateful `mat-paginator` could.

## Implementation Plan

Ordered by dependency and risk — items with no dependencies and small blast radius go first so they can ship immediately; the shared loading-overlay and theme regeneration are the two pieces of infrastructure other work benefits from, so they're sequenced before the table-width polish that should be visually re-verified once the theme changes anyway.

| Step | Item | Depends on | Status | Notes |
|---|---|---|---|---|
| A | Fix nav links to use `routerLink` (Finding 7) | — | **Complete** | Trivial, ships independently |
| B | Fix Player Detail `displayName` bug + add muted legal-name line (Finding 4) | — | **Complete** | Trivial, no API change |
| C | Remove "no final game" bold treatment on Players + Player Detail (Finding 3) | — | **Complete** | CSS/template only |
| D | Build shared loading-overlay component; adopt on Players + Player Detail (Finding 2) | — | **Complete** | Also finally implements frontend-prototype.md's "Page Loading" spec |
| E | Build numbered-paginator component; adopt on Players page (Finding 1) | D helps but isn't required | **Complete** | Emits the same `(page)` event shape as `MatPaginator` |
| F | Regenerate M3 theme from brand hex; apply color to nav/links/actions (Finding 6) | — | **Complete** | Also reworked to use M3 semantic tokens (`--mat-sys-secondary-container` etc.) instead of fixed brand-hex constants, per the human's own pivot away from the seven-swatch approach — see Finding 6's addendum |
| G | Tighten statistics-table column padding + sort-header spacing (Finding 5) | F (re-verify visually once real theme colors are in); **also blocked on data** — needs a real 10+ season career to verify against | **Deferred** | Do this last; re-check against a long real career, not a short mock one |
| H | Widen/re-size the "Per page" field so its label doesn't truncate (Finding 8) | — | **Open** | Small CSS fix, ships independently |
| I | Add a second `<app-page-number-paginator>` above the table (Finding 9) | E (done) | **Open** | Bind to the same signals/handler as the existing bottom instance |
| — | System-aware light/dark theme | F (done) | **On hold** | Blocked on the human choosing a dark-mode base/primary color — see Finding 6's addendum. Not scheduled until that's decided |

Steps A-D can be built and shipped in parallel with each other; E to G touch overlapping visual surface area and are easiest to verify in that order. H and I are both small, independent, and can ship any time.

**Step G ordering note (added after Steps A-D shipped)**: the currently-imported seasons don't yet contain a player with a long enough career (10+ years) to meaningfully verify the column-width fix against real data — a short mock career wouldn't exercise the "Team(s)" column holding a mid-season trade, or confirm the table actually fits without scrolling once real row variety is present. Step G is deferred until 10+ seasons are imported, and should be the last step done for that reason, regardless of where F lands.

**Steps H and I added** after the human spent time with the Step E paginator in the real app and found two follow-on issues — see Findings 8 and 9.

## Acceptance Criteria

- [x] Players page: page-number buttons (windowed, with a "go to page" input) replace prev/next-only navigation; clicking a page number, or Next/Previous, does not visibly shift scroll position.
- [x] Players page and Player Detail page: no bold "no final game on record" styling remains anywhere; the underlying dates still render normally in their existing columns/rows.
- [x] Player Detail page: name renders as "`[useName] [surname]`" (matching the Players page's own logic), with the full legal name shown as a smaller, muted line beneath it only when it differs from the displayed name.
- [ ] Player Detail page: a real 20+-season player's batting and pitching tables fit within the page's normal content width on a standard desktop viewport (~1280px window) without horizontal scrolling; `overflow-x: auto` remains as a fallback for narrower viewports. *(blocked on data — see Finding 5/Step G)*
- [x] The app visibly reflects the brand primary color (spot-checked against the toolbar/nav, a primary button, and the active-link state) rather than Material's default Azure/Blue system palettes. *(criterion itself superseded — see Finding 6's addendum: the human has moved from "match these seven exact swatches" to "theme from one M3 seed color," which this now satisfies)*
- [x] Clicking Home/Players/Teams/About in the header performs a client-side route transition (no full-page reload/flash).
- [ ] Players page: the "Per page" field's label renders in full, not truncated to "Per pa." *(Finding 8/Step H)*
- [ ] Players page: a page-number paginator control appears both above and below the results table, and the two stay in sync. *(Finding 9/Step I)*
- [ ] *(On hold)* The app offers a dark theme that follows the OS/browser's `prefers-color-scheme` — not scheduled until a dark-mode base color is chosen.
