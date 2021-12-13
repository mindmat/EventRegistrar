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
import { AuthModule } from '@auth0/auth0-angular';
import { OverviewComponent } from './modules/admin/overview/overview/overview.component';

const routerConfig: ExtraOptions = {
    preloadingStrategy       : PreloadAllModules,
    scrollPositionRestoration: 'enabled'
};

@NgModule({
    declarations: [
        AppComponent,
        OverviewComponent
    ],
    imports     : [
        BrowserModule,
        BrowserAnimationsModule,
        RouterModule.forRoot(appRoutes, routerConfig),

        // Fuse, FuseConfig & FuseMockAPI
        FuseModule,
        FuseConfigModule.forRoot(appConfig),
        FuseMockApiModule.forRoot(mockApiServices),

        // Core module of your application
        CoreModule,

        // Layout module of your application
        LayoutModule,

        // 3rd party modules that require global configuration via forRoot
        MarkdownModule.forRoot({}),
        
        CommonModule,
        
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
