import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ApplicationService } from '../../services/application.service';
import { Application, STATUS_OPTIONS } from '../../models/application.model';
import { AuthService } from '../../services/auth.service';
import { StatusBadgeComponent } from '../status-badge/status-badge.component';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-application-list',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, DatePipe],
  templateUrl: './application-list.component.html',
  styleUrl: './application-list.component.css',
})
export class ApplicationListComponent implements OnInit {
  private appService = inject(ApplicationService);
  private authService = inject(AuthService);
  private router = inject(Router);

  applications = signal<Application[]>([]);
  activeFilter = signal<string>('All');
  loading = signal<boolean>(true);
  statusOptions = STATUS_OPTIONS;

  filteredApplications = computed(() => {
    const filter = this.activeFilter();
    const apps = this.applications();
    if (filter === 'All') return apps;
    return apps.filter(app => app.status === filter);
  });

  stats = computed(() => {
    const apps = this.applications();
    const counts: Record<string, number> = {};
    for (const status of STATUS_OPTIONS) {
      counts[status] = apps.filter(a => a.status === status).length;
    }
    return { total: apps.length, counts };
  });

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.loading.set(true);
    const userId = this.authService.user()?.id;
    this.appService.getAll(userId).subscribe({
      next: (data) => {
        this.applications.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load applications:', err);
        this.loading.set(false);
      },
    });
  }

  setFilter(status: string): void {
    this.activeFilter.set(status);
  }

  editApplication(id: string): void {
    this.router.navigate(['/edit', id]);
  }

  deleteApplication(id: string, company: string): void {
    if (!confirm(`Delete application for "${company}"? This cannot be undone.`)) {
      return;
    }
    this.appService.delete(id).subscribe({
      next: () => {
        this.applications.update(apps => apps.filter(a => a.id !== id));
      },
      error: (err) => {
        console.error('Failed to delete application:', err);
        alert('Failed to delete. Please try again.');
      },
    });
  }
}
