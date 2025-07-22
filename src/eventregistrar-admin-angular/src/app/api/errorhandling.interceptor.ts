import { inject, Injectable } from '@angular/core';
import { HttpErrorResponse, HttpEvent, HttpEventType, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable()
export class ErrorHandlingInterceptor implements HttpInterceptor
{
    handleRequestsAutomatically: boolean;
    private _snackBar = inject(MatSnackBar);

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>>
    {
        return next.handle(req).pipe(
            catchError((err, _) =>
            {
                if (err instanceof HttpErrorResponse)
                {
                    this.showToast(err);
                }
                return throwError(() => err);
            }));
    }

    async showToast(response: HttpErrorResponse)
    {
        try
        {
            const message = `Fehler: ${response.message}`;
            this._snackBar.open(message, 'OK', { duration: 10000 });
        }
        catch (e)
        {
            console.error('Error displaying toast message:', e, response);
        }
    }
}
