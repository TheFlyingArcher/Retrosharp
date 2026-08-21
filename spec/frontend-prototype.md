# Retrosharp Frontend Prototype

## Overview

This is to prototype out designs and ideas for the frontend of Retrosharp. The goal is to create a user-friendly interface that allows users to easily navigate and interact with the application. This is to satisfy Feature 3 found in [project.md](project.md) and it is a part of Phase One of the project. The frontend will be built using Angular and will communicate with the backend via RESTful APIs.

The goal of the front end is to allow an end user to search baseball players, franchises, seasons, and games. The user should be able to view detailed information about each entity, including statistics, historical data, and related media. The frontend should also provide a way for users to filter and sort the data based on various criteria.

## Pages

Below these are the pages that will be in the prototype. Each page will have requirements and features that will be implemented in the frontend.

### Retrosharp

This refers to the application as a whole. Here, common components such as headers and footers will be implemented. The header will contain the application name, navigation links, and a search bar. The footer will contain copyright information and links to social media. These components will be consistent across all pages, providing a cohesive user experience.

#### Header

The header will contain the following items:
- Application name: Retrosharp
- Navigation links: Home, Players, Franchises, Seasons
- PHASE TWO: Sign on and sign out buttons as well as a user profile link.

#### Footer

The footer will contain the following items:
- Copyright information: © 2026 Retrosharp. All rights reserved.
- My GitHub link: A link to the Retrosharp GitHub repository.
- Report issue link: A link to report issues or bugs with the application, points to GitHub issues section of repository.

#### Page Loading

When data is loaded there is a central loading spinner that will be displayed in the center of the page. This will be used when data is being fetched from the backend and will provide feedback to the user that the application is working on their request. The background will also dim and the user will not be able to interact with the page until the data is loaded. This will prevent users from interacting with the page while data is being fetched and will provide a better user experience, prevent errors, and reduce the number of requests to the backend.

#### Errors

Not found (404) and server errors (500) will be handled gracefully. The user will be redirected to a custom error page that will display a friendly message and provide links to navigate back to the home page or other pages. This will improve the user experience and prevent users from being stuck on an error page.

#### Shared Components

##### Statistics Tables

These are common tables that will be used across multiple pages to display statistics for players, franchises, and seasons. The tables will be sortable on all columns and will include pagination to improve performance and usability. These tables may be standalone components or they may be integrated into other tables, depending on the design of the page.

#### Resolved: RBI and Games Played/Started Were Missing from `Batting`

The player-season `Batting` table (and its `BattingDelta`/`BattingLine` mirrors) had no `RunsBattedIn`, `GamesPlayed`, or `GamesStarted` fields at all — RBI existed only at the play level (`GameEventRunner.IsRBI`) and team level (`GameBattingStatistics.RunsBattedIn`), and G/GS for batters didn't exist anywhere (unlike pitchers, who already had both via `Pitching.GamesPitched`/`GamesStarted`). This broke the hitters columns below on every page that uses this shared table.

Fixed by adding all three fields end-to-end (`Batting`/`BattingModel`/`BattingDelta`/`BattingStatistics`/`BattingLine`, plus the box-score and per-game-log DTOs) and deriving them in [`GameStatisticsResolver`](../src/lib/Retrosharp/Format/PlayByPlay/GameStatisticsResolver.cs):
- **RBI**: summed per batter from `IsRBI` flags on that play's runners — the same rule `GameReconciliationResolver` already uses for the team-level total, just attributed to the current play's batter instead of the batting team.
- **Games Played**: `1` unconditionally for every person with any batting involvement in the game, mirroring how `Pitching.GamesPitched` is already derived.
- **Games Started**: sourced from the event file's own "start" records (`BattingOrder` 1-9, which already excludes a non-batting DH-era starting pitcher) — passed into the resolver as an explicit `startingBatterFranchiseIds` map by [`GameEventImportService`](../src/lib/Retrosharp.Service/GameEventImportService.cs), since the pure resolver only sees plays and can't otherwise tell a starter apart from a bench player.

