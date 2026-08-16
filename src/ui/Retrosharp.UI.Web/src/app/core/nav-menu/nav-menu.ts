import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.html',
  styleUrl: './nav-menu.scss',
  imports: [RouterLink, MatToolbarModule, MatButtonModule],
})
export class NavMenu {}
