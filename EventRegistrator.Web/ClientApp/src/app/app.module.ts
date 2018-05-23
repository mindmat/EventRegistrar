import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
//import { HomeComponent } from './components/home/home.component';

import { RegistrablesComponent } from './registrables/registrables.component';
//import { ParticipantsComponent } from './components/participants/participants.component';
//import { HostingComponent } from './components/hosting/hosting.component';
//import { UnrecognizedPaymentsComponent } from './components/unrecognizedPayments/unrecognizedPayments.component';
//import { PaymentOverviewComponent } from './components/paymentOverview/paymentOverview.component';
//import { RegistrationComponent } from './components/registration/registration.component';
//import { SearchRegistrationComponent } from './components/registration/searchRegistration.component';
//import { PaymentRemindersComponent } from './components/paymentReminders/paymentReminders.component';
//import { SmsConversationComponent } from './components/smsConversation/smsConversation.component';
//import { CheckinViewComponent } from "./components/checkinView/checkinView.component";
//import { PartyOverviewComponent } from "./components/partyOverview/partyOverview.component";
//import { MailTemplatesComponent } from "./components/mailTemplates/mailTemplates.component";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    RegistrablesComponent,
    //ParticipantsComponent,
    //HostingComponent,
    //HomeComponent,
    //UnrecognizedPaymentsComponent,
    //PaymentOverviewComponent,
    //RegistrationComponent,
    //SearchRegistrationComponent,
    //PaymentRemindersComponent,
    //SmsConversationComponent,
    //CheckinViewComponent,
    //PartyOverviewComponent,
    //MailTemplatesComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', redirectTo: 'll18/registrables', pathMatch: 'full' },
      //{ path: 'home', component: HomeComponent },
      { path: ':eventAcronym/registrables', component: RegistrablesComponent },
      //{ path: 'registrables/:id/participants', component: ParticipantsComponent },
      //{ path: 'registration/:id', component: RegistrationComponent },
      //{ path: 'searchRegistration', component: SearchRegistrationComponent },
      //{ path: 'hosting', component: HostingComponent },
      //{ path: 'unrecognizedPayments', component: UnrecognizedPaymentsComponent },
      //{ path: 'paymentOverview', component: PaymentOverviewComponent },
      //{ path: 'paymentReminders', component: PaymentRemindersComponent },
      //{ path: 'checkinView', component: CheckinViewComponent },
      //{ path: 'registrations/:id/sms', component: SmsConversationComponent },
      //{ path: 'partyOverview', component: PartyOverviewComponent },
      //{ path: 'mailTemplates', component: MailTemplatesComponent },
      { path: '**', redirectTo: 'll18/registrables' }
    ])
  ],
  providers: [
    { provide: 'BASE_URL', useFactory: getBaseUrl }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

export function getBaseUrl() {
  return 'https://eventregistrator.azurewebsites.net/'; //  document.getElementsByTagName('base')[0].href;
}
