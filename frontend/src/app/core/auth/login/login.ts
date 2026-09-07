import { Component, inject, signal } from '@angular/core';
import { FormField, email, form, minLength, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../shared/util/server-message';
import { Auth } from '../auth';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-login',
  styleUrl: './login.scss',
  templateUrl: './login.html',
})
export class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  protected readonly form = form(signal({ email: '', password: '' }), (path) => {
    required(path.email, { message: 'Email is required.' });
    email(path.email, { message: 'Enter a valid email address.' });
    required(path.password, { message: 'Password is required.' });
    minLength(path.password, 6, { message: 'Password must be at least 6 characters.' });
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.auth.login(this.form.email().value(), this.form.password().value());
      await this.router.navigate(['/']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Login failed. Please try again.'));
    } finally {
      this.pending.set(false);
    }
  }
}
