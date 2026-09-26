import { Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { ThemeMode, ThemeService } from '../../../service/theme.service';

interface ThemeOption {
  mode: ThemeMode;
  label: string;
  icon: string;
}

const THEME_OPTIONS: ThemeOption[] = [
  { mode: 'system', label: 'System', icon: 'brightness_auto' },
  { mode: 'light', label: 'Light', icon: 'light_mode' },
  { mode: 'dark', label: 'Dark', icon: 'dark_mode' },
];

/** Nav-bar control for choosing the theme mode -- see ThemeService for how the choice is applied. */
@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  templateUrl: './theme-toggle.html',
  styleUrl: './theme-toggle.css',
  imports: [MatButtonModule, MatIconModule, MatMenuModule],
})
export class ThemeToggle {
  private readonly themeService = inject(ThemeService);

  readonly options = THEME_OPTIONS;
  readonly mode = this.themeService.mode;

  readonly currentIcon = computed(
    () => this.options.find((option) => option.mode === this.mode())?.icon ?? 'brightness_auto',
  );

  select(mode: ThemeMode): void {
    this.themeService.setMode(mode);
  }
}
