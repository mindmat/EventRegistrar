import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { CommonModule } from '@angular/common';
import { ExtraOptions, PreloadAllModules, RouterModule } from '@angular/router';
import { MarkdownModule } from 'ngx-markdown';
import { FuseModule } from '@fuse';
import { FuseConfigModule } from '@fuse/services/config';
import { FuseMockApiModule } from '@fuse/lib/mock-api';
import { CoreModule } from 'app/core/core.module';
import { appConfig } from 'app/core/config/app.config';
import { mockApiServices } from 'app/mock-api';
import { LayoutModule } from 'app/layout/layout.module';
import { AppComponent } from 'app/app.component';
import { appRoutes } from 'app/app.routing';
import { AuthHttpInterceptor, AuthModule, AuthService } from '@auth0/auth0-angular';
import { OverviewComponent } from './modules/admin/overview/overview.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FuseFindByKeyPipeModule } from '@fuse/pipes/find-by-key';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBar, MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ParticipantsDoubleComponent } from './modules/admin/participants/participants-double/participants-double.component';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { BaseUrlInterceptor } from '@fuse/services/utils/baseUrl.interceptor';
import { AuthService as AuthServiceFuse } from './core/auth/auth.service';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { ParticipantComponent } from './modules/admin/participants/participant/participant.component';
import { ParticipantsSingleComponent } from './modules/admin/participants/participants-single/participants-single.component';
import { RegistrationComponent } from './modules/admin/registration/registration.component';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { FuseCardComponent } from '@fuse/components/card/card.component';
import { FuseCardModule } from '@fuse/components/card';

const routerConfig: ExtraOptions = {
    preloadingStrategy: PreloadAllModules,
    scrollPositionRestoration: 'enabled'
};

@NgModule({
    declarations: [
        AppComponent,
        OverviewComponent,
        ParticipantsDoubleComponent,
        ParticipantComponent,
        ParticipantsSingleComponent,
        RegistrationComponent
    ],
    providers: [
        { provide: 'BASE_API_URL', useValue: 'https://localhost:5001' },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: BaseUrlInterceptor,
            multi: true
        },
        AuthServiceFuse,
        AuthService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthHttpInterceptor,
            multi: true
        }
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        RouterModule.forRoot(appRoutes, routerConfig),

        // Fuse, FuseConfig & FuseMockAPI
        FuseModule,
        FuseConfigModule.forRoot(appConfig),
        FuseMockApiModule.forRoot(mockApiServices),
        FuseFindByKeyPipeModule,
        FuseCardModule,

        // Core module of your application
        CoreModule,

        // Layout module of your application
        LayoutModule,

        // 3rd party modules that require global configuration via forRoot
        MarkdownModule.forRoot({}),

        CommonModule,
        DragDropModule,
        MatFormFieldModule,
        MatSelectModule,
        MatButtonModule,
        MatIconModule,
        MatSlideToggleModule,
        MatInputModule,
        MatProgressBarModule,
        MatTooltipModule,
        MatDividerModule,
        MatMenuModule,

        AuthModule.forRoot({
            domain: 'eventregistrar.eu.auth0.com',
            clientId: 'yoCfBbd0zLWvoA6qg0FzNPtHxEHu4YH3',

            // Request this audience at user authentication time
            audience: 'https://eventregistrar.azurewebsites.net/api',

            // Request this scope at user authentication time
            // scope: 'read:current_user',

            // Specify configuration for the interceptor              
            httpInterceptor: {
                allowedList: [
                    {
                        // Match any request that starts {uri} (note the asterisk)
                        uri: 'https://eventregistrar.azurewebsites.net/api/*',
                        tokenOptions: {
                            // The attached token should target this audience
                            audience: 'https://eventregistrar.azurewebsites.net/api',

                            // The attached token should have these scopes
                            // scope: 'read:current_user'
                        }
                    },
                    {
                        // Match any request that starts {uri} (note the asterisk)
                        uri: 'https://localhost:5001/api/*',
                        tokenOptions: {
                            // The attached token should target this audience
                            audience: 'https://eventregistrar.azurewebsites.net/api',

                            // The attached token should have these scopes
                            // scope: 'read:current_user'
                        }
                    }
                ]
            }
        })
    ],
    bootstrap: [
        AppComponent
    ]
})
export class AppModule
{
}
