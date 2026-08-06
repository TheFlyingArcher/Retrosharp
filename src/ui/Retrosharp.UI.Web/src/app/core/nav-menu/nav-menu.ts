import { Component } from '@angular/core';
import { MatMenu, MatMenuItem } from '@angular/material/menu';
import { MatToolbar } from '@angular/material/toolbar';

@Component({
  selector: 'app-nav-menu',
  standalone: false,
  templateUrl: './nav-menu.html',
  styleUrl: './nav-menu.scss',
  imports: [MatMenu, MatMenuItem, MatToolbar]
})
export class NavMenu {}
