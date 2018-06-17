import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { AuthService } from './authentication/authService.service';
//import { HomeComponent } from './home/home.component';

import { RegistrablesComponent } from './registrables/registrables.component';
import { ParticipantsComponent } from './participants/participants.component';
import { HostingComponent } from './hosting/hosting.component';
import { UnrecognizedPaymentsComponent } from './unrecognizedPayments/unrecognizedPayments.component';
import { PaymentOverviewComponent } from './paymentOverview/paymentOverview.component';
import { RegistrationComponent } from './registration/registration.component';
import { SearchRegistrationComponent } from './registration/searchRegistration.component';
import { PaymentRemindersComponent } from './paymentReminders/paymentReminders.component';
import { SmsConversationComponent } from './smsConversation/smsConversation.component';
import { CheckinViewComponent } from "./checkinView/checkinView.component";
import { PartyOverviewComponent } from "./partyOverview/partyOverview.component";
import { MailTemplatesComponent } from "./mailTemplates/mailTemplates.component";
import { TokenInterceptor } from "./authentication/tokenInterceptor";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    RegistrablesComponent,
    ParticipantsComponent,
    HostingComponent,
    //HomeComponent,
    UnrecognizedPaymentsComponent,
    PaymentOverviewComponent,
    RegistrationComponent,
    SearchRegistrationComponent,
    PaymentRemindersComponent,
    SmsConversationComponent,
    CheckinViewComponent,
    PartyOverviewComponent,
    MailTemplatesComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpModule,
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', redirectTo: 'tev/registrables', pathMatch: 'full' },
      //{ path: 'home', component: HomeComponent },
      { path: ':eventAcronym/registrables', component: RegistrablesComponent },
      { path: ':eventAcronym/registrables/:id/participants', component: ParticipantsComponent },
      { path: ':eventAcronym/registration/:id', component: RegistrationComponent },
      { path: ':eventAcronym/searchRegistration', component: SearchRegistrationComponent },
      { path: ':eventAcronym/hosting', component: HostingComponent },
      { path: ':eventAcronym/unrecognizedPayments', component: UnrecognizedPaymentsComponent },
      { path: ':eventAcronym/paymentOverview', component: PaymentOverviewComponent },
      { path: ':eventAcronym/paymentReminders', component: PaymentRemindersComponent },
      { path: ':eventAcronym/checkinView', component: CheckinViewComponent },
      { path: ':eventAcronym/registrations/:id/sms', component: SmsConversationComponent },
      { path: ':eventAcronym/partyOverview', component: PartyOverviewComponent },
      { path: ':eventAcronym/mailTemplates', component: MailTemplatesComponent },
      { path: '**', redirectTo: 'tev/registrables' }
    ])
  ],
  providers: [
    { provide: 'BASE_URL', useFactory: getBaseUrl },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true
    },
    AuthService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

export function getBaseUrl() {
  return 'https://eventregistrator.azurewebsites.net/'; //  document.getElementsByTagName('base')[0].href;
}
