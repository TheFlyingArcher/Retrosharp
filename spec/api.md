# Data Viewing API

## Overview

The Data Viewing API is the read-only REST surface `Retrosharp.UI.Api` exposes over the data populated by the Seed Data, Person, Game Log, and Game Event parsers (Steps 1-6). It covers the three Phase 1 features project.md groups together — **Player Search**, **Data Viewing**, and **Statistics** — as a single API surface, since in practice they share the same underlying data and endpoints (a player page needs both their identity and their statistics; a game page needs both its summary and its play-by-play).

Everything in this spec is new. Unlike Steps 1-6, there is no existing partial implementation to correct except one pre-existing scaffold, flagged below in [Considerations](#considerations).

## Scope

**In scope (Phase 1)**:
- Player search by name.
- Player career and season batting/pitching/fielding statistics, including rate stats (AVG, OBP, SLG, OPS, BABIP, WHIP, ERA, FIP, HR/FB, K/9, HR/9, BB/9, FP).
- Player game logs (per-game statistics within a season).
- Team search, team roster for a season, and team season statistics (counting and rate).
- Game search and game summary (final score, box score, lineups).
- Game play-by-play (ordered event detail for a single game).

**Out of scope (Phase 2, per project.md)**: CSV export, cross-season team trend analysis, administrative endpoints, authentication/authorization enforcement, favorites, the Negro Leagues section, ejection-specific endpoints (depends on the deferred `GameEjection` table).

## Considerations

### No authentication in Phase 1

`Retrosharp.UI.Api`'s `Program.cs` already configures JWT bearer authentication via Microsoft Identity Web (`AddMicrosoftIdentityWebApi`), but none of the existing controllers (`PersonController`, `GameLogController`, `GameEventController`) apply `[Authorize]` — they're all reachable anonymously today. project.md places SSO and "maintain anonymous access... while still providing access to basic features" under **Second Phase**. Data Viewing endpoints follow the same precedent as the existing ETL controllers: no `[Authorize]` attribute, reachable anonymously. This isn't a gap to fix in Step 7 — it's the intended Phase 1 state, since there is no user/role model yet for `[Authorize]` to check against.

### Pre-existing scaffold needs auditing, not blind reuse

`Retrosharp.Service.PlayerStatisticsService` / `IPlayerStatisticsService` / `Retrosharp.Contract.Batting.BattingStatistics` already exist in the codebase, predating this build plan (same category as the pre-existing scaffolds Steps 3/5 found and had to fix). `BattingStatistics : Batting` adds computed properties for `BattingAverage`, `OnBasePercentage`, `SluggingPercentage`, `OnBasePlusSlugging`, `BattingAverageOnBallsInPlay`, `IsolatedPower`, and `TotalBases` — the formulas were checked against this spec's [Statistic Formulas](#statistic-formulas) section and are correct. `PlayerStatisticsService.CalculateCareerBattingStatsAsync` aggregates a player's `Batting` rows into a career total correctly. Neither is wired to a controller, neither is registered for DI verification via a live endpoint, and neither has a unit test. Step 7 should verify (not assume) this code is correct end-to-end against real data before building on it, and extend the same pattern for pitching and fielding (`PitchingStatistics`/`FieldingStatistics` don't exist yet).

One stale doc comment should be fixed while touching this code: `Batting.Hits`'s XML doc says "Total singles," but every consumer (`BattingStatistics.SluggingPercentage`'s `TotalBases` formula, `PlayerStatisticsService`) treats it as **total hits** (1B+2B+3B+HR), which is also the correct baseball definition given `Doubles`/`Triples`/`Homeruns` are tracked as separate fields. The field itself and all derivation logic (Step 6d) are correct; only the comment is wrong.

### `Pitching.InningsPitched` stores outs, not innings

Despite the name, `Pitching.InningsPitched` (and `PitchingDelta.InningsPitched`) is total **outs recorded**, not decimal innings (confirmed in `GameStatisticsResolver.cs`: `InningsPitched = kv.Value.OutsRecorded`). This sidesteps the classic baseball-stats bug where "6.1 innings" means 6⅓ innings, not 6.1 decimal innings — but only if every rate-stat formula divides by `InningsPitched / 3.0` to get real innings, not by `InningsPitched` directly. Every pitching formula in this spec does so explicitly. Any future work on this table should keep storing outs for the same reason; the field is a bad name for a good design, not a bug.

### Home runs allowed and fly balls allowed aren't stored as pitcher aggregates — derive one shared query instead of two

`Pitching` has no `HomerunsAllowed` or `FlyBallsAllowed` column, and `GamePitchingStatistics` (team-level) has no hits/BB/K/innings columns at all (only `PitchersUsed`, `IndividualEarnedRuns`, `TeamEarnedRuns`, `WildPitches`, `Balks`). This means **HR/9, HR/FB, FIP, and pitcher BABIP** cannot be computed from stored aggregates alone — they all need per-pitcher-per-season counts that only exist at the `GameEvent` level (`EventType = HomeRun`, `BattedBallType = FlyBall`, and at-bat/sac-fly classification for BABIP's denominator).

Rather than building four separate derivations, Step 7 should build one `PitcherEventAggregate` query (grouping `GameEvent` rows by `PitcherId` for a given season, joined through `Game.GameDate` for the year) that returns `HomerunsAllowed`, `FlyBallsAllowed`, `AtBatsAgainst`, and `SacrificeFliesAgainst` in a single pass — reusing the same `EventType` classification rules Step 6d's `GameStatisticsResolver` already built and verified for the batter side, just grouped by pitcher instead of batter. This is a live query, not a new stored column, accepted as a reasonable Phase 1 tradeoff given the data volumes seen in Steps 5/6 (a single season is ~185,000 `GameEvent` rows across 30 teams; filtered to one pitcher-season it's a small, indexable slice). If this proves too slow once real usage patterns are known, promoting these to stored aggregate columns (updated during Step 6d's derivation, the same way `Batting`/`Pitching`/`Fielding` already are) is the natural Phase 2 follow-up — flagged here rather than pre-built, since it isn't needed until it's proven necessary.

### FIP includes a league-normalizing constant, computed from Retrosharp's own data

The commonly published FIP formula adds a league-and-year-specific constant so FIP sits on the same scale as ERA. That constant isn't external reference data like park factors — it's fully derivable from Retrosharp's own imported data (league-wide ERA compared against the league's own raw HR/BB/HBP/K/IP totals for the year), so it belongs in Phase 1 rather than being deferred.

The constant is computed per `(LeagueId, SeasonYear)`, not MLB-wide, since AL and NL had genuinely different run environments for most of the DH era (1973-2021 for the AL only, universal since 2022) — collapsing both leagues into one constant would understate NL pitchers' FIP and overstate AL pitchers' for every DH-era season. This requires one new aggregate, `LeagueSeasonPitchingTotals`, computed per league-year by summing across every franchise in that league for that season:

- `TeamEarnedRuns` — summed from `GamePitchingStatistics.TeamEarnedRuns` (authoritative team-earned figure, same source used for team ERA elsewhere in this spec).
- `HomerunsAllowed` — summed via the same `GameEvent`-level query `PitcherEventAggregate` already uses, grouped by league-year instead of by pitcher.
- `BaseOnBalls`, `HitBatsmen`, `Strikeouts`, `IP` — summed from that league-year's franchises' `Pitching` rows.

```
leagueEra          = 9 * sum(TeamEarnedRuns) / sum(IP)
leagueRawComponent = (13*sum(HomerunsAllowed) + 3*(sum(BaseOnBalls)+sum(HitBatsmen)) - 2*sum(Strikeouts)) / sum(IP)
fipConstant         = leagueEra - leagueRawComponent
```

A pitcher-season's FIP applies their league's constant for that year. For the rare case of a player traded between leagues within one season (an AL-to-NL or NL-to-AL trade), the combined-season FIP (see the "combined total row" convention above) uses an innings-weighted blend of the two applicable league constants, rather than picking one league arbitrarily — consistent with how every other combined-total rate stat in this spec is recomputed from summed totals rather than averaged. The response for any FIP value includes the constant actually applied and the `(LeagueId, SeasonYear)` (or blend) it came from, for transparency and so a discrepancy against a published figure can be traced to a specific number rather than treated as unexplained.

### Pitcher BABIP reuses the same `PitcherEventAggregate` derivation

Standard pitcher BABIP is `(Hits - HomerunsAllowed) / (AtBatsAgainst - Strikeouts - HomerunsAllowed + SacrificeFliesAgainst)`. `AtBatsAgainst` and `SacrificeFliesAgainst` aren't stored anywhere either, but both fall directly out of the same `PitcherEventAggregate` query described above (the same `EventType` classification rules that determine whether a batter's plate appearance counts as an at-bat already exist and are already correct, per Step 6d). No separate mechanism needed.

