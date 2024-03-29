import { Route } from '@angular/router';
import { AuthGuard } from '@auth0/auth0-angular';
import { NoAuthGuard } from 'app/core/auth/guards/noAuth.guard';
import { LayoutComponent } from 'app/layout/layout.component';
import { InitialDataResolver } from 'app/app.resolvers';
import { OverviewComponent } from './modules/admin/overview/overview.component';
import { OverviewResolver } from './modules/admin/overview/overview.resolvers';
import { ParticipantsDoubleComponent } from './modules/admin/participants/participants-double/participants-double.component';
import { ParticipantsResolver } from './modules/admin/participants/participants.resolvers';
import { ParticipantsSingleComponent } from './modules/admin/participants/participants-single/participants-single.component';
import { RegistrationComponent } from './modules/admin/registration/registration.component';
import { RegistrationResolver } from './modules/admin/registration/registration.resolvers';
import { BankStatementsComponent } from './modules/admin/accounting/bankStatements/bankStatements.component';
import { BankStatementsResolver } from './modules/admin/accounting/bankStatements/bankStatements.resolvers';
import { SearchRegistrationComponent } from './modules/admin/registrations/search-registration/search-registration.component';
import { SettlePaymentsComponent } from './modules/admin/accounting/settle-payments/settle-payments.component';
import { SettlePaymentsResolver } from './modules/admin/accounting/settle-payments/settle-payments.resolvers';
import { SettlePaymentComponent } from './modules/admin/accounting/settle-payment/settle-payment.component';
import { SettlePaymentResolver } from './modules/admin/accounting/settle-payment/settle-payment.resolver';
import { DuePaymentsComponent } from './modules/admin/accounting/due-payments/due-payments.component';
import { DuePaymentsResolver } from './modules/admin/accounting/due-payments/due-payments.resolver';
import { AutoMailTemplatesComponent } from './modules/admin/mailing/auto-mail-templates/auto-mail-templates.component';
import { AutoMailTemplatesResolver } from './modules/admin/mailing/auto-mail-templates/auto-mail-templates.resolver';
import { AutoMailTemplateComponent } from './modules/admin/mailing/auto-mail-templates/auto-mail-template/auto-mail-template.component';
import { AutoMailTemplateResolver } from './modules/admin/mailing/auto-mail-templates/auto-mail-template/auto-mail-template.resolver';
import { AutoMailPreviewComponent } from './modules/admin/mailing/auto-mail-templates/auto-mail-preview/auto-mail-preview.component';
import { AutoMailPreviewResolver } from './modules/admin/mailing/auto-mail-templates/auto-mail-preview/auto-mail-preview.resolver';
import { ReleaseMailsComponent } from './modules/admin/mailing/mails/release-mails/release-mails.component';
import { ReleaseMailsResolver } from './modules/admin/mailing/mails/release-mails/release-mails.resolver';
import { MailViewComponent } from './modules/admin/mailing/mails/mail-view/mail-view.component';
import { MailViewResolver } from './modules/admin/mailing/mails/mail-view/mail-view.resolver';
import { UserAccessComponent } from './modules/admin/event-settings/user-access/user-access.component';
import { EventSettingsResolver } from './modules/admin/event-settings/event-settings.resolver';
import { EventAcronymResolver } from './modules/admin/events/event-acronym.resolver';
import { SelectEventComponent } from './modules/admin/events/select-event/select-event.component';
import { SelectEventResolver } from './modules/admin/events/select-event/select-event.resolver';
import { FormMappingComponent } from './modules/admin/registration-forms/form-mapping/form-mapping.component';
import { FormMappingResolver } from './modules/admin/registration-forms/form-mapping/form-mapping.resolver';
import { PricingResolver } from './modules/admin/pricing/pricing.resolver';
import { PricingComponent } from './modules/admin/pricing/pricing.component';
import { MailViewerComponent } from './modules/admin/mailing/mails/mail-viewer/mail-viewer.component';
import { MailViewerResolver } from './modules/admin/mailing/mails/mail-viewer/mail-viewer.resolver';
import { MatchPartnersComponent } from './modules/admin/registrations/match-partners/match-partners.component';
import { MatchPartnersResolver } from './modules/admin/registrations/match-partners/match-partners.resolver';
import { MatchPartnerComponent } from './modules/admin/registrations/match-partner/match-partner.component';
import { MatchPartnerResolver } from './modules/admin/registrations/match-partner/match-partner.resolver';
import { SearchRegistrationResolver } from './modules/admin/registrations/search-registration/search-registration.resolver';
import { ProblematicEmailsComponent } from './modules/admin/mailing/problematic-emails/problematic-emails.component';
import { ProblematicEmailsResolver } from './modules/admin/mailing/problematic-emails/problematic-emails.resolver';
import { RemarksOverviewComponent } from './modules/admin/registrations/remarks-overview/remarks-overview.component';
import { RemarksOverviewResolver } from './modules/admin/registrations/remarks-overview/remarks-overview.resolver';
import { NotesOverviewComponent } from './modules/admin/registrations/notes-overview/notes-overview.component';
import { NotesOverviewResolver } from './modules/admin/registrations/notes-overview/notes-overview.resolver';
import { BulkMailTemplatesComponent } from './modules/admin/mailing/bulk-mail-templates/bulk-mail-templates.component';
import { BulkMailTemplatesResolver } from './modules/admin/mailing/bulk-mail-templates/bulk-mail-templates.resolver';
import { BulkMailTemplateComponent } from './modules/admin/mailing/bulk-mail-template/bulk-mail-template.component';
import { BulkMailTemplateResolver } from './modules/admin/mailing/bulk-mail-template/bulk-mail-template.resolver';
import { HostingOverviewComponent } from './modules/admin/hosting/hosting-overview/hosting-overview.component';
import { HostingOverviewResolver } from './modules/admin/hosting/hosting-overview/hosting-overview.resolver';
import { PaymentDifferencesComponent } from './modules/admin/accounting/payment-differences/payment-differences.component';
import { PaymentDifferencesResolver } from './modules/admin/accounting/payment-differences/payment-differences.resolver';
import { CancellationsComponent } from './modules/admin/registration/cancellations/cancellations.component';
import { CancellationsResolver } from './modules/admin/registration/cancellations/cancellations.resolver';
import { PayoutsComponent } from './modules/admin/accounting/payouts/payouts.component';
import { PayoutsResolver } from './modules/admin/accounting/payouts/payouts.resolver';
import { AllParticipantsComponent } from './modules/admin/registrations/all-participants/all-participants.component';
import { AllParticipantsResolver } from './modules/admin/registrations/all-participants/all-participants.resolver';
import { SetupEventComponent } from './modules/admin/setup-event/setup-event.component';
import { SetupEventResolver } from './modules/admin/setup-event/setup-event.resolver';
import { EventSettingsComponent } from './modules/admin/event-settings/event-settings.component';

