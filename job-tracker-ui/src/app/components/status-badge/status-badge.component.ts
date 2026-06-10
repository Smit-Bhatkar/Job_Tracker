import { Component, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `
    <span class="badge" [class]="'badge badge--' + status().toLowerCase()">
      {{ status() }}
    </span>
  `,
  styles: `
    .badge {
      display: inline-block;
      padding: 0.25rem 0.75rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 600;
      text-transform: capitalize;
      letter-spacing: 0.025em;
      line-height: 1.4;
      white-space: nowrap;
    }
    .badge--wishlist {
      background-color: hsla(215, 20%, 65%, 0.15);
      color: hsl(215, 20%, 65%);
    }
    .badge--applied {
      background-color: hsla(217, 91%, 60%, 0.15);
      color: hsl(217, 91%, 60%);
    }
    .badge--interview {
      background-color: hsla(38, 92%, 50%, 0.15);
      color: hsl(38, 92%, 50%);
    }
    .badge--offer {
      background-color: hsla(142, 71%, 45%, 0.15);
      color: hsl(142, 71%, 45%);
    }
    .badge--rejected {
      background-color: hsla(0, 84%, 60%, 0.15);
      color: hsl(0, 84%, 60%);
    }
  `,
})
export class StatusBadgeComponent {
  status = input.required<string>();
}
