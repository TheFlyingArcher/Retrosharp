# Game Event Parser

## Overview

The Game Event Parser is responsible for parsing out Retrosheet's play-by-play event files. These event files contain detailed information about each play that occurred during a baseball game, including the players involved, the type of play, and the outcome of the play. The Game Event Parser will extract this information and populate the `GameEvent` table in the Retrosharp database.

## Play-by-Play Format

Retrosheet's play-by-play format is a text-based format that uses a series of codes to represent different types of plays and events. The format can be [viewed](https://www.retrosheet.org/eventfile.htm) on Retrosheet. The Game Event Parser will need to interpret these codes and extract the relevant information for each play.

## Considerations

Retrosheet has play-by-play information that dates back to baseball's earliest years. As such, some of the information may be incomplete or in incompatible formats. The Game Event Parser will need to handle these cases appropriately, ensuring that the data is accurately represented in the database.

The format of the game event files is `TTTYYYY.EVN` for National League and `TTTYYYY.EVA` for American League, where `TTT` is the three-letter team abbreviation, `YYYY` is the year of the game, and `.EVN` or `.EVA` indicates the league. For example `SDN2025.EVN` is the San Diego Padres National League game event file for the year 2025. Because each file is broken down by team and year, there will be "duplicate" games. For example, the July 12, 2025 game between the San Diego Padres and Philadelphia Phillies will be in both the `SDN2025.EVN` and `PHI2025.EVN` files. The Game Event Parser will need to handle these duplicate games appropriately, ensuring that each game is only represented once in the database.

## Data Model

A single play can involve a variable number of participants in variable roles — a routine fly out involves one fielder, while a double play can involve three or more, and a single with runners on base can credit an RBI to the batter once per runner who scores. A single wide `GameEvent` row with a fixed set of columns cannot represent this. Instead, player involvement is normalized into supporting tables, and non-play context (substitutions, adjustments, commentary) is modeled separately from `GameEvent` entirely, since it has a different shape and doesn't feed `Batting`/`Pitching`/`Fielding` derivation.

### `GameEvent`

One row per play, in chronological order within the game.

