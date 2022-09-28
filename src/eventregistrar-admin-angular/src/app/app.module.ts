// Import Froala Editor plugins.
import 'froala-editor/js/plugins/url.min.js';
import 'froala-editor/js/plugins/table.min.js';
import 'froala-editor/js/plugins/paragraph_format.min.js';
import 'froala-editor/js/plugins/paragraph_style.min.js';
import 'froala-editor/js/plugins/lists.min.js';
import 'froala-editor/js/plugins/link.min.js';
import 'froala-editor/js/plugins/line_breaker.min.js';
import 'froala-editor/js/plugins/line_height.min.js';
import 'froala-editor/js/plugins/image.min.js';
import 'froala-editor/js/plugins/image_manager.min.js';
import 'froala-editor/js/plugins/fullscreen.min.js';
import 'froala-editor/js/plugins/font_size.min.js';
import 'froala-editor/js/plugins/font_family.min.js';
import 'froala-editor/js/plugins/align.min.js';
import 'froala-editor/js/plugins/code_view.min.js';
import 'froala-editor/js/plugins/code_beautifier.min.js';
import 'froala-editor/js/plugins/help.min.js';

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
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ParticipantsDoubleComponent } from './modules/admin/participants/participants-double/participants-double.component';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthService as AuthServiceFuse } from './core/auth/auth.service';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { ParticipantComponent } from './modules/admin/participants/participant/participant.component';
import { ParticipantsSingleComponent } from './modules/admin/participants/participants-single/participants-single.component';
import { RegistrationComponent } from './modules/admin/registration/registration.component';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { FuseCardModule } from '@fuse/components/card';
import { BankStatementsComponent } from './modules/admin/accounting/bankStatements/bankStatements.component';
import { SearchRegistrationComponent } from './modules/admin/registrations/search-registration/search-registration.component';
import { SettlePaymentsComponent } from './modules/admin/accounting/settle-payments/settle-payments.component';
import { SettlePaymentComponent } from './modules/admin/accounting/settle-payment/settle-payment.component';
import { AssignmentCandidateRegistrationComponent } from './modules/admin/accounting/settle-payment/assignment-candidate-registration/assignment-candidate-registration.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MissingTranslationHandler, TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslationLoaderService } from './core/i18n/translation-loader.service';
import { MissingTranslationService } from './core/i18n/missing-translation.service';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { DuePaymentsComponent } from './modules/admin/accounting/due-payments/due-payments.component';
import { AutoMailTemplatesComponent } from './modules/admin/mailing/auto-mail-templates/auto-mail-templates.component';
import { AutoMailTemplateComponent } from './modules/admin/mailing/auto-mail-templates/auto-mail-template/auto-mail-template.component';
import { FroalaEditorModule, FroalaViewModule } from 'angular-froala-wysiwyg';
import { AutoMailPreviewComponent } from './modules/admin/mailing/auto-mail-templates/auto-mail-preview/auto-mail-preview.component';
import { SearchModule } from './layout/common/search/search.module';
import { ReleaseMailsComponent } from './modules/admin/mailing/mails/release-mails/release-mails.component';
import { MailViewComponent } from './modules/admin/mailing/mails/mail-view/mail-view.component';
import { UserAccessComponent } from './modules/admin/auth/user-access/user-access.component';

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
        RegistrationComponent,
        BankStatementsComponent,
        SearchRegistrationComponent,
        SettlePaymentsComponent,
        SettlePaymentComponent,
        AssignmentCandidateRegistrationComponent,
        DuePaymentsComponent,
        AutoMailTemplatesComponent,
        AutoMailTemplateComponent,
        AutoMailPreviewComponent,
        ReleaseMailsComponent,
        MailViewComponent,
        UserAccessComponent
    ],
    providers: [
        { provide: 'BASE_API_URL', useValue: 'https://localhost:5001' },
        // {
        //     provide: HTTP_INTERCEPTORS,
        //     useClass: BaseUrlInterceptor,
        //     multi: true
        // },
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
        SearchModule,

        // Core module of your application
        CoreModule,

        // Layout module of your application
        LayoutModule,

        // 3rd party modules that require global configuration via forRoot
        MarkdownModule.forRoot({}),

        TranslateModule.forRoot({
            defaultLanguage: 'de',
            isolate: false,
            loader: { provide: TranslateLoader, useFactory: TranslationLoaderFactory, deps: [TranslationLoaderService] },
            missingTranslationHandler: { provide: MissingTranslationHandler, useClass: MissingTranslationService }
        }),

        CommonModule,
        ReactiveFormsModule,

        DragDropModule,
        MatFormFieldModule,
        MatSelectModule,
        MatButtonModule,
        MatCheckboxModule,
        MatIconModule,
        MatSlideToggleModule,
        MatInputModule,
        MatProgressBarModule,
        MatTooltipModule,
        MatDividerModule,
        MatMenuModule,
        MatSidenavModule,

        FroalaEditorModule.forRoot(),
        FroalaViewModule.forRoot(),

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

export function TranslationLoaderFactory(service: TranslationLoaderService)
{
    return service.createLoader();
}
