import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './components/app/app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';

import { RegistrablesComponent } from './components/registrables/registrables.component';
import { ParticipantsComponent } from './components/participants/participants.component';
import { HostingComponent } from './components/hosting/hosting.component';
import { UnrecognizedPaymentsComponent } from './components/unrecognizedPayments/unrecognizedPayments.component';
import { PaymentOverviewComponent } from './components/paymentOverview/paymentOverview.component';
import { RegistrationComponent } from './components/registration/registration.component';
import { SearchRegistrationComponent } from './components/registration/searchRegistration.component';

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        RegistrablesComponent,
        ParticipantsComponent,
        HostingComponent,
        HomeComponent,
        UnrecognizedPaymentsComponent,
        PaymentOverviewComponent,
        RegistrationComponent,
        SearchRegistrationComponent
    ],
    imports: [
        CommonModule,
        HttpModule,
        FormsModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'registrables', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'registrables', component: RegistrablesComponent },
            { path: 'registrables/:id/participants', component: ParticipantsComponent },
            { path: 'registration/:id', component: RegistrationComponent },
            { path: 'searchRegistration', component: SearchRegistrationComponent },
            { path: 'hosting', component: HostingComponent },
            { path: 'unrecognizedPayments', component: UnrecognizedPaymentsComponent },
            { path: 'paymentOverview', component: PaymentOverviewComponent },
            { path: '**', redirectTo: 'registrables' }
        ])
    ]
})
export class AppModuleShared {
}