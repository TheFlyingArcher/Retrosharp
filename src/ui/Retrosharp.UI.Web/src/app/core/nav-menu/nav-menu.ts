import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatToolbar } from '@angular/material/toolbar';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.html',
  styleUrl: './nav-menu.scss',
  imports: [RouterLink, MatToolbar],
})
export class NavMenu {}
