import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { BattingLine } from '../model/batting-line.model';
import { PagedResult } from '../model/paged-result.model';
import { PitchingLine } from '../model/pitching-line.model';
import { PlayerDetail } from '../model/player-detail.model';
import { PlayerSearchResult } from '../model/player-search-result.model';
import { PlayerStatsResponse } from '../model/player-stats-response.model';
import { HttpService } from './http.service';

@Service()
export class PlayerService {
  private readonly http: HttpService = inject(HttpService);
  private readonly baseUrl = `${environment.apiBaseUrl}/players`;

  /**
   * Browses players ordered by surname, optionally restricted to surnames starting with
   * `letter`. Backs the Players page's A-Z browse list. See GET /api/players in api.md.
   */
  browseAsync(letter: string | null, limit: number, offset: number): Observable<PagedResult<PlayerSearchResult>> {
    const params = new URLSearchParams();
    params.set('limit', limit.toString());
    params.set('offset', offset.toString());
    if (letter) {
      params.set('letter', letter);
    }

    return this.http.getAsync<PagedResult<PlayerSearchResult>>(`${this.baseUrl}?${params.toString()}`);
  }

  /**
   * Gets a player's identity/biographical detail. Backs the Player Detail page.
   */
  getByIdAsync(id: number): Observable<PlayerDetail> {
    return this.http.getAsync<PlayerDetail>(`${this.baseUrl}/${id}`);
  }

  /**
   * Gets a player's batting statistics for one season, or their whole career if `season` is
   * omitted.
   */
  getBattingAsync(id: number, season?: number): Observable<PlayerStatsResponse<BattingLine>> {
    return this.http.getAsync<PlayerStatsResponse<BattingLine>>(`${this.baseUrl}/${id}/batting${this.seasonQuery(season)}`);
  }

  /**
   * Gets a player's pitching statistics for one season, or their whole career if `season` is
   * omitted.
   */
  getPitchingAsync(id: number, season?: number): Observable<PlayerStatsResponse<PitchingLine>> {
    return this.http.getAsync<PlayerStatsResponse<PitchingLine>>(`${this.baseUrl}/${id}/pitching${this.seasonQuery(season)}`);
  }

  private seasonQuery(season?: number): string {
    return season != null ? `?season=${season}` : '';
  }
}
