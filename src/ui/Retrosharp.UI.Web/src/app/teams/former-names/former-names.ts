import { Component, OnInit, inject, signal } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { LoadingOverlay } from '../../shared/loading-overlay/loading-overlay';
import { PageNumberPaginator } from '../../shared/page-number-paginator/page-number-paginator';
import { FormerFranchiseName } from '../../../model/former-franchise-name.model';
import { FranchiseService } from '../../../service/franchise.service';
import { formatGamesOverFiveHundred } from '../../../util/format.util';

const NO_RECORD = '—';

const DISPLAYED_COLUMNS = ['name', 'years', 'became', 'games', 'wins', 'losses', 'ties', 'winPercentage', 'gamesOverFiveHundred'];

@Component({
  selector: 'app-former-names',
  templateUrl: './former-names.html',
  styleUrl: './former-names.css',
  imports: [MatProgressSpinnerModule, MatTableModule, LoadingOverlay, PageNumberPaginator],
})
export class FormerNames implements OnInit {
  private readonly service = inject(FranchiseService);

  // 50 fits all 48 former names on one page; see spec/former-franchise-names.md.
  readonly pageSizeOptions = [25, 50, 100];
  readonly displayedColumns = DISPLAYED_COLUMNS;

  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.pageSizeOptions[1]);

  readonly formerNames = signal<FormerFranchiseName[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly initialLoad = signal(true);

  ngOnInit(): void {
    this.load();
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  years(name: FormerFranchiseName): string {
    return name.firstSeasonYear === name.lastSeasonYear
      ? `${name.firstSeasonYear}`
      : `${name.firstSeasonYear}–${name.lastSeasonYear}`;
  }

  // A name with no games on record has no computed standings yet (see POST
  // /api/standings/compute); show a dash rather than a misleading 0.
  stat(name: FormerFranchiseName, value: number): string {
    return name.gamesPlayed > 0 ? `${value}` : NO_RECORD;
  }

  // Wins minus losses, signed ("+18" / "-42" / "0").
  gamesOverFiveHundred(name: FormerFranchiseName): string {
    return name.gamesPlayed > 0 ? formatGamesOverFiveHundred(name.wins, name.losses) : NO_RECORD;
  }

  // Conventional baseball format: ".500", no leading zero.
  winPercentage(name: FormerFranchiseName): string {
    return name.gamesPlayed > 0 ? name.winPercentage.toFixed(3).replace(/^0/, '') : NO_RECORD;
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.service.getFormerNamesAsync(this.pageSize(), this.pageIndex() * this.pageSize()).subscribe({
      next: (result) => {
        this.formerNames.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
        this.initialLoad.set(false);
      },
      error: (e) => {
        console.error(e);
        this.formerNames.set([]);
        this.totalCount.set(0);
        this.error.set('Unable to load former names. Please try again later.');
        this.loading.set(false);
        this.initialLoad.set(false);
      },
    });
  }
}
