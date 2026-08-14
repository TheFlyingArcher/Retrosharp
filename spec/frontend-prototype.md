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

#### Open Design Question

A single text search bar on the page will leave a lot of space on the page. Can other baseball content be filled on the page such as latest news items from MLB if a feed exists? Or perhaps a list of the most popular players, franchises, seasons, and games? This is an open design question that will be explored during the prototyping phase.

### Players Page

Path: /players

On this page, users will be able to view a list of baseball players grouped by the player's last name. To reduce the amount of data coming through only the player's last name and first name will be displayed in the list. Clicking on a player's name will take the user to the player detail page. There is also on top of the page the alphabet A-Z in which a user can click a letter and it drops them down to the players whose last name starts with that letter. The list of players will be paginated to improve performance and usability. Bold text will indicate the player is currently active in MLB. A cross next to their name will indicate the player is deceased.

#### Resolved: Determining "Is Active"

There is no explicit `IsActive` field within the underlying data store. `PlayerLastDate` (sourced from the Retrosheet biofile's `last_p` column) is nullable, and a player is considered active when it is `null`. A player can't have a debut without also having some most-recent game, so a null last-game date for a person with a populated debut can only mean their most recent appearance hasn't been finalized as their last one, i.e. they are still active. Retrosheet's own biofile documentation doesn't explicitly guarantee this interpretation, but it's the only logically consistent one.

Note: the biofile is imported independently of the Game Log/Event pipeline, so `PlayerLastDate` could in principle lag behind the latest season already loaded elsewhere in the database. If that staleness becomes a practical problem, the fallback is to derive "active" from whether the player has any `Batting`/`Pitching`/`Fielding` row in the most recent season year present in the database instead of trusting this field.

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
- GB (Games Behind): The number of games the franchise was behind the first place team during that season. Before 1960, this will be the number of games behind the best record in the league. After 1960, this will be the number of games behind the first place team in the division.
- Manager: The name of the manager of the franchise during that season.

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
- For position players: Use shared components statistics table for hitters. Group the top nine players that started the most games at each position. The remaining players will be grouped together in a separate section of the table.
- For pitchers: Use shared components statistics table for pitchers. Group the top five pitchers that started the most games (the starters). The remaining pitchers will be grouped together in a separate section of the table (the relievers). The relievers will be sorted by the number of games they appeared in during the season. Include a third section highlighting the top three closers for the franchise during that season. The closers will be sorted by the number of saves they recorded during the season.

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

**Open Design Question — "average age" as of what date?** A team's roster turns over all season (call-ups, trades, retirements), so "average batter's/pitcher's age" needs an as-of date to mean anything consistent, and nothing in this project has defined one yet. The data to compute it exists (`Person.BirthDate`, already imported by the biofile), but the convention doesn't. The common baseball-reference convention is age as of June 30 of that season; recommend adopting that unless there's a reason not to.

**Backend dependency**: there is currently no endpoint that returns every team's stats for a given season in one call — only `GET /teams/{franchiseId}/stats?season=`, one franchise at a time. Populating this page's two tables (one row per team, ~20-30 teams depending on era) would mean either an N-call fan-out from the frontend or a new batched endpoint (e.g. `GET /seasons/{year}/teams/stats`). Flagging this now since it's the same shape of gap as the manager/game-summary items already filed as build-plan steps — not filing it yet unless you want it added.

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