import { Inject, Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class BaseUrlInterceptor implements HttpInterceptor
{
    constructor(@Inject('BASE_API_URL') private baseUrl: string)
    {
    }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>>
    {
        if (request.url.startsWith('api'))
        {
            const apiReq = request.clone({ url: `${this.baseUrl}/${request.url}` });
            return next.handle(apiReq);
        }
        return next.handle(request);
    }
}