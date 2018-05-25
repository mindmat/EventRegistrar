import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { AuthService } from './authService.service';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  constructor(public auth: AuthService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    //request.headers.append("Authorization", `Bearer ${this.auth.ticket.access_token}`);
    request = request.clone({
      setHeaders: {
        Authorization: `Bearer ${this.auth.access_token}`
      }
    });
    return next.handle(request);
  }
}
