import { Injectable, effect, signal } from '@angular/core';

export type ThemeMode = 'system' | 'light' | 'dark';

const STORAGE_KEY = 'retrosharp-theme-mode';

/**
 * Tracks the user's chosen theme mode ('system' follows the OS/browser's prefers-color-scheme,
 * 'light'/'dark' force one regardless of it) and reflects it onto <html> as a `theme-light`/
 * `theme-dark` class, which material-theme.scss's selectors key off of. Persisted to
 * localStorage so a manual choice survives a reload; 'system' is the default when nothing has
 * been chosen yet, and is never itself written to localStorage/a class -- it's just the absence
 * of an override.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly mode = signal<ThemeMode>(this.readStoredMode());

  constructor() {
    effect(() => this.applyMode(this.mode()));
  }

  setMode(mode: ThemeMode): void {
    this.mode.set(mode);
    this.writeStoredMode(mode);
  }

  private applyMode(mode: ThemeMode): void {
    const root = document.documentElement;
    root.classList.toggle('theme-light', mode === 'light');
    root.classList.toggle('theme-dark', mode === 'dark');
  }

  // localStorage can throw (privacy-restricted contexts, some test environments) -- falling back
  // to 'system'/in-memory-only rather than letting that take down theme selection entirely.
  private readStoredMode(): ThemeMode {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      return stored === 'light' || stored === 'dark' ? stored : 'system';
    } catch {
      return 'system';
    }
  }

  private writeStoredMode(mode: ThemeMode): void {
    try {
      if (mode === 'system') {
        localStorage.removeItem(STORAGE_KEY);
      } else {
        localStorage.setItem(STORAGE_KEY, mode);
      }
    } catch {
      // Mode still applies for the current session via the signal; it just won't persist.
    }
  }
}
