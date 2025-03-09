import { inject, Injectable } from '@angular/core';
import { HttpErrorResponse, HttpEvent, HttpEventType, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { catchError, Observable, tap } from 'rxjs';
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
                    this.showToast(err.error);
                }
                throw err;
            }));
    }

    async showToast(blob: Blob)
    {
        const message = `Fehler: ${await blob.text()}`;
        this._snackBar.open(message, 'OK', { duration: 10000 });
    }
}
