import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { HttpClientModule } from '@angular/common/http';
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
import { EventSelectionComponent } from "./events/eventSelection.component";
import { EventAuthorizationComponent } from "./eventAuthorization/eventAuthorization.component";
import { RegistrationFormsComponent } from "./registrationForms/registrationForms.component";
import { QuestionMappingComponent } from "./questionMapping/questionMapping.component";
import { EventService } from "./events/eventService.service";

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
    MailTemplatesComponent,
    EventSelectionComponent,
    EventAuthorizationComponent,
    RegistrationFormsComponent,
    QuestionMappingComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpModule,
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      //{ path: '', redirectTo: 'eventSelection', pathMatch: 'full' },
      //{ path: 'home', component: HomeComponent },
      { path: '', component: EventSelectionComponent },
      { path: ':eventAcronym/authorization', component: EventAuthorizationComponent },
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
      { path: ':eventAcronym/registrationForms', component: RegistrationFormsComponent },
      { path: ':eventAcronym/questionMapping', component: QuestionMappingComponent }
      //{ path: '**', redirectTo: 'll18/registrables' }
    ])
  ],
  providers: [
    { provide: 'BASE_URL', useFactory: getBaseUrl },
    AuthService,
    EventService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

export function getBaseUrl() {
  return 'https://eventregistrator.azurewebsites.net/'; //  document.getElementsByTagName('base')[0].href;
}
