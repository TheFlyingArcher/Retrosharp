import { PitchingLine } from '../../../model/pitching-line.model';
import { StatColumn } from './stat-column.model';

function fixed(value: number, digits: number): string {
  return value.toFixed(digits);
}

/**
 * Pitchers' column set for the shared statistics table, per spec/frontend-prototype.md's
 * "Statistics Tables" column list.
 *
 * Spec gap: Wins (W), Losses (L), Home Runs Allowed (HR), Batters Faced (BF), and Hits Per Nine
 * Innings (H9) are in the spec's column list but have no backing field on `PitchingLine` (see
 * that model's doc comment) — omitted here rather than displaying a fake/zero value. Add them
 * back once the backend aggregates those onto the season `Pitching` row.
 */
export const PITCHING_COLUMNS: StatColumn<PitchingLine>[] = [
  { key: 'year', header: 'Year', value: (r) => r.seasonYear ?? '', sortValue: (r) => r.seasonYear ?? -1 },
  { key: 'team', header: 'Team(s)', value: (r) => r.franchiseName ?? '' },
  { key: 'g', header: 'G', tooltip: 'Games Pitched', value: (r) => r.gamesPitched },
  { key: 'gs', header: 'GS', tooltip: 'Games Started', value: (r) => r.gamesStarted },
  { key: 'ip', header: 'IP', tooltip: 'Innings Pitched', value: (r) => r.inningsPitchedDisplay, sortValue: (r) => Number(r.inningsPitchedDisplay) },
  { key: 'sv', header: 'SV', tooltip: 'Saves', value: (r) => r.saves },
  { key: 'h', header: 'H', tooltip: 'Hits Allowed', value: (r) => r.hits },
  { key: 'r', header: 'R', tooltip: 'Runs Allowed', value: (r) => r.runs },
  { key: 'er', header: 'ER', tooltip: 'Earned Runs Allowed', value: (r) => r.earnedRuns },
  { key: 'k', header: 'K', tooltip: 'Strikeouts', value: (r) => r.strikeouts },
  { key: 'bb', header: 'BB', tooltip: 'Walks', value: (r) => r.baseOnBalls },
  { key: 'ibb', header: 'IBB', tooltip: 'Intentional Walks', value: (r) => r.intentionalBb },
  { key: 'hbp', header: 'HBP', tooltip: 'Hit Batsmen', value: (r) => r.hitBatsmen },
  { key: 'bk', header: 'BK', tooltip: 'Balks', value: (r) => r.balks },
  { key: 'era', header: 'ERA', tooltip: 'Earned Run Average', value: (r) => fixed(r.era, 2), sortValue: (r) => r.era },
  { key: 'bb9', header: 'BB9', tooltip: 'Walks Per Nine Innings', value: (r) => fixed(r.walksPerNine, 2), sortValue: (r) => r.walksPerNine },
  { key: 'k9', header: 'K9', tooltip: 'Strikeouts Per Nine Innings', value: (r) => fixed(r.strikeoutsPerNine, 2), sortValue: (r) => r.strikeoutsPerNine },
  { key: 'whip', header: 'WHIP', tooltip: 'Walks and Hits Per Inning Pitched', value: (r) => fixed(r.whip, 2), sortValue: (r) => r.whip },
  { key: 'hr9', header: 'HR9', tooltip: 'Home Runs Per Nine Innings', value: (r) => fixed(r.homeRunsPerNine, 2), sortValue: (r) => r.homeRunsPerNine },
];
