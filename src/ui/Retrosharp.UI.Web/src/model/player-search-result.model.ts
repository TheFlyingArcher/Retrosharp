export interface PlayerSearchResult {
  /** The unique identifier for the player. */
  id: string;

  /** The unique identifier for the player in the Retrosheet database. */
  retrosheetId: string;

  /** The full name of the player. */
  fullName: string | null;

  /** The name to use for the player. */
  useName: string | null;

  /** The batting hand of the player. */
  bats: string | null;

  /** The throwing hand of the player. */
  throws: string | null;

  /** The date when the player debuted. */
  playerDebutDate: Date | null;

  /** The date when the player last played. */
  playerLastDate: Date | null;

  /** Indicates if the player is in the Hall of Fame. */
  isHof: boolean;
}
