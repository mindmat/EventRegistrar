import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { AuthHttpInterceptor, AuthService } from '@auth0/auth0-angular';
import { AuthService as AuthServiceFuse } from './auth.service';

@NgModule({
    imports: [
        HttpClientModule
    ],
    providers: [
        AuthServiceFuse,
        AuthService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthHttpInterceptor,
            multi: true
        }
    ]
})
export class AuthModule {
}
