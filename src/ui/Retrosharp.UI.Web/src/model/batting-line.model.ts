/**
 * One batting statistics row (or a combined total across rows, in which case
 * `franchiseCode`/`franchiseName`/`seasonYear` are null, since a combined total may span more
 * than one franchise/season). Mirrors `BattingLine` in Retrosharp.UI.Api.
 */
export interface BattingLine {
  franchiseCode: string | null;
  franchiseName: string | null;
  seasonYear: number | null;
  plateAppearances: number;
  atBats: number;
  hits: number;
  doubles: number;
  triples: number;
  homeruns: number;
  baseOnBalls: number;
  strikeouts: number;
  sacrificeFlies: number;
  sacrificeBunts: number;
  intentionalBb: number;
  hitByPitches: number;
  stolenBases: number;
  timesCaughtStealing: number;
  runs: number;
  groundedIntoDoublePlay: number;
  runsBattedIn: number;
  gamesPlayed: number;
  gamesStarted: number;
  totalBases: number;
  battingAverage: number;
  onBasePercentage: number;
  sluggingPercentage: number;
  onBasePlusSlugging: number;
  battingAverageOnBallsInPlay: number;
  isolatedPower: number;
}
