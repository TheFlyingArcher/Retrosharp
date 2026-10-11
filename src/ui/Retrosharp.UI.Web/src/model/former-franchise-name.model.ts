/**
 * One former name of a franchise lineage with its span and record under that name. See
 * GET /api/teams/former-names and spec/former-franchise-names.md.
 */
export interface FormerFranchiseName {
  /** Most recent era's franchise id for this name; resolves GET /api/teams/{id}. */
  franchiseId: number;
  franchiseCode: string;
  /** "Location Nickname", e.g. "Montreal Expos". */
  name: string;
  firstSeasonYear: number;
  lastSeasonYear: number;
  /** Franchise id of the era that directly followed this name, for the "Became" link. */
  nextFranchiseId: number;
  nextName: string;
  gamesPlayed: number;
  wins: number;
  losses: number;
  ties: number;
  winPercentage: number;
}