| Column | Description |
|---|---|
| `GameEventId` | Primary key |
| `GameId` | Foreign key to `Game` |
| `Sequence` | Order of this event within the game, used to reconstruct chronology |
| `Inning` | Inning number |
| `TeamAtBat` | Visiting or home |
| `BatterId` | Foreign key to `Person` |
| `PitcherId` | Foreign key to `Person`, denormalized from the current lineup/substitution state at the time of the play |
| `Balls` | Count of balls in the plate appearance |
| `Strikes` | Count of strikes in the plate appearance |
| `FoulBallsWithTwoStrikes` | Count of foul balls hit while the batter already had two strikes |
| `PitchSequence` | Raw Retrosheet pitch sequence string |
| `RawEventText` | Raw Retrosheet play code string, preserved for traceability back to the source data |
| `EventType` | Categorized event type (single, double, triple, home run, walk, intentional walk, hit by pitch, strikeout, ground out, fly out, error, fielder's choice, stolen base, caught stealing, wild pitch, passed ball, balk, pickoff, etc.) |
| `BattedBallType` | The trajectory of a batted ball (ground ball, line drive, fly ball, pop up), populated from Retrosheet's trajectory modifier codes. Null when the play did not involve a batted ball in play, such as a walk or strikeout. |
| `IsSacHit` | Whether the play was a sacrifice hit |
| `IsSacFly` | Whether the play was a sacrifice fly |

`BattedBallType` is tracked independently of `EventType`, since `EventType` alone conflates trajectory with outcome — a `FlyOut` implies a fly ball that was caught, but a `HomeRun` doesn't otherwise indicate whether it was a fly ball, line drive, or something else. This separation is what makes a statistic like HR/FB (home runs per fly ball allowed) possible: it requires knowing every fly ball a pitcher allowed, regardless of whether that particular one was caught, dropped, or hit for a home run.

Retrosheet does not provide batted-ball contact quality (soft/medium/hard contact) — that is a Statcast-era concept from MLB's own tracking system, introduced in 2015, and is not part of Retrosheet's data for any era. No corresponding column is included in `GameEvent` for this reason.

### `GameEventRunner`

One row per person affected as a baserunner by the play, including the batter — a batter reaching base is simply a runner whose `StartBase` is the batter's box, so no separate "batter outcome" concept is needed.

| Column | Description |
|---|---|
| `GameEventRunnerId` | Primary key |
| `GameEventId` | Foreign key to `GameEvent` |
| `PersonId` | Foreign key to `Person` |
| `StartBase` | Base the runner started the play on (0 = batter's box, 1, 2, or 3) |
| `EndBase` | Base the runner ended the play on (1, 2, 3, Home, or Out) |
| `IsOut` | Whether the runner was put out on this play |
| `IsRBI` | Whether this runner's advance to home is credited as an RBI to the batter |
| `IsEarnedRun` | Whether this runner's run, if scored, is earned |
| `ResponsiblePitcherId` | Foreign key to `Person`, the pitcher charged with this runner if they score (accounts for inherited runners) |

`IsRBI` should be sourced directly from Retrosheet's own `(RBI)`/`(NORBI)`/`(NR)` play-code annotations rather than independently derived by the parser applying the official rules of baseball itself (for example, the rule that a batter is not credited with an RBI when grounding into a double play). Retrosheet's record is treated as authoritative even where it may not strictly reflect the official scoring rules in effect at the time — this is especially relevant for early-era data, where rules were still being finalized. The goal is to capture what was officially recorded as having happened, not to recompute what should have happened under the rules.

Example — runners on second and third, batter hits a single scoring both: one `GameEvent` row (`EventType = Single`). Three `GameEventRunner` rows: the batter (`StartBase = 0`, `EndBase = 1`), the runner from third (`StartBase = 3`, `EndBase = Home`, `IsRBI = true`), and the runner from second (`StartBase = 2`, `EndBase = Home`, `IsRBI = true`). A player's season RBI total is simply the count of `IsRBI = true` rows across all `GameEvent` rows where they were the batter.

### `GameEventFieldingCredit`

One row per fielder action, linked to the specific runner it affected.

| Column | Description |
|---|---|
| `GameEventFieldingCreditId` | Primary key |
| `GameEventId` | Foreign key to `GameEvent` |
| `GameEventRunnerId` | Foreign key to `GameEventRunner`, identifying which runner this fielding action pertains to |
| `PersonId` | Foreign key to `Person`, the fielder |
| `CreditType` | Putout, Assist, or Error |
| `Sequence` | Order of this fielder's involvement in a relay (for example, an assist before a putout) |
| `Position` | The fielding position (1-9) the person was playing at the moment of this credit, denormalized from the current lineup/substitution state at the time of the play, using the same mechanism as `PitcherId` on `GameEvent`. This allows `Fielding` to be tracked separately per position for a player who changes position within a season, or even within a single game. |

A `GameEventFieldingCredit` row only exists when it is tied to an actual out or error on the specific runner referenced by `GameEventRunnerId`. A fielder's throw that does not result in either — for example, a rundown where the runner escapes safely with no misplay — is not recorded, since no assist is credited without an accompanying putout or error.

The same person can be credited more than once on a single play, for different runners. Examples:

- **5-3 ground out**: one `GameEvent` row (`EventType = GroundOut`). One `GameEventRunner` row for the batter (`StartBase = 0`, `EndBase = Out`, `IsOut = true`). Two `GameEventFieldingCredit` rows, both referencing that runner row: the third baseman (`CreditType = Assist`, `Sequence = 1`) and the first baseman (`CreditType = Putout`, `Sequence = 2`).
- **6-4-3 double play**: two `GameEventRunner` rows — the runner forced at second and the batter out at first. Four `GameEventFieldingCredit` rows: the shortstop (`Assist`, `Sequence = 1`) and second baseman (`Putout`, `Sequence = 2`) both linked to the forced runner's row; the second baseman (`Assist`, `Sequence = 1`) and first baseman (`Putout`, `Sequence = 2`) both linked to the batter's row. The second baseman is credited twice on the same play — a putout on one runner and an assist on the other.
- **8-6-2 relay throw doubling off a runner tagging from third on a sacrifice fly**: two `GameEventRunner` rows — the batter (out via the catch) and the runner thrown out at home. The center fielder is credited with a `Putout` (`Sequence = 1`) on the batter's row, and separately an `Assist` (`Sequence = 1`) on the runner's row; the shortstop is credited with an `Assist` (`Sequence = 2`) on the runner's row; the catcher is credited with the final `Putout` (`Sequence = 3`) on the runner's row.
- **Rundown ("pickle")**: however many fielders touch the ball while chasing down the runner, that many `Assist` rows in increasing `Sequence`, ending in a single `Putout` row, all linked to that runner's `GameEventRunner` row. A fielder may appear more than once (for example, the catcher throws to start the rundown, then receives the ball back and makes the final tag).

### `Fielding`

`Fielding` is aggregated from `GameEventFieldingCredit` and keyed by `(PersonId, FranchiseId, SeasonYear, Position)` rather than just `(PersonId, FranchiseId, SeasonYear)`. Because baseball positions are fluid — a player may change primary position across seasons, or even within a single game — a player accumulates separate `Fielding` rows per position actually played, rather than one row per season covering all positions combined.

### `GameEventGameStatus`

Records that a game's statistics have been fully applied to `Batting`, `Pitching`, and `Fielding`, and is the mechanism used to prevent two concurrent sagas from double-applying the same shared game (see [Considerations](#considerations)). This table is owned entirely by the Game Event Parser — `Game` remains the exclusive domain of the Game Log Parser and is never written to by the Game Event Parser.

| Column | Description |
|---|---|
| `GameId` | Primary key, foreign key to `Game` |
| `ProcessedUtc` | Timestamp when this game's statistics were successfully applied |

Before applying a game's events to `Batting`, `Pitching`, and `Fielding`, a saga attempts to insert a row into `GameEventGameStatus` for that `GameId`, within the same transaction as the stat updates. Because `GameId` is the primary key, only one saga can successfully insert this row for a given game; a saga that loses this race detects the uniqueness violation, treats the game as already processed, and continues to the next game in the file without reapplying its statistics.

### Context tables

Substitutions, handedness/batting-order adjustments, and commentary are not plays — they have no fielding or baserunning outcome and are not derived into `Batting`, `Pitching`, or `Fielding`. They are modeled as siblings of `GameEvent`, not sub-tables of it:

- `GameSubstitution`: a player entering the game mid-game (position player substitution, pinch hitter, or pinch runner). Columns: `GameId`, `Sequence`, `PersonId`, `TeamAtBat`, `BattingOrderPosition`, `FieldingPosition`.
- `GameAdjustment`: the less common adjustment records (`badj`, `padj`, `ladj`, `radj`, `presadj`), using a single table with an `AdjustmentType` column since these are infrequent and don't need the same level of normalization as `GameEvent`. Columns: `GameId`, `Sequence`, `AdjustmentType`, `PersonId`, `Value`.
- `GameComment`: free-text commentary (`com`) records. Columns: `GameId`, `Sequence`, `CommentText`. Events without a dedicated Retrosheet record type, such as an ejection, would be captured here for narrative context rather than as a structured event.

## Prerequisites

1. The `Person` table needs to be populated with player, umpire, manager, and coach information before the Game Event Parser can be run. This is because the Game Event Parser will reference the `Person` table to associate players with their respective statistics and game events.
1. The `League` table needs to be populated with league information before the Game Event Parser can be run. This is because the Game Event Parser will reference the `League` table to associate games with their respective leagues and seasons.
1. The `Franchise` table needs to be populated with franchise information before the Game Event Parser can be run. This is because the Game Event Parser will reference the `Franchise` table to associate games with their respective franchises and teams.
1. Since `GameEvent` will derive from `Game`, the `Game` table needs to be populated with game information before the Game Event Parser can be run. This is because the Game Event Parser will reference the `Game` table to associate game events with their respective games. If a Game Event file references a game that is not yet present in the `Game` table (for example, because the corresponding Game Log file has not yet been processed), this is a retryable condition and should be handled using the parser's standard retry/backoff policy rather than treated as a fatal error.

## Requirements

1. The Game Event Parser should be able to successfully read and process Retrosheet's play-by-play event files, extracting relevant information and populating the `GameEvent` table in the Retrosharp database.
1. The `GameEvent` table derives from the `Game` table, so the Game Event Parser should ensure that each game event is associated with the correct game in the `Game` table.
1. The `GameEvent` table and its supporting tables (`GameEventRunner`, `GameEventFieldingCredit`) should follow the [Data Model](#data-model) section below, normalizing player involvement rather than using a fixed set of columns, since a single play may involve a variable number of runners and fielders. `GameEvent` should include the raw Retrosheet play-by-play event string for each play, so that the original data can be referenced if needed.
1. The `GameEvent` table should track plate appearance pitch counts using three simple aggregate columns: total balls, total strikes, and total foul balls. A "foul ball" for this purpose is a ball hit foul while the batter already has two strikes. Pitch-by-pitch detail (a separate row per individual pitch) is not required.
1. Substitutions and other non-play context (handedness/batting-order adjustments, commentary) should be recorded using the context tables (`GameSubstitution`, `GameAdjustment`, `GameComment`) described in the [Data Model](#data-model) section, separate from `GameEvent`, since they do not represent plays and are not derived into `Batting`, `Pitching`, or `Fielding`.
1. Data idempotency. See the [Considerations](#considerations) section above for more information about duplicate games. The Game Event Parser should ensure that the same game event is not duplicated or inserted multiple times in the `GameEvent` table.
1. Because of the complexity of the play-by-play event files, the Game Event Parser should be designed to handle errors gracefully, logging any issues encountered during processing and continuing to process the remaining events in the file.
1. Game Event files may be processed concurrently with one another. Because the same physical game can appear in two different team files (see [Considerations](#considerations)), the Game Event Parser must guard against two sagas concurrently applying the same game's statistics. Before a saga applies a game's events to the `Batting`, `Pitching`, and `Fielding` tables, it must atomically claim the game by inserting a row into `GameEventGameStatus` (see [Data Model](#data-model)) within the same transaction as the stat updates. If that insert fails because the game has already been claimed by another saga, the saga should treat that game as complete and continue to the next game in the file without reapplying its statistics. `GameEventGameStatus` is owned entirely by the Game Event Parser and is never written to by the Game Log Parser, preserving `Game` as the exclusive domain of the Game Log Parser. File-level or global serialization must not be used as the mechanism for preventing this, as it would prevent multiple Game Event files from being processed simultaneously.
1. If a parse is running, each queued message for the same event file should be ignored. This is to ensure that the same event file is not processed multiple times, which could lead to duplicate records in the `GameEvent` table.
1. Messages shall remain queued while a parse is running. This is to ensure that messages are not lost or discarded while a parse is in progress, allowing for reliable processing of all incoming messages.
1. An API endpoint exists to place a message on the service bus to initiate the processing of a game event file. This allows for external systems or users to trigger the processing of game event files as needed.
1. When each game is finished within the parse, and only after the atomic idempotency check described above confirms the game has not already been applied, the tables `Batting`, `Pitching`, and `Fielding` should be updated with the latest statistics for each player based on the game events that were processed, before the next game is parsed. This ensures that the player statistics are always up-to-date and accurate without being double-counted.
1. `Pitching.EarnedRuns` should be sourced from Retrosheet's `data` records for each game, which contain Retrosheet's official post-game earned-run total per pitcher, rather than computed solely from play-by-play error and unearned-run modifiers. The Game Event Parser should still compute earned runs independently from the play-by-play as a validation check; if the computed value disagrees with the corresponding `data` record value, this should be logged as a data-quality warning (game, pitcher, both values) without altering the stored value sourced from the `data` record.
1. After processing the games in a team-season file, the Game Event Parser should compare its own derived team-level aggregate statistics (hits, runs, errors, and other totals covered by both datasets) against the corresponding `GameBattingStatistics`, `GamePitchingStatistics`, and `GameFieldingStatistics` records populated by the Game Log Parser. `Game*Statistics` values are authoritative — they were computed by the Game Log Parser and must never be overwritten by the Game Event Parser, even when the Game Event Parser's own derived totals disagree. Any discrepancy should be logged as a data-quality warning (game, team, stat, both values) for manual review.
1. Each game is delineated by `id,TTTYYYYMMDDX` where `id` is a keyword, `TTT` is the three letter team abbreviation, `YYYYMMDD` is the date of the game, and `X` is a number indicating the game number for that day. For example, `id,SDN202507121` is the first game of the day on July 12, 2025 between the San Diego Padres and Philadelphia Phillies. `0` indicates a single game played, `1` indicates the first game of a doubleheader and `2` indicates the second game of a doubleheader. The `id` record starts the description of a game thus ending the description of the preceding game in the file. The Game Event Parser should use this information to correctly associate each game event with the appropriate game in the `Game` table.

## Acceptance Criteria

1. A saga is created to process Retrosheet's game event files and stores them in the `GameEvent` table in the Retrosharp database.
1. The saga is idempotent, ensuring that the same game event is not duplicated or inserted multiple times in the `GameEvent` table. This is distinct from file-level idempotency (below) — this concerns record-level dedup within a single parse.
1. The saga receives a message off the service bus and begins processing the game event file.
1. The saga maintains atomicity of the parse. If an unrecoverable error occurs during processing, the database should not be left in an inconsistent state. No partial parses!
1. The saga provides detailed logging of each step of the parse for data transparency, traceability, and ease of debugging.
1. Retry/backoff behavior, file-level idempotency (reprocessing the same datafile does not create duplicate entries), and end-of-parse added/updated record logging follow the shared base requirements in [parser.md](parser.md).
1. Multiple Game Event files can be processed concurrently, including two files that both contain the same shared game, without double-counting that game's statistics in the `Batting`, `Pitching`, or `Fielding` tables. This is enforced via an atomic claim on `GameEventGameStatus`, not file-level serialization, and `Game` is never written to by the Game Event Parser.
1. `Pitching.EarnedRuns` is sourced from Retrosheet's `data` records, with a logged warning whenever the parser's independently computed earned-run value disagrees with the `data` record value.
1. Team-level aggregate statistics derived from play-by-play are compared against the corresponding `Game*Statistics` records from the Game Log Parser at the end of processing a team-season file, with any discrepancy logged as a warning and no values in `Game*Statistics` overwritten.
1. The parser is a NServiceBus saga that processes the datafile in a background task. Review the [NServiceBus documentation](https://docs.particular.net/nservicebus/sagas/) for more information on how to implement a saga. Further review [parser](parser.md) for more information on how to implement a parser and acceptance criteria.
