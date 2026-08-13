import { Component, inject } from '@angular/core';
import { PlayerService } from '../../service/player.service';

@Component({
  selector: 'app-players',
  templateUrl: './players.html',
  styleUrl: './players.css',
})
export class Players {
  service = inject(PlayerService)
}
