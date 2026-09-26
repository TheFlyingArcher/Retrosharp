import { Component, input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

/**
 * Shared "Page Loading" overlay (see spec/frontend-prototype.md, "Page Loading"): a dimmed scrim
 * with a centered spinner, layered on top of projected content rather than replacing it.
 *
 * Pages that swap their whole content area for a spinner while `loading` (the original pattern
 * on Players/Player Detail) collapse the page's height for the duration of the request, which
 * shifts scroll position under an unmoved viewport -- see spec/frontend-ux-improvements.md,
 * "Players Page: Next/Previous Scrolling the Page". Keeping the previous content laid out and
 * visible under the scrim avoids that: the document height doesn't change, so scroll position
 * doesn't move, while the scrim still blocks interaction until loading finishes.
 */
@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  templateUrl: './loading-overlay.html',
  styleUrl: './loading-overlay.css',
  imports: [MatProgressSpinnerModule],
})
export class LoadingOverlay {
  readonly active = input.required<boolean>();
}
