import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { FormerFranchiseName } from '../model/former-franchise-name.model';
import { FranchiseCareerSummary } from '../model/franchise-career-summary.model';
import { PagedResult } from '../model/paged-result.model';
import { HttpService } from './http.service';

@Service()
export class FranchiseService {
  private readonly http: HttpService = inject(HttpService);
  private readonly baseUrl = `${environment.apiBaseUrl}/teams`;

  /**
   * Gets one all-time career summary per franchise lineage. See GET /api/teams in api.md.
   */
  getAllTimeAsync(limit: number, offset: number): Observable<PagedResult<FranchiseCareerSummary>> {
    return this.http.getAsync<PagedResult<FranchiseCareerSummary>>(`${this.baseUrl}?${this.pageQuery(limit, offset)}`);
  }

  /**
   * Gets every former name of every franchise lineage, most recently used first. Backs the Teams
   * page's "Former names" table. See spec/former-franchise-names.md.
   */
  getFormerNamesAsync(limit: number, offset: number): Observable<PagedResult<FormerFranchiseName>> {
    return this.http.getAsync<PagedResult<FormerFranchiseName>>(`${this.baseUrl}/former-names?${this.pageQuery(limit, offset)}`);
  }

  private pageQuery(limit: number, offset: number): string {
    const params = new URLSearchParams();
    params.set('limit', limit.toString());
    params.set('offset', offset.toString());
    return params.toString();
  }
}
