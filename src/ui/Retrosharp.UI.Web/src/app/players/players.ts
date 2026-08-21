import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PlayerSearchResult } from '../../model/player-search-result.model';
import { PlayerService } from '../../service/player.service';

const ALPHABET = Array.from({ length: 26 }, (_, i) => String.fromCharCode(65 + i));

interface PlayerGroup {
  letter: string;
  players: PlayerSearchResult[];
}

@Component({
  selector: 'app-players',
  templateUrl: './players.html',
  styleUrl: './players.css',
  imports: [RouterLink, MatButtonModule, MatPaginatorModule, MatProgressSpinnerModule],
})
export class Players implements OnInit {
  private readonly service = inject(PlayerService);

  readonly alphabet = ALPHABET;
  readonly pageSizeOptions = [25, 50, 100];

  readonly selectedLetter = signal<string | null>(null);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.pageSizeOptions[0]);

  readonly players = signal<PlayerSearchResult[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  // Players arrive already ordered by surname, so consecutive same-first-letter runs on the
  // current page can just be grouped in place rather than re-sorted.
  readonly groups = computed<PlayerGroup[]>(() => {
    const groups: PlayerGroup[] = [];
    for (const player of this.players()) {
      const letter = (player.surname ?? this.displayName(player) ?? '?').charAt(0).toUpperCase();
      const currentGroup = groups.at(-1);
      if (currentGroup && currentGroup.letter === letter) {
        currentGroup.players.push(player);
      } else {
        groups.push({ letter, players: [player] });
      }
    }
    return groups;
  });

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

  displayName(player: PlayerSearchResult): string {
    return player.useName ?? player.fullName ?? player.retroSheetId;
  }

  // Not a live active/retired flag -- Retrosheet has no current-roster feed, only a per-player
  // "last game" date that lags reality (e.g. a retirement announced since the last data update).
  // Null here means only "no final game on record", which is the most this data can honestly claim.
  hasNoFinalGame(player: PlayerSearchResult): boolean {
    return player.playerLastDate == null;
  }

  isDeceased(player: PlayerSearchResult): boolean {
    return player.deathDate != null;
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
