import { ApplicationConfig, ErrorHandler, LOCALE_ID, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { PreloadAllModules, RouteReuseStrategy, provideRouter, withComponentInputBinding, withPreloading } from '@angular/router';
import { appRoutes } from './app.routing';
import { BrowserModule } from '@angular/platform-browser';
import { provideAnimations } from '@angular/platform-browser/animations';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { API_BASE_URL, AuthService as ApiAuthService } from './api/api';
import { environment } from 'environments/environment';
import { AuthHttpInterceptor, AuthModule, AuthService } from '@auth0/auth0-angular';
import { AuthService as AuthServiceFuse } from './core/auth/auth.service';
import { ErrorHandlingInterceptor } from './api/errorhandling.interceptor';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MAT_MOMENT_DATE_ADAPTER_OPTIONS, MomentDateAdapter } from '@angular/material-moment-adapter';
import { MissingTranslationHandler, TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslationLoaderService } from './core/i18n/translation-loader.service';
import { MissingTranslationService } from './core/i18n/missing-translation.service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import * as Sentry from "@sentry/angular";
import { provideStore } from '@ngrx/store';
import { provideRouterStore } from '@ngrx/router-store';
import { NgxMatDateAdapter, NGX_MAT_DATE_FORMATS } from '@angular-material-components/datetime-picker';
import { NGX_MAT_MOMENT_DATE_ADAPTER_OPTIONS } from '@angular-material-components/moment-adapter';

export const DE_FORMATS = {
    parse: {
        dateInput: 'LL',
    },
    display: {
        dateInput: 'LL',
        monthYearLabel: 'MMM YYYY',
        dateA11yLabel: 'LL',
        monthYearA11yLabel: 'MMMM YYYY',
    },
};

export const DE_FORMATS_TIME = {
    parse: {
        dateInput: 'LLL',
    },
    display: {
        dateInput: 'LLL',
        monthYearLabel: 'MMM YYYY',
        dateA11yLabel: 'LL',
        monthYearA11yLabel: 'MMMM YYYY',
    },
};

export function translationLoaderFactory(translationLoaderService: TranslationLoaderService): TranslateLoader
{
    return translationLoaderService;
}

export const appConfig: ApplicationConfig = {
    providers: [
        provideZoneChangeDetection({ eventCoalescing: true }),
        provideRouter(appRoutes,
            withPreloading(PreloadAllModules),
            withComponentInputBinding()
        ),
        provideAnimations(),
        provideHttpClient(withInterceptorsFromDi()),
        importProvidersFrom(
            BrowserModule,
            AuthModule.forRoot({
                domain: environment.auth.domain,
                clientId: environment.auth.clientId,
                authorizationParams: {
                    audience: environment.auth.audience,
                    redirect_uri: window.location.origin + '/overview',
                    scope: 'openid profile email offline_access'
                },
                httpInterceptor: {
                    allowedList: [
                        {
                            uri: `${environment.API_BASE_URL}/*`,
                            tokenOptions: {
                                authorizationParams: {
                                    audience: environment.auth.audience
                                }
                            }
                        }
                    ]
                }
            }),
            TranslateModule.forRoot({
                defaultLanguage: 'de',
                isolate: false,
                loader: { provide: TranslateLoader, useFactory: translationLoaderFactory, deps: [TranslationLoaderService] },
                missingTranslationHandler: { provide: MissingTranslationHandler, useClass: MissingTranslationService }
            })
        ),
        provideStore(),
        provideRouterStore(),
        { provide: API_BASE_URL, useValue: environment.API_BASE_URL },
        AuthServiceFuse,
        ApiAuthService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthHttpInterceptor,
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: ErrorHandlingInterceptor,
            multi: true
        },
        { provide: LOCALE_ID, useValue: 'de-CH' },
        { provide: DateAdapter, useClass: MomentDateAdapter, deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS] },
        { provide: MAT_DATE_FORMATS, useValue: DE_FORMATS },
        { provide: NgxMatDateAdapter, useClass: MomentDateAdapter, deps: [MAT_DATE_LOCALE, NGX_MAT_MOMENT_DATE_ADAPTER_OPTIONS] },
        { provide: NGX_MAT_DATE_FORMATS, useValue: DE_FORMATS_TIME },
        {
            provide: ErrorHandler,
            useValue: Sentry.createErrorHandler()
        },
        { provide: MAT_DATE_LOCALE, useValue: 'de-CH' },
        { provide: NGX_MAT_DATE_FORMATS, useValue: DE_FORMATS_TIME },
        { provide: NGX_MAT_MOMENT_DATE_ADAPTER_OPTIONS, useValue: { strict: true } }
    ]
};
