import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { PagedResult } from '../model/paged-result.model';
import { PlayerSearchResult } from '../model/player-search-result.model';
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
}
