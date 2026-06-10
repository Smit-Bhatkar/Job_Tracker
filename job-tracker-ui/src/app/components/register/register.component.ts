import { Component, inject, signal, computed } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

/** Custom validator that enforces password complexity rules */
function passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value) return null;

  const errors: ValidationErrors = {};

  if (value.length < 8) {
    errors['minLength'] = true;
  }
  if (!/[A-Z]/.test(value)) {
    errors['noUppercase'] = true;
  }
  if (!/[a-z]/.test(value)) {
    errors['noLowercase'] = true;
  }
  if (!/[\W_]/.test(value)) {
    errors['noSpecial'] = true;
  }

  return Object.keys(errors).length ? errors : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  submitting = signal(false);
  errorMessage = signal<string | null>(null);

  form: FormGroup = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, passwordStrengthValidator]],
  });

  // Computed password validation state for live feedback
  passwordValue = computed(() => {
    // Access the control value reactively via signal trick
    return this.form.get('password')?.value || '';
  });

  get passwordControl() {
    return this.form.get('password');
  }

  get hasMinLength(): boolean {
    return (this.passwordControl?.value?.length || 0) >= 8;
  }

  get hasUppercase(): boolean {
    return /[A-Z]/.test(this.passwordControl?.value || '');
  }

  get hasLowercase(): boolean {
    return /[a-z]/.test(this.passwordControl?.value || '');
  }

  get hasSpecial(): boolean {
    return /[\W_]/.test(this.passwordControl?.value || '');
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.authService.register({
      username: this.form.value.username.trim(),
      email: this.form.value.email.trim(),
      password: this.form.value.password,
    }).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.submitting.set(false);
        if (err.status === 409) {
          this.errorMessage.set('Username or email already exists.');
        } else if (err.error?.errors) {
          // Server-side validation errors
          const messages = Object.values(err.error.errors).flat();
          this.errorMessage.set(messages.join(' '));
        } else {
          this.errorMessage.set('Something went wrong. Please try again.');
        }
      },
    });
  }
}
