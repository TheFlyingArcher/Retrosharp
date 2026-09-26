import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { ThemeToggle } from '../theme-toggle/theme-toggle';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.html',
  styleUrl: './nav-menu.scss',
  imports: [RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, ThemeToggle],
})
export class NavMenu {}
