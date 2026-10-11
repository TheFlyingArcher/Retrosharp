# Former Franchise Names — Endpoint Design (Draft)

Status: **draft, not implemented.** Written 2026-10-03 as a design note for the Teams page's second table ("Former names", below the 30 current franchises). Like the rest of `spec/`, treat this as a learning artifact — the code is the source of truth once it exists.

Supersedes the earlier "Inactive Franchises" draft, which was renamed and reframed (see [Why "Former names"](#why-former-names-not-inactive)).

## Problem

The Teams page lists the 30 current lineages via `GET /teams` (see [api.md](./api.md#franchise-all-time-summaries-are-computed-from-every-era-in-a-lineage-not-just-one)), each under its current name, with earlier names only as a footnote. The human wants a second table listing the earlier names themselves — Montreal Expos, Washington Senators, St. Louis Browns, Oakland Athletics, and so on — each with its years, its record under that name, and the current franchise it became.

## Decision: list every former name

An earlier draft selected only names where the franchise had both moved *and* rebranded (so the Athletics' Oakland years and the Cleveland Indians were excluded). The human decided against that: any selection rule is an editorial judgment that Retrosheet doesn't make, and the table is simpler and easier to defend without one. **Every earlier name of every lineage is listed**, including same-city renames (Cleveland Indians, Cincinnati Redlegs) and same-nickname moves (Brooklyn Dodgers, Oakland Athletics). No rule, no override file.

## Why "Former names", not "Inactive"

Retrosheet and Baseball-Reference both fold the Expos into the Washington Nationals' lineage: one entry, one set of records, shared history (the Nationals even keep Expos-era retired numbers). Calling the Expos "inactive" or "defunct" would assert something the project's own sources do not. This matches how the project already treats players: the Players page says "no final game on record," never "active" (see [frontend-prototype.md](./frontend-prototype.md#resolved-determining-is-active--reframed-as-no-final-game-on-record)).

So the table states a neutral fact — *this name was used from X to Y, and the franchise is now the Washington Nationals* — and leaves "gone" as the reader's judgment. Even leagues disagree about who owns history after a move (the NFL's Browns/Ravens case split history from the legal entity); Retrosheet follows the entity, and so does this table.

## What the data can and cannot say

- `Franchise` has **125 rows, but they are eras** (one row per city/nickname/division stretch), not franchises. They belong to **30 lineages** (`FranchiseIdentifier`); all 30 are current (`FranchiseEnd == null`, see `SeedDataService`).
- `franchises.csv` is derived from Retrosheet's `CurrentNames.csv`, so it contains **only former names of today's 30 franchises**. Franchises with no current successor (e.g. Cleveland Spiders) are not in it. See [Out of scope](#out-of-scope-phase-1).
- Every era in the CSV has an end date except the 30 current ones; the only gaps between eras of one lineage are real history (CHN 1871→1874, SLN 1891→1892). No missing end dates.

## Definition

A *name* is a distinct (`FranchiseLocation`, `Nickname`) pair within one lineage. Eras that differ only by division, league or `FranchiseCode` share a name (CAL/ANA Angels era rows collapse), and a name that recurs after a gap is **one** name, not two (Boston Braves 1912–35 and 1941–52 around the Boston Bees years; Brooklyn Superbas 1899–1910 and 1913 around the Brooklyn Dodgers' 1911–12 stint, which itself recurs as 1932–57). Its years span the first era's start to the last era's end, and its record sums every era of that name.

The lineage's **current name** is its most recent era's pair. A **former name** is any other name in the lineage. Counting this way, `franchises.csv` yields **48 former names across 21 of the 30 lineages** (the other nine have only ever had one name). Largest: Boston/Milwaukee Braves lineage (8 former names), Brooklyn/Los Angeles Dodgers (6), Cleveland (4).

Use `FranchiseLocation` ("Tampa Bay"), not `PlayingCity` ("St. Petersburg"), as the location half of the pair — the Devil Rays → Rays pair and the Rays' current name both read "Tampa Bay," as the CSV intends, even though the games were played in St. Petersburg (and, in 2025, Tampa).

## Why not reuse `GET /teams`

`FranchiseCareerSummaryResolver` collapses every era into one row per lineage, which is the opposite of what this table needs, and `GET /teams` is documented as lineage-per-row. Adding a filter would change the meaning and DTO shape of an existing endpoint. A separate route mirrors the existing browse-vs-search split.

## Endpoint

`GET /api/teams/former-names?limit=&offset=` on `TeamsController`.

- Same validation as `GET /teams`: `limit` 1–100 (default 25), `offset` ≥ 0, else `400`.
- Literal segment `former-names` wins over `{id}` in endpoint routing, same as `search`; declare it before `{id}`.
- Order: `LastSeasonYear` descending, then `Name` (Expos 2004 first). Open question below.
- Response: `PagedResult<FormerFranchiseNameLine>`. At 48 rows the default `limit` yields two pages; the Teams page should either request `limit=50` or keep the paginator.

```csharp
public class FormerFranchiseNameLine
{
    public int FranchiseId { get; set; }          // this name's most recent era's Franchise.Id -> GET /teams/{id}
    public string FranchiseCode { get; set; }     // of that era (e.g. "MON")
    public string Name { get; set; }              // "Montreal Expos" ("FranchiseLocation Nickname")
    public short FirstSeasonYear { get; set; }    // first era's start year
    public short LastSeasonYear { get; set; }     // last era's end year (a recurring name may have gaps between)
    public int CurrentFranchiseId { get; set; }   // lineage's current representative era -> link target
    public string CurrentName { get; set; }       // e.g. "Washington Nationals"
    public short GamesPlayed { get; set; }
    public short Wins { get; set; }
    public short Losses { get; set; }
    public short Ties { get; set; }
    public float WinPercentage { get; set; }
}
```

## Implementation sketch

Mirror `FranchiseCareerSummaryResolver` / `TeamService.GetAllTimeSummariesAsync`:

1. New pure `FormerFranchiseNameResolver.Resolve(eras, standings)` in `Retrosharp/Format/Standings/` — group eras by `FranchiseIdentifier`, identify the current name (latest `FranchiseStart`), group the remaining eras by (`FranchiseLocation`, `Nickname`), then compute span and record per group.
2. **Records need no new game aggregation:** `FranchiseSeasonStanding` rows are keyed by era `FranchiseId`, so a name's record is the sum of the standings rows across *its* eras — the same operation the all-time summary does across a whole lineage, scoped one level down.
3. `ITeamService.GetFormerNamesAsync(limit, offset)` + `TeamService` implementation (load all eras and standings, resolve, order, page in memory — same scale reasoning as `GetAllTimeSummariesAsync`).
4. `FormerFranchiseNameLine` DTO in `Retrosharp.UI.Api/Models/`, Mapster mapping, controller action.
5. Frontend: implement the empty `FranchiseService` (`getAllTime`, `getFormerNames`), second table on the Teams page reusing the shared components. Suggested columns: Name (links to its era), Years (first–last), Became (current name, links), W, L, T, W-L%.

## Out of scope (Phase 1)

**Franchises with no current successor** (Cleveland Spiders, Troy Trojans, Buffalo Bisons, Federal League clubs and the like). These are the only franchises that are defensibly "defunct," but they are not in `franchises.csv`, so listing them needs a new data source. Retrosheet's franchise directory (https://www.retrosheet.org/boxesetc/MISC/FRDIR.htm) *appears* to include some, but that has not been analyzed: whether it is machine-readable, whether its franchise definitions match `CurrentNames.csv`, and whether game data for those clubs is imported. Deferred to a later phase pending that analysis.

## Caveats

- Records cover only seasons with computed `FranchiseSeasonStanding` rows, same caveat as the all-time summaries. Check what has been imported before presenting the Expos' record as complete; a name whose seasons were never imported would show zero games.
- Retrosheet lags one season, but this table is almost entirely historical, so the lag matters less than it does for players.
- [api.md](./api.md#franchise-all-time-summaries-are-computed-from-every-era-in-a-lineage-not-just-one) says "~100 franchise lineages"; the actual figure is 30 lineages / 125 eras. At the default `limit` of 25 the current-franchise table spans two pages — the Teams page should either raise its page size to fit 30 or keep the paginator.
- Listing every name means the table includes 19th-century names (Boston Red Stockings, Brooklyn Bridegrooms, and so on). That is deliberate; a reader can sort or page past them.

## Tests

Resolver unit tests: Expos listed with `CurrentName` "Washington Nationals"; Athletics lineage lists Philadelphia, Kansas City and Oakland; Indians → Guardians and Devil Rays → Rays each list their old name; both Washington Senators names listed under their own lineages; Boston Braves around the Bees years collapses to one name with the right span and summed record; CAL/ANA Angels era rows collapse and the current "Los Angeles Angels" name is not listed; a lineage that never changed names yields no rows; records sum across a name's eras only.

## Open Questions

1. Order: last season descending (proposed) vs alphabetical (matches the current-franchise table) vs grouped by current franchise.
2. Whether the table should offer a way to filter or group by current franchise, since 48 rows spread across 21 lineages.

## Update (issue #16): "Became" is the immediate successor

The original design stamped the lineage's current name on every row, which misrepresented multi-step lineages (California Angels -> Anaheim Angels -> ... -> Los Angeles Angels). `CurrentName`/`CurrentFranchiseId` were replaced by `NextName`/`NextFranchiseId`: the name of the era directly after a name's last era. For a recurring name (Boston Braves) the successor follows its last era.
