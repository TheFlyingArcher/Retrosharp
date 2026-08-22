/**
 * One pitching statistics row (or a combined total across rows). Mirrors `PitchingLine` in
 * Retrosharp.UI.Api.
 *
 * Spec gap: spec/frontend-prototype.md's pitcher column list also calls for Wins, Losses,
 * Home Runs Allowed (raw count), Batters Faced, and Hits Per Nine Innings — none of these are
 * aggregated onto the backend `Pitching` season row (Wins/Losses only exist per-game, via
 * `Game.WinningPitcherId`/`LosingPitcherId`; HR-allowed/H9/BF aren't stored anywhere at the
 * season-aggregate level). Not modeled here until that backend work exists.
 */
export interface PitchingLine {
  franchiseCode: string | null;
  franchiseName: string | null;
  seasonYear: number | null;
  gamesPitched: number;
  gamesStarted: number;
  gamesFinished: number;
  completeGames: number;
  shutouts: number;
  saves: number;
  inningsPitchedDisplay: string;
  hits: number;
  runs: number;
  earnedRuns: number;
  baseOnBalls: number;
  strikeouts: number;
  intentionalBb: number;
  hitBatsmen: number;
  balks: number;
  wildPitches: number;
  era: number;
  whip: number;
  strikeoutsPerNine: number;
  walksPerNine: number;
  homeRunsPerNine: number;
  homeRunsPerFlyBall: number;
  fip: number;
  battingAverageOnBallsInPlay: number;
  fipConstant: number | null;
  fipConstantLeagueCode: string | null;
  fipConstantSeasonYear: number | null;
}
