import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApplicationService } from '../../services/application.service';
import { AuthService } from '../../services/auth.service';
import { STATUS_OPTIONS, TYPE_OPTIONS } from '../../models/application.model';

@Component({
  selector: 'app-application-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './application-form.component.html',
  styleUrl: './application-form.component.css',
})
export class ApplicationFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private appService = inject(ApplicationService);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  isEditMode = signal(false);
  applicationId = signal<string | null>(null);
  ownerId = signal<string | null>(null);
  loading = signal(false);
  submitting = signal(false);

  statusOptions = STATUS_OPTIONS;
  typeOptions = TYPE_OPTIONS;

  form: FormGroup = this.fb.group({
    company:     ['', [Validators.required, Validators.maxLength(100)]],
    role:        ['', [Validators.required, Validators.maxLength(100)]],
    type:        ['Job'],
    status:      ['Wishlist'],
    dateApplied: [''],
    link:        ['', [Validators.pattern(/^https?:\/\/.+/)]],
    notes:       [''],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.applicationId.set(id);
      this.loadApplication(id);
    }
  }

  private loadApplication(id: string): void {
    this.loading.set(true);
    this.appService.getById(id).subscribe({
      next: (app) => {
        this.ownerId.set((app as any).userId || null);
        this.form.patchValue({
          company:     app.company,
          role:        app.role,
          type:        app.type,
          status:      app.status,
          dateApplied: app.dateApplied ? app.dateApplied.split('T')[0] : '',
          link:        app.link || '',
          notes:       app.notes || '',
        });
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load application:', err);
        this.loading.set(false);
        alert('Could not load application. Returning to list.');
        this.router.navigate(['/']);
      },
    });
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.submitting.set(true);
    const formValue = this.form.value;
    const userId = this.authService.user()?.id;

    const payload = {
      company:     formValue.company.trim(),
      role:        formValue.role.trim(),
      type:        formValue.type,
      status:      formValue.status,
      dateApplied: formValue.dateApplied || null,
      link:        formValue.link?.trim() || null,
      notes:       formValue.notes?.trim() || null,
      userId:      userId || this.ownerId() || null,
    };

    if (this.isEditMode() && this.applicationId()) {
      this.appService.update(this.applicationId()!, payload).subscribe({
        next: () => this.router.navigate(['/']),
        error: (err) => {
          console.error('Failed to update application:', err);
          this.submitting.set(false);
          alert('Failed to save. Please try again.');
        },
      });
    } else {
      this.appService.create(payload).subscribe({
        next: () => this.router.navigate(['/']),
        error: (err) => {
          console.error('Failed to create application:', err);
          this.submitting.set(false);
          alert('Failed to save. Please try again.');
        },
      });
    }
  }
}
