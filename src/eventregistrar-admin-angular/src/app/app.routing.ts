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

// @formatter:off
/* eslint-disable max-len */
/* eslint-disable @typescript-eslint/explicit-function-return-type */
export const appRoutes: Route[] = [

    // Redirect empty path to '/overview'
    { path: '', pathMatch: 'full', redirectTo: 'overview' },

    // Redirect signed in user to the '/overview'
    //
    // After the user signs in, the sign in page will redirect the user to the 'signed-in-redirect'
    // path. Below is another redirection for that path to redirect the user to the desired
    // location. This is a small convenience to keep all main routes together here on this file.
    { path: 'signed-in-redirect', pathMatch: 'full', redirectTo: 'overview' },

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

    // Admin routes
    {
        path: '',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        component: LayoutComponent,
        resolve: {
            initialData: InitialDataResolver,
        },
        children: [
            {
                path: 'overview',
                children: [
                    { path: '', component: OverviewComponent, resolve: { initialData: OverviewResolver } },
                    { path: ':id/double/participants', component: ParticipantsDoubleComponent, resolve: { initialData: ParticipantsResolver } },
                    { path: ':id/single/participants', component: ParticipantsSingleComponent, resolve: { initialData: ParticipantsResolver } },
                ]
            },
            {
                path: 'registration/:id', canActivate: [AuthGuard], component: RegistrationComponent, resolve: { initialData: RegistrationResolver }
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
                resolve: { initialData: SettlePaymentsResolver }
            }
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
                path: 'search-registration', canActivate: [AuthGuard], component: SearchRegistrationComponent
            }
        ]
    },
];
