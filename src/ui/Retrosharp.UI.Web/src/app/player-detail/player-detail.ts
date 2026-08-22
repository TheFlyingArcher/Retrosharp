import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BATTING_COLUMNS } from '../shared/statistics-table/batting-columns';
import { PITCHING_COLUMNS } from '../shared/statistics-table/pitching-columns';
import { StatisticsTable } from '../shared/statistics-table/statistics-table';
import { BattingLine } from '../../model/batting-line.model';
import { PitchingLine } from '../../model/pitching-line.model';
import { PlayerDetail as PlayerDetailModel } from '../../model/player-detail.model';
import { PlayerService } from '../../service/player.service';

/** Formats a height stored as total inches (see Person.Height) as `[ft]' [in]"`. */
export function formatHeight(heightInInches: number | null): string {
  if (heightInInches == null) {
    return '';
  }
  const feet = Math.floor(heightInInches / 12);
  const inches = Math.round(heightInInches % 12);
  return `${feet}' ${inches}"`;
}

/** Formats a city/state/country triple as "City, State, Country", dropping any missing parts. */
export function formatPlace(city: string | null, state: string | null, country: string | null): string {
  return [city, state, country].filter((part) => !!part).join(', ');
}

@Component({
  selector: 'app-player-detail',
  standalone: true,
  templateUrl: './player-detail.html',
  styleUrl: './player-detail.css',
  imports: [DatePipe, MatProgressSpinnerModule, StatisticsTable],
})
export class PlayerDetail implements OnInit {
  private readonly service = inject(PlayerService);

  readonly id = input.required<string>();
  private readonly personId = computed(() => Number(this.id()));

  readonly BATTING_COLUMNS = BATTING_COLUMNS;
  readonly PITCHING_COLUMNS = PITCHING_COLUMNS;

  readonly player = signal<PlayerDetailModel | null>(null);
  readonly battingRows = signal<BattingLine[]>([]);
  readonly battingTotal = signal<BattingLine | null>(null);
  readonly pitchingRows = signal<PitchingLine[]>([]);
  readonly pitchingTotal = signal<PitchingLine | null>(null);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly displayName = computed(() => {
    const player = this.player();
    return player ? (player.useName ?? player.fullName ?? player.retroSheetId) : '';
  });

  readonly height = computed(() => formatHeight(this.player()?.height ?? null));

  readonly birthPlace = computed(() => {
    const player = this.player();
    return player ? formatPlace(player.birthCity, player.birthStateProvince, player.birthCountry) : '';
  });

  readonly deathPlace = computed(() => {
    const player = this.player();
    return player ? formatPlace(player.deathCity, player.deathStateProvince, player.deathCountry) : '';
  });

  readonly isDeceased = computed(() => this.player()?.deathDate != null);

  // Not a live active/retired flag -- see Players.hasNoFinalGame() for why null only means
  // "no final game on record", not "currently active".
  readonly hasNoFinalGame = computed(() => this.player()?.playerLastDate == null);

  readonly burialLocation = computed(() => {
    const player = this.player();
    if (!player) {
      return '';
    }
    const place = formatPlace(player.cemeteryCity, player.cemeteryStateProv, player.cemeteryCountry);
    return [player.cemetery, place].filter((part) => !!part).join(', ');
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    const id = this.personId();
    this.loading.set(true);
    this.error.set(null);

    forkJoin({
      player: this.service.getByIdAsync(id),
      batting: this.service.getBattingAsync(id),
      pitching: this.service.getPitchingAsync(id),
    }).subscribe({
      next: ({ player, batting, pitching }) => {
        this.player.set(player);
        this.battingRows.set(batting.rows);
        this.battingTotal.set(batting.combinedTotal);
        this.pitchingRows.set(pitching.rows);
        this.pitchingTotal.set(pitching.combinedTotal);
        this.loading.set(false);
      },
      error: (e: unknown) => {
        console.error(e);
        this.player.set(null);
        this.error.set(
          e instanceof HttpErrorResponse && e.status === 404
            ? 'Player not found.'
            : 'Unable to load player. Please try again later.',
        );
        this.loading.set(false);
      },
    });
  }
}
