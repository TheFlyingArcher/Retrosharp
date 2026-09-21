import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { LoadingOverlay } from './loading-overlay';

@Component({
  standalone: true,
  imports: [LoadingOverlay],
  template: `<app-loading-overlay [active]="active"><p class="projected">Content</p></app-loading-overlay>`,
})
class HostComponent {
  active = false;
}

describe('LoadingOverlay', () => {
  let fixture: ComponentFixture<HostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HostComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(HostComponent);
  });

  it('projects content and hides the scrim when inactive', () => {
    fixture.detectChanges();

    expect(fixture.debugElement.query(By.css('.projected'))).toBeTruthy();
    expect(fixture.debugElement.query(By.css('.loading-overlay-scrim'))).toBeNull();
  });

  it('keeps projected content visible while showing the scrim when active', () => {
    fixture.componentInstance.active = true;
    fixture.detectChanges();

    expect(fixture.debugElement.query(By.css('.projected'))).toBeTruthy();
    expect(fixture.debugElement.query(By.css('.loading-overlay-scrim'))).toBeTruthy();
  });
});