### A season split across multiple franchises produces a combined total row

`Batting`/`Pitching`/`Fielding` are keyed by `(PersonId, FranchiseId, SeasonYear[, Position])`, so a player traded mid-season has multiple rows for that year. Season endpoints return one entry per franchise the player appeared for that season, **plus** a computed combined total (summed counting stats, rate stats recomputed from the summed totals — not averaged) when more than one franchise row exists for that season, mirroring the standard "TOT" row convention used by other baseball statistics sites. `Fielding`'s extra `Position` dimension means a player can also have multiple rows for the *same* franchise-season (a utility player); the combined total for a franchise-season sums across positions too, alongside the per-position breakdown.

### Team season statistics: two different authoritative sources for two different things

`GameBattingStatistics`/`GameFieldingStatistics` (summed across a franchise's games for a season) are the authoritative source for team batting and fielding counting stats and rate stats — they're the Game Log Parser's own team-level totals, already reconciled against the Game Event Parser's derived totals in Step 6e. Team fielding percentage is computed directly from summed `GameFieldingStatistics.Putouts`/`Assists`/`Errors`.

`GamePitchingStatistics` is *not* sufficient for team pitching rate stats — it only carries `PitchersUsed`, `IndividualEarnedRuns`, `TeamEarnedRuns`, `WildPitches`, and `Balks`, with no hits/walks/strikeouts/innings at team granularity. Team pitching rate stats (ERA, WHIP, K/9, BB/9, HR/9, FIP) are instead computed by summing that franchise-season's individual pitchers' `Pitching` rows (hits, BB, K, innings-as-outs, and the `PitcherEventAggregate` values), **except** the ERA numerator, which uses summed `GamePitchingStatistics.TeamEarnedRuns` rather than summed individual earned runs, since `TeamEarnedRuns` is specifically the authoritative team-earned figure (distinct from the sum of each pitcher's individually-earned runs, per the Team Earned Run Phase 2 note in [game-event.md](./game-event.md#future-enhancement-phase-2-team-earned-run-reconciliation-and-passed-balldouble-play-reconciliation)).

### Player game logs are derived on demand, not stored

project.md's Player Search feature explicitly promises "career statistics and game logs," but Phase 1's schema has no per-game player statistic line — only season aggregates (`Batting`/`Pitching`/`Fielding`) and event-level detail (`GameEvent`/`GameEventRunner`/`GameEventFieldingCredit`). A player's game log is derived per request: group that player's `GameEvent`/`GameEventRunner` rows (as batter) or `GameEvent` rows (as pitcher) by `GameId` within the requested season, and apply the same counting-stat classification Step 6d already uses, just grouped per-game instead of accumulated per-season. This is naturally bounded (one player, one season, at most ~162 games) so performance isn't a concern the way a league-wide query would be.

### Response DTOs, not Contract entities, cross the API boundary

Controllers return dedicated response classes (`Retrosharp.UI.Api/Models/`, mirroring how `GameLogImportRequest` already lives alongside its controller), not the `Retrosharp.Contract.*` entities directly. This keeps the JSON shape controllable independent of the data layer (denormalizing a franchise code onto a game summary, flattening rate stats alongside counting stats, omitting internal-only fields) and matches the separation of concerns the repository pattern already establishes for the data layer.

### Pagination

List/search endpoints (`players/search`, `teams/search`, `games/search`) accept `limit` (default 25, max 100) and `offset` (default 0) query parameters, and return a `{ items, totalCount, limit, offset }` envelope. No existing convention to follow here — this is the first read/list endpoint shape in the project — so this is a new, deliberately simple convention rather than adopting a heavier standard (cursor pagination, `Link` headers) not otherwise needed at Phase 1's data volumes.

## API Surface

All routes are `GET`, prefixed `/api`, and return `200 OK` with a JSON body on success, `404 Not Found` for a missing single-resource lookup (player/team/game by ID), and `400 Bad Request` for invalid query parameters.

| Route | Description |
|---|---|
| `GET /players/search?q=&limit=&offset=` | Search players by name (surname, use name, or full name — reuses `IPersonRepository.SearchByNameAsync`). |
| `GET /players/{personId}` | Player identity/biographical detail. |
| `GET /players/{personId}/batting?season=` | Batting stats — all seasons, or one season (with the combined-total row when applicable), including rate stats. |
| `GET /players/{personId}/pitching?season=` | Same shape, pitching. |
| `GET /players/{personId}/fielding?season=` | Same shape, fielding (broken out by position). |
| `GET /players/{personId}/games?season=&type=batting\|pitching` | Per-game statistic lines for one player-season, derived on demand. |
| `GET /teams/search?q=&code=&season=&limit=&offset=` | Search franchises by name/nickname or code; `season` disambiguates codes reused across a franchise's eras (see Step 2's `FranchiseCode` non-uniqueness finding). |
| `GET /teams/{franchiseId}` | Franchise identity detail. |
| `GET /teams/{franchiseId}/roster?season=` | Players who recorded a `Batting`, `Pitching`, or `Fielding` row for that franchise-season. |
| `GET /teams/{franchiseId}/stats?season=` | Team season statistics, counting and rate (see [Considerations](#team-season-statistics-two-different-authoritative-sources-for-two-different-things)). |
| `GET /games/search?date=&season=&franchiseId=&limit=&offset=` | Search games by date, season, and/or participating franchise. |
| `GET /games/{gameId}` | Game summary: final score, both teams' box-score totals, both starting lineups, decisions (winning/losing/saving pitcher), umpires, ballpark. |
| `GET /games/{gameId}/events` | Full play-by-play: `GameEvent` rows (with nested runners and fielding credits) interleaved with `GameSubstitution`/`GameAdjustment`/`GameComment` context records, ordered by `Sequence`. |

## Statistic Formulas

Batting formulas reuse the existing, verified `BattingStatistics` computed properties. Pitching and fielding formulas are new for Step 7. `IP` below always means `InningsPitched / 3.0` (real innings), never the raw stored value (see [Considerations](#pitchinginningspitched-stores-outs-not-innings)).

| Stat | Formula | Source |
|---|---|---|
| AVG | `Hits / AtBats` | `Batting` |
| OBP | `(Hits + BaseOnBalls + HitByPitches) / (AtBats + BaseOnBalls + HitByPitches + SacrificeFlies)` | `Batting` |
| SLG | `(Hits + 2*Doubles + 3*Triples + 4*Homeruns) / AtBats` | `Batting` |
| OPS | `OBP + SLG` | derived |
| BABIP (batter) | `(Hits - Homeruns) / (AtBats - Strikeouts - Homeruns + SacrificeFlies)` | `Batting` |
| ERA | `9 * EarnedRuns / IP` | `Pitching` |
| WHIP | `(Hits + BaseOnBalls) / IP` | `Pitching` |
| K/9 | `9 * Strikeouts / IP` | `Pitching` |
| BB/9 | `9 * BaseOnBalls / IP` | `Pitching` |
| HR/9 | `9 * HomerunsAllowed / IP` | `Pitching` + `PitcherEventAggregate` |
| HR/FB | `HomerunsAllowed / FlyBallsAllowed` | `PitcherEventAggregate` |
| FIP | `(13*HomerunsAllowed + 3*(BaseOnBalls + HitBatsmen) - 2*Strikeouts) / IP + fipConstant` | `Pitching` + `PitcherEventAggregate` + `LeagueSeasonPitchingTotals` |
| BABIP (pitcher) | `(Hits - HomerunsAllowed) / (AtBatsAgainst - Strikeouts - HomerunsAllowed + SacrificeFliesAgainst)` | `Pitching` + `PitcherEventAggregate` |
| FP | `(Putouts + Assists) / (Putouts + Assists + Errors)` | `Fielding` / `GameFieldingStatistics` |

All rate stats return `0` (not `NaN`/error) when their denominator is `0`, matching `BattingStatistics`'s existing convention.

## Requirements

1. Every endpoint in the [API Surface](#api-surface) table is implemented and reachable anonymously (no `[Authorize]`), per [Considerations](#no-authentication-in-phase-1).
1. Response shapes are dedicated DTOs in `Retrosharp.UI.Api/Models/`, not `Retrosharp.Contract.*` entities returned directly.
1. Rate statistics follow the [Statistic Formulas](#statistic-formulas) section exactly, including outs-to-innings conversion for every pitching rate stat.
1. The pre-existing `PlayerStatisticsService`/`BattingStatistics` scaffold is verified against real data (not assumed correct) before Step 7 builds on it, and the `Batting.Hits` doc-comment fix is applied.
1. A season split across multiple franchises returns one row per franchise plus a combined total row, per [Considerations](#a-season-split-across-multiple-franchises-produces-a-combined-total-row).
1. Team pitching rate stats are computed by summing that franchise-season's player-level `Pitching` rows (with `TeamEarnedRuns` substituted for the ERA numerator), not from `GamePitchingStatistics`, which lacks the necessary columns.
1. HR/9, HR/FB, FIP, and pitcher BABIP are computed via a single shared `PitcherEventAggregate` query per pitcher-season, not four independent derivations.
1. FIP includes the league-normalizing constant, computed per `(LeagueId, SeasonYear)` via the `LeagueSeasonPitchingTotals` aggregate described in [Considerations](#fip-includes-a-league-normalizing-constant-computed-from-retrosharps-own-data), with an innings-weighted blend applied for a player-season split across leagues. The constant value and the league-year(s) it was derived from are included in the response.
1. Player game logs are derived on demand from `GameEvent`/`GameEventRunner`, scoped to one player and one season per request.
1. List/search endpoints support `limit`/`offset` pagination with the envelope shape described in [Considerations](#pagination), with sensible defaults and an enforced maximum `limit`.
1. Team search accounts for `FranchiseCode` non-uniqueness across a franchise's eras (Step 2 finding) by supporting an optional `season` filter.
1. Game play-by-play interleaves `GameEvent` with the context tables (`GameSubstitution`, `GameAdjustment`, `GameComment`) in a single chronologically-ordered response, using `Sequence`.

## Acceptance Criteria

1. Every route in [API Surface](#api-surface) returns correct data verified against real, already-imported reference data (the same `2025SDN.EVN`/`2025SEA.EVA`/`gl2025.txt`/`biofile0.csv` data used to verify Steps 3, 5, and 6), not just unit-tested fixtures.
1. Rate statistics for at least one well-known real player-season are spot-checked against a published, independently-verifiable reference figure for AVG, OBP, SLG, ERA, and WHIP at minimum.
1. FIP is **not** spot-checked against a published figure in Step 7 — with only a small number of team-season event files imported at this point in the build plan (see [Considerations](#fip-includes-a-league-normalizing-constant-computed-from-retrosharps-own-data)), `leagueEra`/`leagueRawComponent` cannot yet be computed from a real, complete league, since they require every team in a league-season to be imported. Comparing a self-derived FIP against a published figure (for example, Baseball-Reference) is deferred to Step 9, which is where a complete season across every team is first guaranteed to exist. Step 7 instead verifies the FIP constant formula itself — `leagueEra`, `leagueRawComponent`, and `fipConstant` — via unit tests against constructed league-total fixtures with known, hand-computed expected outputs, independent of how much real data happens to be imported at the time.
1. A season split across two franchises produces correct per-franchise rows and a correct combined total. This uses a **constructed fixture** (hand-built, small, with known expected combined totals — the same technique already used successfully in Steps 6b/6d/6f), not a real mid-season trade from the imported data: with only `2025SDN.EVN`/`2025SEA.EVA` imported, a real 2025 trade (for example, Ryan O'Hearn or Ramón Laureano moving to the Padres at that year's deadline) only has one side of the trade actually in the database — the player's prior club's stats don't exist at all until that club's own event file is imported. Since this check validates our own aggregation logic rather than comparing against an external published figure (unlike FIP), a constructed fixture with precisely known expected values is no less rigorous than a real trade, and avoids importing an extra team's file solely for this one check.
1. A shared game (the same physical game imported from both teams' event files, per [game-event.md](./game-event.md#considerations)) returns identical play-by-play regardless of which team's perspective is queried.
1. Pagination behaves correctly at boundary conditions (empty result set, exact-multiple-of-`limit` result set, `offset` beyond the end of the result set).
1. All endpoints are reachable without an `Authorization` header.
