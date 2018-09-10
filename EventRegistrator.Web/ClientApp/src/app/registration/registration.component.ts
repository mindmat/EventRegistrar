import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Params } from '@angular/router';

@Component({
  selector: 'registration',
  templateUrl: './registration.component.html'
})
export class RegistrationComponent {
  registration: Registration;
  mails: Mail[];
  mail: Mail;
  spots: Spot[];
  allRegistrables: Registrable[];
  private registrationId: string;
  private bookedRegistrableIds: string[];

  constructor(private readonly http: HttpClient, private route: ActivatedRoute) {
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
    let url = `api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/Cancel?reason=${reason}`;
    if (ignorePayments) {
      url += "&ignorePayments=true";
    }
    if (refundPercentage > 0) {
      url += `&refundPercentage=${refundPercentage % 100}`;
    }
    if (preventPromotion) {
      url += `&preventPromotion=true`;
    }
    this.http.post(url, null)
      .subscribe(result => { this.reloadRegistration(); }, error => console.error(error));
  }

  showMail(mail: Mail) {
    this.mail = mail;
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
    var url = `api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/setWaitingListFallback`;
    this.http.post(url, null)
      .subscribe(result => { this.reloadRegistration(); }, error => console.error(error));
  }

  addRegistrable(registrableId: string) {
    var url = `api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/spots/${registrableId}`;
    this.http.put(url, null)
      .subscribe(result => { this.reloadSpots(); this.reloadRegistration(); }, error => console.error(error));
  }

  removeRegistrable(registrableId: string) {
    var url = `api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/spots/${registrableId}`;
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

  composeAndSendMail(withhold: boolean, allowDuplicate: boolean) {
    var url = `api/events/${this.getEventAcronym()}/registrations/${this.registration.id}/ComposeAndSendMail?`;
    if (withhold) {
      url += "&withhold=true";
    }
    if (allowDuplicate) {
      url += "&allowDuplicate=true";
    }

    this.http.post(url, null)
      .subscribe(result => { this.reloadMails(); }, error => console.error(error));
  }

  loadRegistration(registrationId: string) {
    this.registrationId = registrationId;
    this.reloadRegistration();
    this.reloadMails();
    this.reloadSpots();
  }
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
}

class Spot {
  id: string;
  registrableId: string;
  registrable: string;
  partnerRegistrationId: string;
  firstPartnerJoined: Date;
  partner: string;
  isCore: boolean;
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
