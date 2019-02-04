import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Params } from '@angular/router';
import { Guid } from "../infrastructure/guid";
import 'bootstrap/js/dist/dropdown'

@Component({
  selector: 'registration',
  templateUrl: './registration.component.html'
})
export class RegistrationComponent {
  registration: Registration;
  mailTypes: MailType[];
  mails: Mail[];
  mail: Mail;
  spots: Spot[];
  payments: AssignedPayments[];
  allRegistrables: Registrable[];
  selectedMailType: number;

  private registrationId: string;
  private bookedRegistrableIds: string[];

  constructor(private readonly http: HttpClient, private readonly route: ActivatedRoute) {
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  ngOnInit() {
    this.http.get<Registrable[]>(`api/events/${this.getEventAcronym()}/registrables`).subscribe(result => {
      this.allRegistrables = result;
      if (this.spots != null) {
        this.changeAvailability();
      }
    }, error => console.error(error));

    this.loadRegistration(this.route.snapshot.params['id']);

    this.route.params.forEach((params: Params) => {
      this.loadRegistration(params["id"]);
    });
  }

  reloadRegistration() {
    this.http.get<Registration>(`api/events/${this.getEventAcronym()}/registrations/${this.registrationId}`).subscribe(result => {
      this.registration = result;
    }, error => console.error(error));
  }

  reloadMails() {
    this.http.get<Mail[]>(`api/events/${this.getEventAcronym()}/registrations/${this.registrationId}/mails`).subscribe(result => {
      this.mails = result;
      this.mails.forEach(mail => mail.eventsText = mail.events.map(mev => `${mev.when}: ${mev.stateText} (${mev.email})`).reduce((sum, currrent) => sum + "\r" + currrent));
    }, error => console.error(error));

    this.http.get<MailType[]>(`api/events/${this.getEventAcronym()}/registrations/${this.registrationId}/possibleMailTypes`).subscribe(result => {
      this.mailTypes = result;
      if (this.mailTypes.length > 0) {
        this.selectedMailType = this.mailTypes[0].type;
      }
    }, error => console.error(error));
  }

  reloadPayments() {
    this.http.get<AssignedPayments[]>(`api/events/${this.getEventAcronym()}/registrations/${this.registrationId}/AssignedPayments`).subscribe(result => {
      this.payments = result;
    }, error => console.error(error));
  }

  reloadSpots() {
    this.http.get<Spot[]>(`api/events/${this.getEventAcronym()}/registrations/${this.registrationId}/spots`).subscribe(result => {
      this.spots = result;
      this.bookedRegistrableIds = this.spots.map(spot => spot.registrableId);

      if (this.allRegistrables != null) {
        this.changeAvailability();
      }
    }, error => console.error(error));
  }

  cancelRegistration(reason: string, ignorePayments: boolean, refundPercentage: number, preventPromotion: boolean) {
    console.log(`cancel registration ${this.registration.id}, reason ${reason}, ignorePayments ${ignorePayments}, refundPercentage ${refundPercentage}, preventPromotion ${preventPromotion}`);
    this.registration.status = 4; // cancelled
    let url = `api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/?reason=${reason}`;
    if (ignorePayments) {
      url += "&ignorePayments=true";
    }
    if (refundPercentage > 0) {
      url += `&refundPercentage=${refundPercentage}`;
    }
    if (preventPromotion) {
      url += `&preventPromotion=true`;
    }
    this.http.delete(url)
      .subscribe(result => { this.reloadRegistration(); }, error => console.error(error));
  }

  showMail(mail: Mail) {
    this.mail = mail;
  }

  swapFirstLastName() {
    this.http.post(`api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/swapFirstLastName`, null)
      .subscribe(result => { this.reloadRegistration(); }, error => console.error(error));
  }

  releaseMail(mailId: string) {
    var url = `api/events/${this.getEventAcronym()}/mails/${mailId}/release`;
    this.http.post(url, null)
      .subscribe(result => { this.reloadMails(); }, error => console.error(error));
  }

  deleteMail(mailId: string) {
    var url = `api/events/${this.getEventAcronym()}/mails/${mailId}`;
    this.http.delete(url)
      .subscribe(result => { this.reloadMails(); }, error => console.error(error));
  }

  fallbackToPartyPass() {
    this.http.put(`api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/setWaitingListFallback`, null)
      .subscribe(result => { this.reloadRegistration(); }, error => console.error(error));
  }

  addRegistrable(registrableId: string, asFollower: true) {
    var url = `api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/spots/${registrableId}`;
    if (asFollower) {
      url += '?asFollower=true';
    }
    this.http.put(url, null)
      .subscribe(result => { this.reloadSpots(); this.reloadRegistration(); }, error => console.error(error));
  }

  removeRegistrable(registrableId: string) {
    var url = `api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/spots/${registrableId}`;
    this.http.delete(url)
      .subscribe(result => { this.reloadSpots(); this.reloadRegistration(); }, error => console.error(error));
  }

  unassignPayment(paymentAssignmentId: string) {
    var url = `api/events/${this.getEventAcronym()}/paymentAssignments/${paymentAssignmentId}`;
    this.http.delete(url)
      .subscribe(result => { this.reloadSpots(); this.reloadRegistration(); }, error => console.error(error));
  }

  changeAvailability() {
    for (let registrable of this.allRegistrables) {
      if (this.bookedRegistrableIds.indexOf(registrable.id) >= 0) {
        registrable.addAvailable = false;
        registrable.removeAvailable = true;
      } else {
        registrable.addAvailable = true;
        registrable.removeAvailable = false;
      }
    }
  }

  createMail(mailType: number, withhold: boolean, allowDuplicate: boolean) {
    var url = `api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/mails/create?mailType=${mailType}`;
    if (withhold) {
      url += "&withhold=true";
    }
    if (allowDuplicate) {
      url += "&allowDuplicate=true";
    }

    this.http.post(url, null)
      .subscribe(result => { this.reloadMails(); }, error => console.error(error));
  }

  setReducedPrice() {
    this.http.put(`api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/setReducedPrice`, null)
      .subscribe(result => { }, error => console.error(error));
  }

  initNewReduction() {
    this.newReductionId = Guid.newGuid();
  }

  addReduction(amount: number, reason: string) {
    var url = `api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/reductions/${this.newReductionId}?amount=${amount}`;
    if (reason != null && reason !== "") {
      url += `&reason=${reason}`;
    }

    this.http.post(url, null)
      .subscribe(result => { this.reloadRegistration(); },
        error => console.error(error));
  }

  loadRegistration(registrationId: string) {
    this.registrationId = registrationId;
    this.reloadRegistration();
    this.reloadMails();
    this.reloadSpots();
    this.reloadPayments();
  }

  newReductionId: string;
}

class Registration {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  language: string;
  isWaitingList: boolean;
  price: number;
  paid: number;
  status: number;
  statusText: string;
  receivedAt: Date;
  reminderLevel: number;
  soldOutMessage: string;
  fallbackToPartyPass: boolean;
  smsCount: number;
  remarks: string;
  phoneNormalized: string;
  partnerOriginal: string;
  partnerId: string;
  isReduced: boolean;
}

class Spot {
  id: string;
  registrableId: string;
  registrable: string;
  partnerRegistrationId: string;
  firstPartnerJoined: Date;
  partner: string;
  isCore: boolean;
  isWaitingList: boolean;
}

class Mail {
  id: string;
  senderMail: string;
  senderName: string;
  subject: string;
  recipients: string;
  created: Date;
  withhold: boolean;
  contentHtml: string;
  state: string;
  eventsText: string;
  events: MailEvent[];
}

class MailEvent {
  when: Date;
  email: string;
  state: number;
  stateText: string;
}

class Registrable {
  id: string;
  name: string;
  hasWaitingList: boolean;
  isDoubleRegistrable: boolean;
  showInMailListOrder: number;
  addAvailable: boolean;
  removeAvailable: boolean;
}

class AssignedPayments {
  paymentAssignmentId: string;
  amount: number;
  bookingDate: Date;
  currency: string;
  paymentAssignmentId_Counter: string;
}

class MailType {
  type: number;
  userText: string;
}
