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
import { MatIconModule } from '@angular/material/icon';
import { FuseFindByKeyPipeModule } from '@fuse/pipes/find-by-key';
import { MatSidenavModule } from '@angular/material/sidenav';
import { ParticipantsDoubleComponent } from './modules/admin/participants/participants-double/participants-double.component';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthService as AuthServiceFuse } from './core/auth/auth.service';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { ParticipantComponent } from './modules/admin/participants/participant/participant.component';
import { ParticipantsSingleComponent } from './modules/admin/participants/participants-single/participants-single.component';
import { RegistrationComponent } from './modules/admin/registration/registration.component';
import { MatDividerModule } from '@angular/material/divider';
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
import { DuePaymentsComponent } from './modules/admin/accounting/due-payments/due-payments.component';
import { AutoMailTemplatesComponent } from './modules/admin/mailing/auto-mail-templates/auto-mail-templates.component';
import { AutoMailTemplateComponent } from './modules/admin/mailing/auto-mail-templates/auto-mail-template/auto-mail-template.component';
import { FroalaEditorModule, FroalaViewModule } from 'angular-froala-wysiwyg';
import { AutoMailPreviewComponent } from './modules/admin/mailing/auto-mail-templates/auto-mail-preview/auto-mail-preview.component';
import { SearchModule } from './layout/common/search/search.module';
import { ReleaseMailsComponent } from './modules/admin/mailing/mails/release-mails/release-mails.component';
import { MailViewComponent } from './modules/admin/mailing/mails/mail-view/mail-view.component';
import { UserAccessComponent } from './modules/admin/auth/user-access/user-access.component';
import { SelectEventComponent } from './modules/admin/events/select-event/select-event.component';
import { CreateEventComponent } from './modules/admin/events/select-event/create-event/create-event.component';
import { FormMappingComponent } from './modules/admin/registration-forms/form-mapping/form-mapping.component';
import { TagsPickerComponent } from './shared/tags-picker/tags-picker.component';
import { PricingComponent } from './modules/admin/pricing/pricing.component';
import { RegistrableDetailComponent } from './modules/admin/overview/registrable-detail/registrable-detail.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatInputModule } from '@angular/material/input';
import { MatLuxonDateModule } from '@angular/material-luxon-adapter';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatOptionModule } from '@angular/material/core';
import { SharedModule } from './shared/shared.module';
import { FileUploadComponent } from './modules/admin/infrastructure/file-upload/file-upload.component';
import { MailViewerComponent } from './modules/admin/mailing/mails/mail-viewer/mail-viewer.component';
import { MatchPartnersComponent } from './modules/admin/registrations/match-partners/match-partners.component';
import { MatchPartnerComponent } from './modules/admin/registrations/match-partner/match-partner.component';
import { FuseAlertModule } from '@fuse/components/alert';
import { CreateIndividualReductionComponent } from './modules/admin/registration/create-individual-reduction/create-individual-reduction.component';
import { CancelRegistrationComponent } from './modules/admin/registration/cancel-registration/cancel-registration.component';
import { AgoPipe } from './modules/admin/infrastructure/ago.pipe';
import { UserHasRightDirective } from './core/auth/user-has-right.directive';
import { TranslateEnumPipe } from './modules/admin/infrastructure/translate-enum.pipe';
import { ChangeSpotsComponent } from './modules/admin/registration/change-spots/change-spots.component';
import { ProblematicEmailsComponent } from './modules/admin/mailing/problematic-emails/problematic-emails.component';
import { ChangeEmailComponent } from './modules/admin/registration/change-email/change-email.component';
import { RemarksOverviewComponent } from './modules/admin/registrations/remarks-overview/remarks-overview.component';
import { NotesOverviewComponent } from './modules/admin/registrations/notes-overview/notes-overview.component';
import { BulkMailTemplatesComponent } from './modules/admin/mailing/bulk-mail-templates/bulk-mail-templates.component';
import { BulkMailTemplateComponent } from './modules/admin/mailing/bulk-mail-template/bulk-mail-template.component';
import { HostingOverviewComponent } from './modules/admin/hosting/hosting-overview/hosting-overview.component';
import { NgApexchartsModule } from 'ng-apexcharts';
import { PaymentDifferencesComponent } from './modules/admin/accounting/payment-differences/payment-differences.component';
import { CancellationsComponent } from './modules/admin/registration/cancellations/cancellations.component';
import { PayoutsComponent } from './modules/admin/accounting/payouts/payouts.component';
import { AllParticipantsComponent } from './modules/admin/registrations/all-participants/all-participants.component';
import { CreateAssignPaymentComponent } from './modules/admin/registration/create-assign-payment/create-assign-payment.component';

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
        UserAccessComponent,
        SelectEventComponent,
        CreateEventComponent,
        FormMappingComponent,
        TagsPickerComponent,
        PricingComponent,
        RegistrableDetailComponent,
        FileUploadComponent,
        MailViewerComponent,
        MatchPartnersComponent,
        MatchPartnerComponent,
        CreateIndividualReductionComponent,
        CancelRegistrationComponent,
        AgoPipe,
        TranslateEnumPipe,
        UserHasRightDirective,
        ChangeSpotsComponent,
        ProblematicEmailsComponent,
        ChangeEmailComponent,
        RemarksOverviewComponent,
        NotesOverviewComponent,
        BulkMailTemplatesComponent,
        BulkMailTemplateComponent,
        HostingOverviewComponent,
        PaymentDifferencesComponent,
        CancellationsComponent,
        PayoutsComponent,
        AllParticipantsComponent,
        CreateAssignPaymentComponent
    ],
    providers: [
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
        FuseAlertModule,
        SearchModule,

        // Core module of your application
        CoreModule,

        // Layout module of your application
        LayoutModule,
        SharedModule,

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
        MatIconModule,
        MatCheckboxModule,
        MatButtonModule,
        MatSlideToggleModule,
        MatInputModule,
        MatLuxonDateModule,
        MatProgressBarModule,
        MatTooltipModule,
        MatDividerModule,
        MatMenuModule,
        MatSidenavModule,
        MatDialogModule,
        MatOptionModule,
        MatDatepickerModule,

        FroalaEditorModule.forRoot(),
        FroalaViewModule.forRoot(),

        NgApexchartsModule,

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
                        uri: 'https://event-admin-backend.azurewebsites.net/api/*',
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
