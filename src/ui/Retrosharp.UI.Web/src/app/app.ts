import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { NavMenu } from './core/nav-menu/nav-menu';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [RouterOutlet, NavMenu],
})
export class App {
  protected readonly title = signal('Retrosharp.UI.Web');
}
