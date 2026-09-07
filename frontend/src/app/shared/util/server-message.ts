import { HttpErrorResponse } from '@angular/common/http';

export function serverMessage(error: unknown, fallback: string): string {
  if (error instanceof HttpErrorResponse && typeof error.error?.title === 'string') {
    return error.error.title;
  }
  return fallback;
}
