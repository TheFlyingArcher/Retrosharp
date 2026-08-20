export interface PlayerSearchResult {
  /** The unique identifier for the player. */
  id: number;

  /** The unique identifier for the player in the Retrosheet database. */
  retroSheetId: string;

  /** The player's surname (last name), used for the A-Z browse list. */
  surname: string | null;

  /** The full name of the player. */
  fullName: string | null;

  /** The name to use for the player. */
  useName: string | null;

  /** The player's date of birth, as an ISO 8601 date string. */
  birthDate: string | null;

  /** The city the player was born in. */
  birthCity: string | null;

  /** The state/province the player was born in, if applicable. */
  birthStateProvince: string | null;

  /** The country the player was born in. */
  birthCountry: string | null;

  /** The city the player died in. */
  deathCity: string | null;

  /** The state/province the player died in, if applicable. */
  deathStateProvince: string | null;

  /** The country the player died in. */
  deathCountry: string | null;

  /** The batting hand of the player. */
  bats: string | null;

  /** The throwing hand of the player. */
  throws: string | null;

  /** The player's height, in total inches. */
  height: number | null;

  /** The player's weight, in pounds. */
  weight: number | null;

  /** The date when the player debuted, as an ISO 8601 date string. */
  playerDebutDate: string | null;

  /** The date when the player last played, as an ISO 8601 date string. A player with no last-played date is still active. */
  playerLastDate: string | null;

  /** The player's date of death, as an ISO 8601 date string. Non-null indicates the player is deceased. */
  deathDate: string | null;

  /** Indicates if the player is in the Hall of Fame. */
  isHof: boolean;
}
