import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { MatMenuTrigger } from '@angular/material/menu';
import { ThemeToggle } from './theme-toggle';
import { ThemeService } from '../../../service/theme.service';

describe('ThemeToggle', () => {
  let fixture: ComponentFixture<ThemeToggle>;
  let component: ThemeToggle;
  let themeService: ThemeService;

  beforeEach(async () => {
    // See theme.service.spec.ts: this project's vitest environment has no working `localStorage`
    // global at all, and ThemeService only needs to tolerate that (via its own try/catch), not
    // have it actually work, for these component-level tests.
    document.documentElement.classList.remove('theme-light', 'theme-dark');

    await TestBed.configureTestingModule({
      imports: [ThemeToggle],
    }).compileComponents();

    fixture = TestBed.createComponent(ThemeToggle);
    component = fixture.componentInstance;
    themeService = TestBed.inject(ThemeService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('shows the auto icon by default (system mode)', () => {
    expect(component.currentIcon()).toBe('brightness_auto');
    const icon = fixture.debugElement.query(By.css('mat-icon'));
    expect(icon.nativeElement.getAttribute('fontIcon')).toBe('brightness_auto');
  });

  it('selecting dark updates the icon and the underlying ThemeService', () => {
    component.select('dark');
    fixture.detectChanges();

    expect(themeService.mode()).toBe('dark');
    expect(component.currentIcon()).toBe('dark_mode');
  });

  it('selecting light updates the icon and the underlying ThemeService', () => {
    component.select('light');
    fixture.detectChanges();

    expect(themeService.mode()).toBe('light');
    expect(component.currentIcon()).toBe('light_mode');
  });

  // Regression test: MatMenuItem's own template does
  // `<ng-content select="mat-icon, [matMenuItemIcon]">` before its label span, which projects
  // *any* direct-child <mat-icon> into that leading slot -- not just the first one. A checkmark
  // <mat-icon> placed as a second direct child (the original bug here) got silently pulled out of
  // its intended trailing position and rendered before the label instead, which is exactly the
  // "stray character pushing the label right" the human reported. Nesting it one level inside the
  // label span (as theme-toggle.html now does) keeps it out of that named slot, since content
  // projection only matches direct children.
  it('keeps exactly one direct-child icon per menu item, with the checkmark nested inside the label', () => {
    component.select('dark');
    fixture.detectChanges();

    const trigger = fixture.debugElement.query(By.directive(MatMenuTrigger)).injector.get(MatMenuTrigger);
    trigger.openMenu();
    fixture.detectChanges();

    const items = Array.from(document.querySelectorAll('button[mat-menu-item]'));
    expect(items.length).toBe(3);

    for (const item of items) {
      const directIcons = Array.from(item.children).filter((el) => el.tagName.toLowerCase() === 'mat-icon');
      expect(directIcons.length).toBe(1);
    }

    const darkItem = items.find((item) => item.textContent?.includes('Dark'));
    const checkIcon = darkItem?.querySelector('.theme-toggle-check');
    expect(checkIcon).toBeTruthy();
    expect(checkIcon?.closest('.theme-toggle-label')).toBeTruthy();

    trigger.closeMenu();
  });
});
