import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { PlayerSearchResult } from '../../model/player-search-result.model';
import { PlayerService } from '../../service/player.service';
import { formatAge, formatHeight, formatPlace } from '../../util/format.util';

const ALPHABET = Array.from({ length: 26 }, (_, i) => String.fromCharCode(65 + i));

const DISPLAYED_COLUMNS = [
  'name',
  'birthDate',
  'deathDate',
  'age',
  'birthPlace',
  'deathPlace',
  'bats',
  'throws',
  'height',
  'weight',
  'playerDebutDate',
  'playerLastDate',
];

@Component({
  selector: 'app-players',
  templateUrl: './players.html',
  styleUrl: './players.css',
  imports: [
    RouterLink,
    DatePipe,
    MatButtonModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
})
export class Players implements OnInit {
  private readonly service = inject(PlayerService);

  readonly alphabet = ALPHABET;
  readonly pageSizeOptions = [25, 50, 100];
  readonly displayedColumns = DISPLAYED_COLUMNS;

  readonly selectedLetter = signal<string | null>(null);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.pageSizeOptions[0]);

  readonly players = signal<PlayerSearchResult[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  selectLetter(letter: string | null): void {
    if (this.selectedLetter() === letter) {
      return;
    }

    this.selectedLetter.set(letter);
    this.pageIndex.set(0);
    this.load();
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  // Spec: "UseName Surname" -- e.g. "Babe Ruth" (UseName "Babe", Surname "Ruth").
  displayName(player: PlayerSearchResult): string {
    if (player.useName && player.surname) {
      return `${player.useName} ${player.surname}`;
    }

    return player.useName ?? player.surname ?? player.fullName ?? player.retroSheetId;
  }

  // Not a live active/retired flag -- Retrosheet has no current-roster feed, only a per-player
  // "last game" date that lags reality (e.g. a retirement announced since the last data update).
  // Null here means only "no final game on record", which is the most this data can honestly claim.
  hasNoFinalGame(player: PlayerSearchResult): boolean {
    return player.playerLastDate == null;
  }

  // Per spec: a null PlayerLastDate only implies "still active" for someone who
  // has a recorded debut -- otherwise there's no evidence they ever played, let
  // alone recently (some early-era Person records have neither a debut nor a
  // last-played date at all). A populated DeathDate always means retired, even
  // when PlayerLastDate wasn't captured -- a dead player can't still be active
  // regardless of which field is the gap.
  private isActive(player: PlayerSearchResult): boolean {
    return player.playerDebutDate != null && player.playerLastDate == null && player.deathDate == null;
  }

  isDeceased(player: PlayerSearchResult): boolean {
    return player.deathDate != null;
  }

  // Baseball-Reference convention: current age for an active/living player, age
  // at death when known. A retired player with no recorded death date is
  // ambiguous -- early-era biofile records frequently omit it, and there's no
  // way to tell whether they died shortly after their last game or decades
  // later -- so that case reports "N/A" rather than guessing an "as of" date,
  // same as an unknown birth date.
  age(player: PlayerSearchResult): number | 'N/A' {
    if (player.birthDate == null) {
      return 'N/A';
    }

    if (player.deathDate) {
      return formatAge(player.birthDate, new Date(player.deathDate)) ?? 'N/A';
    }

    if (this.isActive(player)) {
      return formatAge(player.birthDate, new Date()) ?? 'N/A';
    }

    return 'N/A';
  }

  birthPlace(player: PlayerSearchResult): string | null {
    return formatPlace(player.birthCity, player.birthStateProvince, player.birthCountry);
  }

  deathPlace(player: PlayerSearchResult): string | null {
    return formatPlace(player.deathCity, player.deathStateProvince, player.deathCountry);
  }

  height(player: PlayerSearchResult): string | null {
    return formatHeight(player.height);
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.service
      .browseAsync(this.selectedLetter(), this.pageSize(), this.pageIndex() * this.pageSize())
      .subscribe({
        next: (result) => {
          this.players.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: (e) => {
          console.error(e);
          this.players.set([]);
          this.totalCount.set(0);
          this.error.set('Unable to load players. Please try again later.');
          this.loading.set(false);
        },
      });
  }
}