// @formatter:off
/* eslint-disable max-len */
/* eslint-disable @typescript-eslint/explicit-function-return-type */
export const appRoutes: Route[] =
    [
        // Redirect empty path to '/select-event'
        { path: '', pathMatch: 'full', redirectTo: '/select-event' },

        // Redirect signed in user to the '/select-event'
        //
        // After the user signs in, the sign in page will redirect the user to the 'signed-in-redirect'
        // path. Below is another redirection for that path to redirect the user to the desired
        // location. This is a small convenience to keep all main routes together here on this file.
        { path: 'signed-in-redirect', pathMatch: 'full', redirectTo: '/select-event' },

        // Auth routes for guests
        {
            path: '',
            canActivate: [NoAuthGuard],
            canActivateChild: [NoAuthGuard],
            component: LayoutComponent,
            data: {
                layout: 'empty'
            },
            children: [
                { path: 'confirmation-required', loadChildren: () => import('app/modules/auth/confirmation-required/confirmation-required.module').then(m => m.AuthConfirmationRequiredModule) },
                { path: 'forgot-password', loadChildren: () => import('app/modules/auth/forgot-password/forgot-password.module').then(m => m.AuthForgotPasswordModule) },
                { path: 'reset-password', loadChildren: () => import('app/modules/auth/reset-password/reset-password.module').then(m => m.AuthResetPasswordModule) },
                { path: 'sign-in', loadChildren: () => import('app/modules/auth/sign-in/sign-in.module').then(m => m.AuthSignInModule) },
                { path: 'sign-up', loadChildren: () => import('app/modules/auth/sign-up/sign-up.module').then(m => m.AuthSignUpModule) }
            ]
        },

        // Auth routes for authenticated users
        {
            path: '',
            canActivate: [AuthGuard],
            canActivateChild: [AuthGuard],
            component: LayoutComponent,
            data: {
                layout: 'empty'
            },
            children: [
                { path: 'sign-out', loadChildren: () => import('app/modules/auth/sign-out/sign-out.module').then(m => m.AuthSignOutModule) },
                { path: 'unlock-session', loadChildren: () => import('app/modules/auth/unlock-session/unlock-session.module').then(m => m.AuthUnlockSessionModule) }
            ]
        },

        {
            path: 'select-event',
            canActivate: [AuthGuard],
            canActivateChild: [AuthGuard],
            component: LayoutComponent,
            // data: { layout: 'empty' },
            children: [
                {
                    path: '',
                    canActivate: [AuthGuard],
                    component: SelectEventComponent,
                    resolve: { initialData: SelectEventResolver },
                }]
        },
        {
            path: ':eventAcronym',
            canActivate: [AuthGuard],
            canActivateChild: [AuthGuard],
            resolve: { initialData: EventAcronymResolver },

            children: [

                // Redirect empty path to '/overview'
                { path: '', pathMatch: 'full', redirectTo: 'overview' },


                // Landing routes
                {
                    path: '',
                    component: LayoutComponent,
                    data: {
                        layout: 'empty'
                    },
                    children: [
                        { path: 'home', loadChildren: () => import('app/modules/landing/home/home.module').then(m => m.LandingHomeModule) },
                    ]
                },

                // mail preview (no layout)
                {
                    path: 'mail-template-preview/:templateId',
                    canActivate: [AuthGuard],
                    component: AutoMailPreviewComponent,
                    resolve: { initialData: AutoMailPreviewResolver }
                },

                // mail view (no layout)
                {
                    path: 'mail-viewer/:mailId',
                    canActivate: [AuthGuard],
                    component: MailViewerComponent,
                    resolve: { initialData: MailViewerResolver }
                },

                // Admin routes
                {
                    path: '',
                    canActivate: [AuthGuard],
                    canActivateChild: [AuthGuard],
                    component: LayoutComponent,
                    resolve: { initialData: InitialDataResolver },
                    children: [
                        {
                            path: 'overview',
                            children: [
                                { path: '', component: OverviewComponent, resolve: { initialData: OverviewResolver } },
                                { path: ':id/double/participants', component: ParticipantsDoubleComponent, resolve: { initialData: ParticipantsResolver } },
                                { path: ':id/single/participants', component: ParticipantsSingleComponent, resolve: { initialData: ParticipantsResolver } },
                            ]
                        }
                    ]
                },
                {
                    path: 'accounting',
                    canActivate: [AuthGuard],
                    canActivateChild: [AuthGuard],
                    component: LayoutComponent,
                    resolve: { initialData: InitialDataResolver },
                    children: [
                        {
                            path: 'bank-statements',
                            canActivate: [AuthGuard],
                            component: BankStatementsComponent,
                            resolve: { initialData: BankStatementsResolver }
                        },
                        {
                            path: 'settle-payments',
                            canActivate: [AuthGuard],
                            component: SettlePaymentsComponent,
                            resolve: { initialData: SettlePaymentsResolver },
                            children: [
                                {
                                    path: ':id',
                                    component: SettlePaymentComponent,
                                    resolve: { initialData: SettlePaymentResolver }
                                }
                            ]
                        },
                        {
                            path: 'due-payments',
                            canActivate: [AuthGuard],
                            component: DuePaymentsComponent,
                            resolve: { initialData: DuePaymentsResolver }
                        },
                        {
                            path: 'payment-differences',
                            canActivate: [AuthGuard],
                            component: PaymentDifferencesComponent,
                            resolve: { initialData: PaymentDifferencesResolver }
                        },
                        {
                            path: 'payouts',
                            canActivate: [AuthGuard],
                            component: PayoutsComponent,
                            resolve: { initialData: PayoutsResolver }
                        },
                    ]
                },
                {
                    path: 'registrations',
                    canActivate: [AuthGuard],
                    canActivateChild: [AuthGuard],
                    component: LayoutComponent,
                    resolve: { initialData: InitialDataResolver },
                    children: [
                        {
                            path: 'search-registration',
                            canActivate: [AuthGuard],
                            component: SearchRegistrationComponent,
                            resolve: { initialData: SearchRegistrationResolver }
                        },
                        {
                            path: 'match-partners',
                            canActivate: [AuthGuard],
                            canActivateChild: [AuthGuard],
                            component: MatchPartnersComponent,
                            resolve: { initialData: MatchPartnersResolver },
                            children: [
                                {
                                    path: ':id',
                                    component: MatchPartnerComponent
                                }
                            ]
                        },
                        {
                            path: 'remarks-overview',
                            canActivate: [AuthGuard],
                            component: RemarksOverviewComponent,
                            resolve: { initialData: RemarksOverviewResolver }
                        },
                        {
                            path: 'notes-overview',
                            canActivate: [AuthGuard],
                            component: NotesOverviewComponent,
                            resolve: { initialData: NotesOverviewResolver }
                        },
                        {
                            path: 'cancellations',
                            canActivate: [AuthGuard],
                            component: CancellationsComponent,
                            resolve: { initialData: CancellationsResolver }
                        },
                        {
                            path: 'all-participants',
                            canActivate: [AuthGuard],
                            component: AllParticipantsComponent,
                            resolve: { initialData: AllParticipantsResolver }
                        },
                        {
                            path: ':id',
                            component: RegistrationComponent,
                            resolve: { initialData: RegistrationResolver }
                        }
                    ]
                },
                {
                    path: 'mailing',
                    canActivate: [AuthGuard],
                    canActivateChild: [AuthGuard],
                    component: LayoutComponent,
                    resolve: { initialData: InitialDataResolver },
                    children: [
                        {
                            path: 'auto-mail-templates',
                            canActivate: [AuthGuard],
                            component: AutoMailTemplatesComponent,
                            resolve: { initialData: AutoMailTemplatesResolver },
                            children: [
                                {
                                    path: ':id',
                                    component: AutoMailTemplateComponent,
                                    resolve: { initialData: AutoMailTemplateResolver }
                                }
                            ]
                        },
                        {
                            path: 'bulk-mail-templates',
                            canActivate: [AuthGuard],
                            component: BulkMailTemplatesComponent,
                            resolve: { initialData: BulkMailTemplatesResolver },
                            children: [
                                {
                                    path: ':id',
                                    component: BulkMailTemplateComponent,
                                    resolve: { initialData: BulkMailTemplateResolver }
                                }
                            ]
                        },
                        {
                            path: 'release-mails',
                            canActivate: [AuthGuard],
                            component: ReleaseMailsComponent,
                            resolve: { initialData: ReleaseMailsResolver },
                            children: [
                                {
                                    path: ':id',
                                    component: MailViewComponent,
                                    resolve: { initialData: MailViewResolver }
                                }
                            ]
                        },
                        {
                            path: 'problematic-emails',
                            canActivate: [AuthGuard],
                            component: ProblematicEmailsComponent,
                            resolve: { initialData: ProblematicEmailsResolver }
                        }
                    ]
                },
                {
                    path: 'hosting',
                    canActivate: [AuthGuard],
                    canActivateChild: [AuthGuard],
                    component: LayoutComponent,
                    resolve: { initialData: InitialDataResolver },
                    children: [
                        {
                            path: '',
                            canActivate: [AuthGuard],
                            component: HostingOverviewComponent,
                            resolve: { initialData: HostingOverviewResolver }
                        },
                    ]
                },
                {
                    path: 'admin',
                    canActivate: [AuthGuard],
                    canActivateChild: [AuthGuard],
                    component: LayoutComponent,
                    resolve: { initialData: InitialDataResolver },
                    children: [
                        {
                            path: 'event-settings',
                            canActivate: [AuthGuard],
                            component: EventSettingsComponent,
                            resolve: { initialData: EventSettingsResolver }
                        },
                        {
                            path: 'setup-event',
                            canActivate: [AuthGuard],
                            component: SetupEventComponent,
                            resolve: { initialData: SetupEventResolver }
                        },
                        {
                            path: 'form-mapping',
                            canActivate: [AuthGuard],
                            component: FormMappingComponent,
                            resolve: { initialData: FormMappingResolver }
                        },
                        {
                            path: 'pricing',
                            canActivate: [AuthGuard],
                            component: PricingComponent,
                            resolve: { initialData: PricingResolver }
                        }
                    ]
                }]
        }
    ];