**Scope note**: `GamesStarted` is only correct for the season-aggregate path (`GameEventImportService`, which persists `Batting`), since that's the only caller with access to the game's starting lineup. The on-demand per-game paths (`PlayerGameLogService`, `GameSummaryService`'s box score) don't pass starter data, so `GamesStarted` isn't exposed on `GameBattingLine`/`GameBoxScoreBattingParticipantStats` — only `RunsBattedIn` was added there, since that field is fully self-contained per play regardless of caller.

**Backfill note**: like the Step 7h starting-pitcher/line-score fix, this only affects games processed *after* the fix — `GameEventGameStatus`'s per-game claim prevents already-applied games from being reprocessed, so existing imported seasons need a wipe-and-reimport (or a one-off backfill script) before these three fields are accurate for them.

Columns for hitters:

- Year
- Team(s)
- Games (Played) (G)
- Games Started (GS)
- At Bats (AB)
- Runs (R)
- Hits (H)
- Doubles (2B)
- Triples (3B)
- Home Runs (HR)
- Runs Batted In (RBI)
- Stolen Bases (SB)
- Caught Stealing (CS)
- Strikeouts (K)
- Walks (BB)
- Intentional Walks (IBB)
- Average (AVG)
- On Base Percentage (OBP)
- Slugging Percentage (SLG)
- On Base Plus Slugging (OPS)
- Hit By Pitch (HBP)
- Sacrifice Hits (SH)
- Sacrifice Flies (SF)
- Total Bases (TB)
- Grounded Into Double Plays (GIDP)

Columns for pitchers:

- Year
- Team(s)
- Games (G)
- Games Started (GS)
- Innings Pitched (IP)
- Wins (W)
- Losses (L)
- Saves (SV)
- Hits Allowed (H)
- Runs Allowed (R)
- Earned Runs Allowed (ER)
- Strikeouts (K)
- Walks (BB)
- Intentional Walks (IBB)
- Home Runs Allowed (HR)
- Hit Batsmen (HBP)
- Balks (BK)
- Batters Faced (BF)
- Earned Run Average (ERA)
- Hits Per Nine Innings (H9)
- Walks Per Nine Innings (BB9)
- Strikeouts Per Nine Innings (K9)
- Walks and Hits Per Inning Pitched (WHIP)
- Home Runs Per Nine Innings (HR9)

### Home Page

Path: / or /home
This is the landing page for the application. It will provide an overview of the application and its features, as well as links to the other pages. The home page will also include a search bar that allows users to quickly find players, franchises, seasons, and games. The search bar will feature type ahead suggestions to help users find what they are looking for more efficiently. Upon selecting either a player, franchise, season, or game from the search results, the user will be redirected to the corresponding detail page.

#### Resolved: "On This Day in Baseball History" Widget

A single text search bar leaves a lot of empty space on the page. Considered and rejected: pulling in a live MLB news feed (`mlb.com/feeds/news/rss.xml`) or the unofficial `statsapi.mlb.com` — both are external dependencies with their own availability/ToS risk, and both are oriented around the *current* MLB season, which sits awkwardly next to an app whose whole identity is historical Retrosheet data Retrosharp already owns. **Decision: a self-sourced "On this day in baseball history" widget**, using only data already imported by this project — no external API, no ToS risk, works offline in local dev.

**The offseason problem, and why "games played on this day" alone doesn't solve it**: MLB's regular season has historically run roughly April-October, and Retrosharp only imports Retrosheet's *regular-season* Game Logs — postseason isn't imported by any parser (see [project.md](./project.md)'s Second Phase list, postseason import added there as part of this same decision). A calendar-date lookup against `Game.GameDate` for any day between November and March returns zero results in every year, with no exceptions. The same clustering problem rules out `Person.PlayerDebutDate`/`PlayerLastDate` as a fix — debuts and final games happen *during* the season too, so they go just as quiet in the offseason as `Game` does.

**What actually covers the offseason**: `Person.BirthDate` and `Person.DeathDate` aren't clustered by baseball season at all — they're spread roughly uniformly across all 366 calendar days. With ~27,000 people imported by the biofile parser, that's on the order of 70+ people born on any given calendar date, offseason included. "Born on this day" and "died on this day" (matched on birth/death month-and-day against today's date, independent of year) are the widget's year-round backbone. `Game`-sourced entries ("On this day in `year`, `Team A` beat `Team B` `X-Y`") are folded in whenever they exist, giving richer content in-season without the widget depending on them.

**Fallback for a day with no matches at all**: a rotating record-book card not tied to any date — all-time single-season leaders (from `Batting`/`Pitching`), a random Hall of Famer spotlight (`Person.IsHof`), a random franchise history fact (`Franchise.FranchiseStart`/former names, via the franchise all-time summary work), or a notable season from `FranchiseSeasonStanding` (best/worst win percentage, longest droughts). None of these need a date match, so this fallback never runs dry.

**Every player named in the widget links to their player detail page** — a "born on this day" or "died on this day" entry, and any player named within a "games played on this day" entry (batters, pitchers, anyone else surfaced), is a link to `/players/[id]`, giving a one-click path from the home page into that player's full career, consistent with every other player reference elsewhere in this document.

**Tone note**: defaulting to "who died today" every single day may read as morbid for a casual browsing widget even though the data fully supports it — worth a design pass on how prominently death-anniversary entries are surfaced versus births/games, but not a blocker to building this.

**Backend dependency**: no endpoint currently supports a "by calendar month/day, any year" lookup against `Person`/`Game` — every existing search/browse route filters by a specific year or an exact date, not a recurring month-day pattern. This needs new query support before the widget can be built.

### Players Page

Path: /players

On this page, users will be able to view a table of baseball players grouped by the player's last name.
There is on top of the page the alphabet A-Z in which a user can click a letter and a table will load with players whose last name starts with that letter. Clicking on a player's name will take the user to the player detail page. The table of players will be paginated to improve performance and usability. Bold text (with a tooltip explaining the caveat) indicates the player has no final game on record in Retrosheet data — most often because they're still active, but see the note below. A cross next to their name will indicate the player is deceased.

The table contains the following columns:

- Name ("UseName Surname", with the aforementioned requirements)
- Birth Date
- Death Date
- Age(d)
- Birth Place ("BirthCity, BirthStateProvince, BirthCountry" e.g. "San Diego, California, USA" or "Cienfuegos, Cuba" if no state province)
- Death Place (follows same formatting as Birth Place)
- Bats
- Throws
- Height (in "[ft]' [in]" format)
- Weight (in lbs)
- Player Debut
- Player Last

#### Resolved: Determining "Is Active" — reframed as "No Final Game on Record"

There is no explicit `IsActive` field within the underlying data store, and — after further discussion — no way to safely derive one either. Retrosheet is a historical record of completed seasons, community-maintained and always at least one season behind, not a live roster feed. `PlayerLastDate` (sourced from the Retrosheet biofile's `last_p` column) is nullable, but a `null` value only means Retrosheet has not recorded a final game for this player — not that they are active in MLB *right now*. The gap between those two claims is exactly the "farewell tour" case (e.g. Albert Pujols, Yadier Molina): a player can play a full final season, appear with a non-null `PlayerLastDate` only after the following offseason's biofile update, and in the meantime nothing in this dataset can distinguish "genuinely active" from "recently retired, not yet reflected." The reverse also holds near a season boundary — a player who just debuted or is mid-season has a `null` last-game date for the ordinary reason that their career isn't over, which is indistinguishable in the data from the stale-retirement case.

Given that, the UI does not claim a true active/retired status. Bold text on the Players page means only "`PlayerLastDate` is `null`", labeled and tooltipped as "no final game on record" rather than "active", so the page never asserts something the data can't back up. Deriving from the most-recent-season's `Batting`/`Pitching`/`Fielding` rows (considered as a fallback) doesn't solve this either — it narrows *when* the last known appearance was, but still can't tell a genuinely active player from one who retired after that season, so it isn't worth the added query cost. Getting an actually-accurate live status would require an external live-roster source (e.g. `statsapi.mlb.com`), which was already rejected for this project on the "On This Day" widget (see above) as an unwanted external/ToS dependency for an app whose whole identity is self-contained historical Retrosheet data — the same reasoning applies here.

#### Resolved: Deceased Indicator

`PlayerSearchResult` (the DTO backing both the browse list and free-text search) had no `DeathDate`, so the "cross next to deceased players" indicator had nothing to render from, even though `Person.DeathDate` already exists. Fixed by adding a matching `DeathDate` property to `PlayerSearchResult` — Mapster's convention-based mapping picks it up automatically, no controller change needed, same as the earlier burial-location fix.

#### Resolved: Browsing by Surname

The only player-listing route was `players/search`, which requires a non-empty free-text `q` and `Contains`-matches surname/use name/full name together — there was no way to list every player ordered by surname, or restrict to one starting letter for the A-Z jump nav, and `PlayerSearchResult` didn't even carry `Surname` separately from `FullName`/`UseName`.

Fixed with a dedicated browse route, `GET /players?letter=&limit=&offset=` ([api.md](./api.md#players-page-browse-list-needs-surname-not-just-fulluse-name)), backed by a new `IPersonRepository.BrowseBySurnameAsync(letter, limit, offset)` — orders by surname (then use name), optionally filtered to surnames starting with `letter`, paginated the same way as the other list endpoints. `PlayerSearchResult` now also exposes `Surname`. Each A-Z letter link on this page queries `?letter=X`; the unfiltered `?letter=` (omitted) case powers the plain paginated full list.

### Player Detail Page

Path: /players/[id]

The detail page will display the details of the player with that ID. The player detail page will include information such as the player's name, position, height, weight, birthdate, birth city, death date, death city, burial location, team, statistics, and related media.

#### Resolved: Burial Location

`Person` already carries burial data (`Cemetery`, `CemeteryCity`, `CemeteryStateProv`, `CemeteryCountry`, `CemeteryNote`), but the `PlayerDetail` response DTO omitted them — burial location was undisplayable via the API despite existing in the data store. Fixed by adding the five matching properties to `PlayerDetail` ([PlayerDetail.cs](../src/ui/Retrosharp.UI.Api/Models/PlayerDetail.cs)); the controller uses Mapster's convention-based `Adapt<PlayerDetail>()`, so no controller change was needed once the DTO had matching property names.

There will be a table containing the following columns based on position:
- If the player is a pitcher: Use shared components statistics table for pitchers.
- If the player is a batter: Use shared components statistics table for hitters.

Table will be sortable on all columns. MLB careers rarely span more than 20 years, so pagination shouldn't be a necessity. If a player has played for multiple teams in a single year, the table will display each team on a separate row. Despite mid-season trades, rows shouldn't exceed 30 rows.

**A Note on Franchises:** It is exceedingly rare, however franchises have changed name and/or location during a player's playing career on that franchise. Case in point is in 2002 when the California Angels were renamed to the Anaheim Angels. In this case, the franchise is the same, but the name has changed. The player detail page will display the franchise name as it was during the player's tenure with that franchise. The franchise detail page will display all names and locations of the franchise throughout its history.

### Player Season Detail Page

Path: /players/[id]/seasons/[year]

This page will display all the games the player played in that season. The page will also display the player's statistics for that season, as well as any awards or honors the player received during that season. Clicking on a game will take the user to the game detail page for that game. This will also include the same biographical information as the player detail page.

Table includes the following columns:
- Date: The date of the game.
- Opponent: The opponent team for that game.
- Score: The score of the game.
- For position player: Include shared components statistics table for hitters.
- For pitchers: Include shared components statistics table for pitchers.
- Position(s) played in the game: The position(s) the player played in that game. If the player played multiple positions in a single game, then all positions will be displayed in a comma separated list.

#### Resolved: Position(s) Played Per Game

The per-game log endpoint (`GET /players/{id}/games`) had no `Position` field on `GameBattingLine`/`GamePitchingLine`, even though the resolution logic already existed — `GameSummaryService.ResolvePosition` (starting lineup slot plus every subsequent substitution-recorded position, de-duplicated) was built for Step 7i's full game box score, just never called from the per-player-per-season path.

Fixed by reusing that same static method from `PlayerGameLogService`, called once per game in the player's log (fetching that game's `GameLineup`/`GameSubstitution` rows the same way `GameSummaryService` already does for a box score) rather than duplicating the position logic. Bounded the same way the rest of this endpoint already is — at most ~162 extra lookups for one player-season.

### Franchises Page

Path: /franchises

The Franchises page will display a table of all franchises that ever have existed within Major League Baseball. Each row is the current extant franchise so Washington Nationals is featured not Montreal Expos. There will be a footnote denoting each the former names of the franchise in a comma separated list. Clicking on a franchise name will take the user to the franchise detail page. The table will be sortable on all columns. Table columns:
- Name: Name of the franchise as it is currently known. This will also contain a link to the franchise detail page.
- From: The year the franchise was established from the beginning of the franchise's history.
- Games (Played): The total number of games played by the franchise from its inception to the present.
- Wins: The total number of wins by the franchise from its inception to the present.
- Losses: The total number of losses by the franchise from its inception to the present.
- Win %: The total win percentage of the franchise from its inception to the present.
- Above .500: The total number of seasons the franchise has finished above a .500 win percentage.
- Below .500: The total number of seasons the franchise has finished below a .500 win percentage.

#### Resolved: Franchise All-Time Summary

This page needs every franchise's *all-time* totals in one call — not per-season (already covered by the standings work above) and not per-era (a franchise like the Nationals has two `Franchise` rows, Montreal Expos 1969-2004 and Washington Nationals 2005-present, sharing one `FranchiseIdentifier`), but summed across a lineage's entire history and displayed under its current name only, per this page's own "Washington Nationals featured, not Montreal Expos" rule. Nothing computed this shape before.

Fixed with a new pure [`FranchiseCareerSummaryResolver`](../src/lib/Retrosharp/Format/Standings/FranchiseCareerSummaryResolver.cs): groups every `Franchise` era by `FranchiseIdentifier`, picks the most recent era (by `FranchiseStart`) as the representative row (its city/nickname is `CurrentName`; every earlier era's city/nickname becomes a `FormerNames` entry, oldest first — directly backing this page's footnote), and sums every precomputed `FranchiseSeasonStanding` row across *every* era's `FranchiseId` in that lineage, not just the representative era's. `SeasonsAboveFiveHundred`/`SeasonsBelowFiveHundred` count seasons (again, across every era) with win percentage strictly above/below .500 — a season at exactly .500 counts toward neither, and `FirstSeasonYear` is the lineage's earliest era's start year regardless of how many renames happened since.

Exposed as `GET /teams?limit=&offset=` (new bare browse route on `TeamsController`, alongside the existing `teams/search`) — mirrors the Players page's `GET /players?letter=` browse-vs-search split exactly: one route for "browse everything, paginated," a separate one for free-text search. See [api.md](./api.md#franchise-all-time-summaries-are-computed-from-every-era-in-a-lineage-not-just-one) for the full design.

### Franchise Detail Page

Path: /franchises/[id]

The franchise detail page will display a table with the following columns:
- Year: The year of the season.
- Complete Franchise Name: The name of the franchise during that season which includes the city and nickname (e.g. San Diego Padres).
- League: The league the franchise played in during that season (e.g. American League or National League).
- Division: The division the franchise played in during that season (e.g. AL West, NL East). **NOTE**: Divisions were not created until 1969, so this column will be blank for seasons prior to 1969.
- Wins: The total number of wins by the franchise during that season.
- Losses: The total number of losses by the franchise during that season.
- Win %: The win percentage of the franchise during that season.
- Finish: The final standing of the franchise during that season (e.g. 1st, 2nd, 3rd, etc.). 
	- **NOTE**: For seasons prior to 1969, this column will display as "[position] in [league]" (e.g. 1st in American League, 2nd in National League, etc.) since there were no divisions prior to 1969.
	- **NOTE**: The American League did not exist until 1901. So this will simply display the ordinal position in the league (e.g. 1st, 2nd, 3rd, etc.) for seasons prior to 1901.
	- **NOTE**: The National League did not exist until 1876. So this will simply display the ordinal position in the league (e.g. 1st, 2nd, 3rd, etc.) for seasons prior to 1876.
	- **NOTE**: After 1969, the finish column will display the ordinal position in the division (e.g. "1st in AL West", "2nd in NL East", etc.)
- Divisions: The number of times the franchise won the division during that season. This will be blank for seasons prior to 1969.
- Pennants: The number of times the franchise won the league pennant during that season. Prior to 1969, this will be the number of times the franchise was the best record of the league. After 1969, this will be the number of times the franchise won the league championship series.
- GB (Games Behind): The number of games the franchise was behind the first place team during that season. Before 1969, this will be the number of games behind the best record in the league. After 1969, this will be the number of games behind the first place team in the division.
- Manager: The name of the manager of the franchise during that season.

#### Resolved: Standings Derivation

There was no Wins/Losses/standings concept anywhere in the codebase — not in `Contract`, not in `TeamService`/`TeamStatisticsService`, not in api.md — despite Wins, Losses, Win %, Finish, Divisions, Pennants, and GB all appearing as columns here (and, by reuse, on the Franchises and Season Detail pages too). `TeamStatisticsService` only ever computed batting/pitching/fielding counting stats; nothing counted a franchise's game *outcomes*.

Fixed with a new precomputed `FranchiseSeasonStanding` table (one row per franchise-season: `Wins`/`Losses`/`Ties`/`Rank`/`GamesBehind`/`DivisionChampion`/`LeagueBestRecord`, plus a computed `WinPercentage`), derived by a new pure [`StandingsResolver`](../src/lib/Retrosharp/Format/Standings/StandingsResolver.cs) from that season's already-imported `Game` rows (win = more runs, loss = fewer, tie = equal — ties excluded from the win-percentage denominator, the standard convention). Ranking uses each franchise-season's already-era-resolved `Franchise.LeagueId`/`DivisionCode` (no extra era lookup needed, since `Game.HomeFranchiseId`/`VisitorFranchiseId` already point at the correct era) — grouped by division when `DivisionCode` is populated for that era, by league alone otherwise, matching this page's own before/after-1969 rule. `Rank`/`GamesBehind` reflect that grouping; `LeagueBestRecord` is always computed league-wide regardless of division, independent of `DivisionChampion`.

**This is precomputed, not live-queried** — a `POST /api/standings/compute?season=` endpoint recomputes and atomically replaces one season's rows (idempotent: re-running it after importing more of that season's Game Log data just recomputes from whatever games exist now), matching the "precomputed per season" requirement. It's a plain synchronous recompute, not a saga, since it's a fast in-memory aggregation over already-imported data with no external file and no retryable failure mode — see [api.md](./api.md#standings-are-precomputed-not-live-queried) for the full reasoning.

**Scope boundary — "Pennants" post-1969**: this column's own definition splits at 1969 the same way "Finish" does — pre-1969 pennant meant best regular-season record (`LeagueBestRecord`, fully computable), but post-1969 it meant winning the League Championship Series, which requires postseason data this project doesn't import. This is the exact same gap already resolved for the Seasons page's [League Champion](#resolved-league-champion-is-out-of-scope-as-originally-worded) column — `LeagueBestRecord` should **not** be displayed as "Pennants" for a post-1969 season until postseason data is imported; doing so would silently mislabel a regular-season stat as a postseason result.

This covers a single franchise-season (`GET /teams/{id}/stats` now includes a `Standing` field) and a whole season at once (`GET /seasons/{year}/standings`). The Franchises page's *all-time* Wins/Losses/Win %/Above-.500/Below-.500 columns are covered too, by a separate all-time summary — see [Resolved: Franchise All-Time Summary](#resolved-franchise-all-time-summary) on the Franchises page above.

#### Resolved: Multiple Managers Per Season

The data fully supports mid-season manager changes: Retrosheet's Game Log format records a manager per game, per team, and the schema mirrors this — every `Game` row has its own home/visitor manager reference rather than one manager per team-season. So a mid-season firing shows up as a change in manager partway through that franchise's games for the year.

The Manager column will resolve to one or more managers by grouping that franchise-season's games by manager in chronological order, e.g. "Bruce Bochy (through Jun 15), Mike Shildt (from Jun 16)", falling back to a single name when there was no change during the season.

**Backend dependency**: the team-season statistics endpoint does not currently surface manager data. A new field/endpoint is needed to expose the per-game manager grouping described above before this column can be implemented.

### Franchise Season Detail Page

Path: /franchises/[id]/seasons/[year]

The franchise season detail page will display all the pertient information about the franchise for that season. This includes the franchise's record, standings, and statistics for that season. The page will also display a list of all players who played for the franchise during that season, along with their individual statistics. Clicking on a player's name will take the user to the player detail page for that season. The page will also display a list of all games played by the franchise during that season, along with the scores and outcomes of each game. Clicking on a game will take the user to the game detail page for that game.

There will be two sections on the page. First section is for the position players and the second section is for the pitchers. Each section will have a table with the following columns:
- Player Name: The name of the player. This will also contain a link to the player detail page for that season.
- Games Played: The total number of games played by the player during that season.
- For position players: Use shared components statistics table for hitters, sorted by Games Played (descending). **Phase 1 does not group position players by position** — see Resolved note below.
- For pitchers: Use shared components statistics table for pitchers. Group the top five pitchers that started the most games (the starters). The remaining pitchers will be grouped together in a separate section of the table (the relievers). The relievers will be sorted by the number of games they appeared in during the season. Include a third section highlighting the top three closers for the franchise during that season. The closers will be sorted by the number of saves they recorded during the season.

#### Resolved: Position-Player Grouping Deferred to Phase 2

The original design grouped position players into "the top nine that started the most games at each position," but no data supports this: `Fielding` has no games-played/games-started column at all (only Putouts/Assists/Errors/PassedBalls/DoublePlays/TriplePlays), and `Batting.Positions` is explicitly a scope-excluded placeholder pending the Phase 2 replacement already described in [project.md](./project.md) (Second Phase, item 104) and [game-event.md](./game-event.md#future-enhancement-phase-2-batting-positions-played) — tracking games/innings actually played per position, sourced from `GameLineup`/`GameSubstitution`.

**Decision: scope down for Phase 1, do not pull the Phase 2 tracking forward.** An approximation (for example, ranking by fielding chances) was considered and rejected — it would misrank real players, since a DH accumulates zero putouts/assists despite starting every game, and a backup catcher can rack up more fielding chances per game than an everyday corner outfielder. Rather than ship a grouping that quietly gets some players wrong, Phase 1 lists position players in one flat table sorted by Games Played (using `Batting.GamesPlayed`/`GamesStarted`, both season aggregates already available), with no per-position tiering. The top-nine-by-position grouping is deferred to Phase 2, once real per-position games-started data exists to support it correctly.

### Franchise Games Per Season

Path: /franchises/[id]/seasons/[year]/games

This page displays the games played within a season by the given franchise in a table with the following columns:

- Date
- Start Time — blank for games with no imported play-by-play; see the [Individual Game Played in a Season](#individual-game-played-in-a-season) note on start time's coverage-dependent availability.
- Day/Night
- Game Number
- Home/Away
- Opponent (with link to franchise detail page for that season)
- Score (The current franchise's score is always first. If the opponent won, the score would be 3-7)
- Manager
- Opponent Manager
- Game Detail Link (links to the [Individual Game Played in a Season](#individual-game-played-in-a-season))

### Seasons Page

Path: /seasons

The Seasons page will display a list of all seasons in Major League Baseball history. Each row is one season, with the year (linked to that season's detail page), the number of games played, and the number of teams that season. Clicking on a season will take the user to the season detail page for that season.

#### Resolved: "League Champion" Is Out of Scope as Originally Worded

The original column list called for a single "league champion" per season, but that's ambiguous, and the unambiguous readings don't line up with what Retrosharp currently imports:

- If it means **pennant winner**, there are two per season (one per league), not one — already modeled by the Franchise Detail page's `Pennants` column (best regular-season record pre-1969, LCS winner after), and derivable purely from regular-season standings, which Retrosharp already has.
- If it means **World Series champion** (the single-team reading "champion" usually implies), that requires postseason game results, which aren't part of Retrosheet's regular-season Game Logs and aren't imported by any parser in this project — there is no "postseason" or "champion" concept anywhere in the current schema.

For Phase 1, this column is dropped from both the Seasons page and Season Detail page. If a single per-season champion is wanted later, it depends on importing Retrosheet's separate postseason game log files, which is new ETL scope, not a frontend gap.

### Season Detail Page

Path: /seasons/[year]

The season detail page will display all the pertinent information about the season, including the number of games played and the number of teams. The page will also display a list of all franchises that participated in that season, along with their records and standings (reusing the same Year/League/Division/Wins/Losses/Win %/Finish/Divisions/Pennants/GB/Manager columns already defined on the [Franchise Detail Page](#franchise-detail-page), for that one year across every franchise instead of every year for one franchise). Clicking on a franchise will take the user to that franchise's season detail page. There will be two tables below the standings, one for hitting and one for pitching, each with one row per team that played that season.

Hitters:
- Team (with link to franchise detail page for that season)
- Average batter's age
- Runs per game (R/G)
- Shared components statistics table for hitters.

Pitchers:
- Team (with link to franchise detail page for that season)
- Average pitcher's age
- Runs allowed per game (RA/G)
- Shared components statistics table for pitchers.

#### Resolved: "Average Age" as of June 30

A team's roster turns over all season (call-ups, trades, retirements), so "average batter's/pitcher's age" needs a fixed as-of date to mean anything consistent across players and across teams. **Decision: age as of June 30 of that season** — the standard Baseball-Reference "baseball age" convention (already this project's own named design reference), chosen because June 30 is roughly the midpoint of a 162-game season, so it's the single date that best represents how old a player was for most of that season. Computed as `SeasonYear - BirthYear`, minus 1 if the player's birthday falls after June 30 that year. The age is fixed for the whole season regardless of when the player's actual birthday falls — no mid-season change.

The data to compute it already exists (`Person.BirthDate`, imported by the biofile); this only needed the convention decided, not new data. A team's average batter's/pitcher's age for the season is the mean of this fixed per-player age across every position player/pitcher who appeared for that team that season.

#### Resolved: Batched Team Stats for a Season

`GET /seasons/{year}/standings` already returned every team's *standings* for a season in one call, but this page also needs every team's *batting/pitching statistics* for the season in one call — the only existing route, `GET /teams/{franchiseId}/stats?season=`, is one franchise at a time, which would mean a 20-30-call fan-out from the frontend to populate this page's two tables.

Fixed with `GET /seasons/{year}/teams/stats`, a new batched endpoint (`ITeamStatisticsService.GetSeasonSummariesAsync`) that returns both tables — every participating franchise's batting summary and every participating franchise's pitching summary — in one response. Each row also carries this page's other two per-team columns: Runs/Runs-Allowed Per Game (team runs ÷ games played, tallied directly from `Game` so this doesn't depend on standings having been computed first) and the average age decided above (`Person.BirthDate` resolved per distinct batter/pitcher on that team-season, using the new pure [`BaseballAge.ComputeAge`](../src/lib/Retrosharp/Format/BaseballAge.cs) helper, 0 when none of them have a known birth date).

### Season's Games Played

Path: /seasons/[year]/games

This page displays all the games played in a single MLB season.

This page will display:

- Total games played (usually 162, but tie-breaker 163rd games have occured, also COVID/strike shortened seasons)
- Longest game played by innings
- Longest game played by time
- Most lopsided game
- Number of batters played
- Number of pitchers played

The following table will display below the above information with the following columns:

- Date
- Start Time — blank for games with no imported play-by-play; see the [Individual Game Played in a Season](#individual-game-played-in-a-season) note on start time's coverage-dependent availability.
- Day/Night
- Game Number
- Visitor Team (with link to their franchise season detail page)
- Visitor Score
- Home Team (with link to their franchise season detail page)
- Home Score
- Visitor Manager
- Home Manager
- Game Detail Link

**NOTE**: A MLB season has a total of 2,430 games played in a 162 regular season. Lazy load the table with pagination of user selectable 25, 50, or 100 rows per page to reduce the amount of data

### Individual Game Played in a Season

Path: /seasons/[year]/games/[id]

This page displays the full box score for a single game: game information, line score, starting lineups and pitchers, a batting and pitching box score for every player who appeared (not just starters), and play-by-play. Modeled after a [Baseball Reference box score](https://www.baseball-reference.com/boxes/SDN/SDN202606260.shtml), scoped to data Retrosharp actually has — Baseball Reference's own derived metrics (WPA, RE24, aLI, cWPA, and similar) are out of scope.

#### Game Information

- Date, day of week, game number (single game, or 1st/2nd of a doubleheader).
- Venue: ballpark name and city.
- Day/Night.
- Attendance.
- Length of game (elapsed time).
- Umpires (up to 5 positions: home plate, 1B, 2B, 3B, and — for earlier eras — left/right field).
- Winning pitcher, losing pitcher, saving pitcher, and game-winning RBI batter, each linked to their player detail page.

**Start time / end time — capturable, but not yet implemented, and coverage-dependent:** Retrosheet's Game Log format (the source for everything else on this page) has no wall-clock start or end time — only total elapsed minutes and a Day/Night indicator. However, Retrosheet's play-by-play event files *do* carry it, as an `info,starttime,7:44PM`-style record. Retrosharp's Game Event Parser currently reads every `info` record but only extracts `hometeam`/`visteam`/`date`/`number` from it (`EventFileReader.ApplyInfo`) — `starttime` is parsed off the file and silently discarded today. This is a real, fixable gap, not a fundamental one, but it inherits play-by-play's own coverage limitation: it will only ever be populated for games whose team-season event file has been imported, same as the [Play by Play](#play-by-play) section below. "Time game ended" isn't a separate stored field either way — once start time is captured, it's a trivial derived display value (start time + `GameLengthMinutes`, already available). See the game-event.md/api.md/build-plan cross-references below for the proposed fix.

Team logos are deliberately out of scope for Phase 1 and this prototype — see [project.md](./project.md)'s Second Phase list. Nothing in this page's design should assume a logo is available.

#### Line Score

A standard line score: one row per team, one column per inning, plus R/H/E totals.

- Data exists today (`Game.VisitorLineScore`/`HomeLineScore`, inning-by-inning run strings, plus `VisitorRuns`/`HomeTeamRuns`, `VisitorHits`/`HomeHits`, `VisitorErrors`/`HomeErrors`), but isn't yet exposed by the API.
- **Backend dependency**: `GameSummaryResponse` currently returns each team's R/H/E totals but not the inning-by-inning line score string. Needs the two line-score fields added to the response — no new derivation, just exposing already-stored columns.

#### Starting Lineups and Starting Pitchers

- Batting order (1-9), player name (linked to that player's season detail page), and defensive position — already available via `GameSummaryResponse`'s `HomeLineup`/`VisitorLineup`.
- Starting pitcher for each team.
  - **Backend dependency**: Retrosheet's Game Log format records each team's starting pitcher explicitly (separate from the batting lineup, since a DH-era starting pitcher never appears in the batting order at all). Retrosharp's Game Log Parser already reads this field into its raw import object, but it is currently discarded — never persisted onto `Game`, never exposed via the API. Needs `VisitorStartingPitcherId`/`HomeStartingPitcherId` added to `Game` and surfaced on `GameSummaryResponse`, rather than relying on batting-lineup position "P" (which silently fails to identify the starter in any DH-era game).

#### Batting Box Score

One row per player who batted for either team — starters and substitutes alike, not just the starting lineup — with standard counting stats: PA, AB, R, H, 2B, 3B, HR, RBI, BB, IBB, SO, HBP, SH, SF, SB, CS, GIDP. Each row also shows the defensive position(s) the player appeared at.

- **Backend dependency**: this is the same shape and derivation Step 7d already built for a player's per-game log (`GameBattingLine`) — grouping `GameEvent`/`GameEventRunner` rows by batter within one game — just grouped the opposite way (every batter for one `GameId`, instead of every game for one batter). No endpoint currently returns this; it needs a new query grouped by `GameId` rather than by `PersonId`.
- The underlying `GameSubstitution` data (`BattingOrderPosition`, `FieldingPosition`, `PersonId`, `TeamAtBat`) is sufficient to show which substitute replaced which starter in a given lineup slot, if a Baseball-Reference-style grouping ("pinch hit for so-and-so") is wanted later — not required for the initial prototype.

#### Pitching Box Score

One row per pitcher who appeared for either team — starters and relievers — with standard counting stats: IP (display-friendly, e.g. "6.1", not decimal), H, R, ER, BB, IBB, SO, HR allowed, HBP, BK. The pitcher(s) credited with the win, loss, or save are flagged using the game's already-available `WinningPitcherId`/`LosingPitcherId`/`SavingPitcherId`.

- **Backend dependency**: same situation as the batting box score — this is `GamePitchingLine`'s existing shape (already built for Step 7d's per-game player log), needing the same "all participants for one `GameId`" grouping instead of "all games for one player."
- Per-player fielding detail (putouts/assists) is intentionally out of scope for this box score — team fielding totals are already available via `GameSummaryResponse`'s existing team box score.

#### Play by Play

The full, chronologically-ordered sequence of everything that happened in the game, already built by Step 7f's `GET /games/{gameId}/events` and returned via `GamePlayByPlayResponse`. For each entry, in true file order (via `RecordIndex`, not any individual record type's own independent `Sequence` counter):

- **Plays**: inning, home/visitor at bat, batter, pitcher, ball/strike count, pitch sequence, the play's raw text and classified event type, batted-ball type, sacrifice hit/fly flags, and every baserunner's start/end base, whether they were put out, drove in a run, or scored an earned run, plus fielding credits (with relay sequence, e.g. an assist before a putout).
- **Substitutions**: who entered, for which team, at which batting-order slot and defensive position.
- **Adjustments**: batting/pitching-hand changes and lineup-responsibility adjustments.
- **Comments**: free-text notes (Phase 1 also stores ejection records here verbatim rather than as structured data — see [game-event.md](./game-event.md#future-enhancement-phase-2-gameejection)).

Every person referenced anywhere in the play-by-play (batters, pitchers, runners, fielders, substitutes) is resolved once via the response's `People` glossary rather than repeated per occurrence.

**Data completeness note**: play-by-play depends on that specific team-season's event file having been imported, which is a much smaller set of games than Game Log coverage (Game Logs exist far further back, and for more seasons overall, than imported play-by-play). A game with no imported play-by-play already returns a valid, empty result (`{ people: {}, events: [] }`) rather than an error — this page should render the rest of the box score normally and show a plain "Play-by-play not available for this game" message in this section rather than treating it as a failure.

### Games

There will be no "Games" page with path `/games`. In the 150 or so years of professional baseball in the United States, there have been upwards of 200,000+ games played. This is a massive amount of data to return from the backend and display on the front end even with pagination and row limits.

## Reference Designs

Examine the designs of these pages to get a better understanding of how to design the frontend of Retrosharp. These designs are not perfect, but they provide a good starting point for the prototyping phase.

- [Baseball Reference](https://www.baseball-reference.com/)
- [FanGraphs](https://www.fangraphs.com/)