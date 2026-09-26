import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme.service';

// This project's vitest environment doesn't provide a working `localStorage` global at all (see
// the pre-existing, unrelated failure in players.spec.ts) -- ThemeService tolerates that via its
// own try/catch, but exercising the persistence behavior itself needs a real Storage
// implementation, so these tests install a minimal in-memory one for their own duration.
class MemoryStorage implements Storage {
  private readonly store = new Map<string, string>();

  get length(): number {
    return this.store.size;
  }

  clear(): void {
    this.store.clear();
  }

  getItem(key: string): string | null {
    return this.store.has(key) ? this.store.get(key)! : null;
  }

  key(index: number): string | null {
    return Array.from(this.store.keys())[index] ?? null;
  }

  removeItem(key: string): void {
    this.store.delete(key);
  }

  setItem(key: string, value: string): void {
    this.store.set(key, value);
  }
}

describe('ThemeService', () => {
  beforeEach(() => {
    Object.defineProperty(globalThis, 'localStorage', { value: new MemoryStorage(), configurable: true });
    document.documentElement.classList.remove('theme-light', 'theme-dark');
    TestBed.configureTestingModule({});
  });

  it('defaults to system mode with no override class when nothing is stored', () => {
    const service = TestBed.inject(ThemeService);
    TestBed.tick();

    expect(service.mode()).toBe('system');
    expect(document.documentElement.classList.contains('theme-light')).toBe(false);
    expect(document.documentElement.classList.contains('theme-dark')).toBe(false);
  });

  it('applies theme-dark and persists the choice when dark is selected', () => {
    const service = TestBed.inject(ThemeService);
    service.setMode('dark');
    TestBed.tick();

    expect(document.documentElement.classList.contains('theme-dark')).toBe(true);
    expect(document.documentElement.classList.contains('theme-light')).toBe(false);
    expect(localStorage.getItem('retrosharp-theme-mode')).toBe('dark');
  });

  it('applies theme-light when light is selected', () => {
    const service = TestBed.inject(ThemeService);
    service.setMode('light');
    TestBed.tick();

    expect(document.documentElement.classList.contains('theme-light')).toBe(true);
    expect(document.documentElement.classList.contains('theme-dark')).toBe(false);
  });

  it('clears both override classes and the stored value when switched back to system', () => {
    const service = TestBed.inject(ThemeService);
    service.setMode('dark');
    TestBed.tick();

    service.setMode('system');
    TestBed.tick();

    expect(document.documentElement.classList.contains('theme-dark')).toBe(false);
    expect(document.documentElement.classList.contains('theme-light')).toBe(false);
    expect(localStorage.getItem('retrosharp-theme-mode')).toBeNull();
  });

  it('picks up a previously-stored mode on construction', () => {
    localStorage.setItem('retrosharp-theme-mode', 'dark');

    const service = TestBed.inject(ThemeService);
    TestBed.tick();

    expect(service.mode()).toBe('dark');
    expect(document.documentElement.classList.contains('theme-dark')).toBe(true);
  });
});
