/**
 * One franchise lineage's all-time record. See GET /api/teams.
 */
export interface FranchiseCareerSummary {
  franchiseId: number;
  franchiseCode: string;
  currentName: string;
  formerNames: string[];
  firstSeasonYear: number;
  gamesPlayed: number;
  wins: number;
  losses: number;
  ties: number;
  winPercentage: number;
  seasonsAboveFiveHundred: number;
  seasonsBelowFiveHundred: number;
}
