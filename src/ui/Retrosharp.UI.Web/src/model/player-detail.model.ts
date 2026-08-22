/** Player identity/biographical detail. Mirrors `PlayerDetail` in Retrosharp.UI.Api. */
export interface PlayerDetail {
  id: number;
  retroSheetId: string;
  surname: string | null;
  useName: string | null;
  fullName: string | null;
  birthDate: string | null;
  birthCity: string | null;
  birthStateProvince: string | null;
  birthCountry: string | null;
  deathDate: string | null;
  deathCity: string | null;
  deathStateProvince: string | null;
  deathCountry: string | null;
  cemetery: string | null;
  cemeteryCity: string | null;
  cemeteryStateProv: string | null;
  cemeteryCountry: string | null;
  cemeteryNote: string | null;
  bats: string | null;
  throws: string | null;
  /** Height in inches. */
  height: number | null;
  /** Weight in pounds. */
  weight: number | null;
  isHof: boolean;
  playerDebutDate: string | null;
  playerLastDate: string | null;
  managerDebutDate: string | null;
  managerLastDate: string | null;
  coachDebutDate: string | null;
  coachLastDate: string | null;
  umpireDebutDate: string | null;
  umpireLastDate: string | null;
}
