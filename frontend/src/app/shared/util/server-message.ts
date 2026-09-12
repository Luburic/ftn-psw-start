import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../api/problem-details';

export function serverMessage(error: unknown, fallback: string): string {
  if (error instanceof HttpErrorResponse) {
    const problem = error.error as ProblemDetails | null;
    if (typeof problem?.title === 'string') {
      return problem.title;
    }
  }
  return fallback;
}
