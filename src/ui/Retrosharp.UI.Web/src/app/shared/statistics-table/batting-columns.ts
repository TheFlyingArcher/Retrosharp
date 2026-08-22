import { BattingLine } from '../../../model/batting-line.model';
import { StatColumn } from './stat-column.model';

function fixed(value: number, digits: number): string {
  return value.toFixed(digits);
}

/** Batting average/OBP/SLG/OPS follow the baseball convention of no leading zero, e.g. ".300". */
function average(value: number): string {
  const formatted = fixed(value, 3);
  return value < 0 ? formatted : formatted.replace(/^0\./, '.');
}

/**
 * Hitters' column set for the shared statistics table, per spec/frontend-prototype.md's
 * "Statistics Tables" column list. Backed entirely by `BattingLine`, no gaps.
 */
export const BATTING_COLUMNS: StatColumn<BattingLine>[] = [
  { key: 'year', header: 'Year', value: (r) => r.seasonYear ?? '', sortValue: (r) => r.seasonYear ?? -1 },
  { key: 'team', header: 'Team(s)', value: (r) => r.franchiseName ?? '' },
  { key: 'g', header: 'G', tooltip: 'Games Played', value: (r) => r.gamesPlayed },
  { key: 'gs', header: 'GS', tooltip: 'Games Started', value: (r) => r.gamesStarted },
  { key: 'ab', header: 'AB', tooltip: 'At Bats', value: (r) => r.atBats },
  { key: 'r', header: 'R', tooltip: 'Runs', value: (r) => r.runs },
  { key: 'h', header: 'H', tooltip: 'Hits', value: (r) => r.hits },
  { key: '2b', header: '2B', tooltip: 'Doubles', value: (r) => r.doubles },
  { key: '3b', header: '3B', tooltip: 'Triples', value: (r) => r.triples },
  { key: 'hr', header: 'HR', tooltip: 'Home Runs', value: (r) => r.homeruns },
  { key: 'rbi', header: 'RBI', tooltip: 'Runs Batted In', value: (r) => r.runsBattedIn },
  { key: 'sb', header: 'SB', tooltip: 'Stolen Bases', value: (r) => r.stolenBases },
  { key: 'cs', header: 'CS', tooltip: 'Caught Stealing', value: (r) => r.timesCaughtStealing },
  { key: 'k', header: 'K', tooltip: 'Strikeouts', value: (r) => r.strikeouts },
  { key: 'bb', header: 'BB', tooltip: 'Walks', value: (r) => r.baseOnBalls },
  { key: 'ibb', header: 'IBB', tooltip: 'Intentional Walks', value: (r) => r.intentionalBb },
  { key: 'avg', header: 'AVG', tooltip: 'Batting Average', value: (r) => average(r.battingAverage), sortValue: (r) => r.battingAverage },
  { key: 'obp', header: 'OBP', tooltip: 'On Base Percentage', value: (r) => average(r.onBasePercentage), sortValue: (r) => r.onBasePercentage },
  { key: 'slg', header: 'SLG', tooltip: 'Slugging Percentage', value: (r) => average(r.sluggingPercentage), sortValue: (r) => r.sluggingPercentage },
  { key: 'ops', header: 'OPS', tooltip: 'On Base Plus Slugging', value: (r) => average(r.onBasePlusSlugging), sortValue: (r) => r.onBasePlusSlugging },
  { key: 'hbp', header: 'HBP', tooltip: 'Hit By Pitch', value: (r) => r.hitByPitches },
  { key: 'sh', header: 'SH', tooltip: 'Sacrifice Hits', value: (r) => r.sacrificeBunts },
  { key: 'sf', header: 'SF', tooltip: 'Sacrifice Flies', value: (r) => r.sacrificeFlies },
  { key: 'tb', header: 'TB', tooltip: 'Total Bases', value: (r) => r.totalBases },
  { key: 'gidp', header: 'GIDP', tooltip: 'Grounded Into Double Plays', value: (r) => r.groundedIntoDoublePlay },
];
