import { Component, OnInit, inject, signal } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { FranchiseCareerSummary } from '../../../model/franchise-career-summary.model';
import { FranchiseService } from '../../../service/franchise.service';
import { formatGamesOverFiveHundred } from '../../../util/format.util';

const NO_RECORD = '—';

const DISPLAYED_COLUMNS = ['name', 'since', 'games', 'wins', 'losses', 'winPercentage', 'gamesOverFiveHundred'];

// The API caps limit at 100; there are only 30 current franchises, so one request gets them all.
const ALL_FRANCHISES_LIMIT = 100;

@Component({
  selector: 'app-current-franchises',
  templateUrl: './current-franchises.html',
  styleUrl: './current-franchises.css',
  imports: [MatProgressSpinnerModule, MatTableModule],
})
export class CurrentFranchises implements OnInit {
  private readonly service = inject(FranchiseService);

  readonly displayedColumns = DISPLAYED_COLUMNS;

  readonly franchises = signal<FranchiseCareerSummary[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  // A franchise with no games on record has no computed standings yet (see POST
  // /api/standings/compute); show a dash rather than a misleading 0.
  stat(franchise: FranchiseCareerSummary, value: number): string {
    return franchise.gamesPlayed > 0 ? `${value}` : NO_RECORD;
  }

  // Wins minus losses, signed ("+18" / "-42" / "0").
  gamesOverFiveHundred(franchise: FranchiseCareerSummary): string {
    return franchise.gamesPlayed > 0 ? formatGamesOverFiveHundred(franchise.wins, franchise.losses) : NO_RECORD;
  }

  // Conventional baseball format: ".500", no leading zero.
  winPercentage(franchise: FranchiseCareerSummary): string {
    return franchise.gamesPlayed > 0 ? franchise.winPercentage.toFixed(3).replace(/^0/, '') : NO_RECORD;
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.service.getAllTimeAsync(ALL_FRANCHISES_LIMIT, 0).subscribe({
      next: (result) => {
        this.franchises.set(result.items);
        this.loading.set(false);
      },
      error: (e) => {
        console.error(e);
        this.franchises.set([]);
        this.error.set('Unable to load franchises. Please try again later.');
        this.loading.set(false);
      },
    });
  }
}
