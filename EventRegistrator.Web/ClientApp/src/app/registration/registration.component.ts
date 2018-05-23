import { Component, Inject, } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'registration',
  templateUrl: './registration.component.html'
})
export class RegistrationComponent {
  public registration: Registration;
  public mails: Mail[];
  public spots: Spot[];
  private registrationId: string;
  public allRegistrables: Registrable[];
  private bookedRegistrableIds: string[];

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.registrationId = this.route.snapshot.params['id'];

    const eventId = "762A93A4-56E0-402C-B700-1CFB3362B39D";
    this.http.get<Registrable[]>(`${this.baseUrl}api/events/${eventId}/registrables`).subscribe(result => {
      this.allRegistrables = result;
      if (this.spots != null) {
        this.changeAvailability();
      }
    }, error => console.error(error));

    this.reloadRegistration();
    this.reloadMails();
    this.reloadSpots();
  }

  reloadRegistration() {
    this.http.get<Registration>(`${this.baseUrl}api/registrations/${this.registrationId}`).subscribe(result => {
      this.registration = result;
    }, error => console.error(error));
  }

  reloadMails() {
    this.http.get<Mail[]>(`${this.baseUrl}api/registrations/${this.registrationId}/mails`).subscribe(result => {
      this.mails = result;
    }, error => console.error(error));
  }

  reloadSpots() {
    this.http.get<Spot[]>(`${this.baseUrl}api/registrations/${this.registrationId}/spots`).subscribe(result => {
      this.spots = result;
      this.bookedRegistrableIds = this.spots.map(spot => spot.RegistrableId);

      if (this.allRegistrables != null) {
        this.changeAvailability();
      }
    }, error => console.error(error));
  }

  cancelRegistration(reason: string, ignorePayments: boolean, refundPercentage: number, preventPromotion: boolean) {
    console.log(`cancel registration ${this.registration.Id}, reason ${reason}, ignorePayments ${ignorePayments}, refundPercentage ${refundPercentage}, preventPromotion ${preventPromotion}`);
    this.registration.Status = 4; // cancelled
    let url = `${this.baseUrl}api/registration/${this.registration.Id}/Cancel?reason=${reason}`;
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

  showMail(content: string) {
    var mailContainer = document.getElementById("mailContainer") as HTMLDivElement;
    mailContainer.innerHTML = content;
  }

  releaseMail(mailId: string) {
    var url = `${this.baseUrl}api/mails/${mailId}/release`;
    this.http.post(url, null)
      .subscribe(result => { this.reloadMails(); }, error => console.error(error));
  }

  deleteMail(mailId: string) {
    var url = `${this.baseUrl}api/mails/${mailId}/delete`;
    this.http.post(url, null)
      .subscribe(result => { this.reloadMails(); }, error => console.error(error));
  }

  fallbackToPartyPass() {
    var url = `${this.baseUrl}api/registrations/${this.registration.Id}/setWaitingListFallback`;
    this.http.post(url, null)
      .subscribe(result => { this.reloadRegistration(); }, error => console.error(error));
  }

  addRegistrable(registrableId: string) {
    var url = `${this.baseUrl}api/registrations/${this.registration.Id}/addSpot?registrableId=${registrableId}`;
    this.http.post(url, null)
      .subscribe(result => { this.reloadSpots(); this.reloadRegistration(); }, error => console.error(error));
  }

  removeRegistrable(registrableId: string) {
    var url = `${this.baseUrl}api/registrations/${this.registration.Id}/removeSpot?registrableId=${registrableId}`;
    this.http.post(url, null)
      .subscribe(result => { this.reloadSpots(); this.reloadRegistration(); }, error => console.error(error));
  }

  changeAvailability() {
    for (let registrable of this.allRegistrables) {
      if (this.bookedRegistrableIds.indexOf(registrable.Id) >= 0) {
        registrable.addAvailable = false;
        registrable.removeAvailable = true;
      } else {
        registrable.addAvailable = true;
        registrable.removeAvailable = false;
      }
    }
  }

  composeAndSendMail(withhold: boolean, allowDuplicate: boolean) {
    var url = `${this.baseUrl}api/registrations/${this.registration.Id}/ComposeAndSendMail?`;
    if (withhold) {
      url += "&withhold=true";
    }
    if (allowDuplicate) {
      url += "&allowDuplicate=true";
    }

    this.http.post(url, null)
      .subscribe(result => { this.reloadMails(); }, error => console.error(error));
  }
}

interface Registration {
  Id: string;
  Email: string;
  FirstName: string;
  LastName: string;
  Language: string;
  IsWaitingList: boolean;
  Price: number;
  Paid: number;
  Status: number;
  StatusText: string;
  ReceivedAt: Date;
  ReminderLevel: number;
  SoldOutMessage: string;
  FallbackToPartyPass: boolean;
  SmsCount: number;
  Remarks: string;
  PhoneNormalized: string;
}

interface Spot {
  Id: string;
  RegistrableId: string;
  Registrable: string;
  PartnerRegistrationId: string;
  FirstPartnerJoined: Date;
  Partner: string;
  IsCore: boolean;
}

interface Mail {
  Id: string;
  SenderMail: string;
  SenderName: string;
  Subject: string;
  Recipients: string;
  Created: Date;
  Withhold: boolean;
  ContentHtml: string;
}

interface Registrable {
  Id: string;
  Name: string;
  HasWaitingList: boolean;
  IsDoubleRegistrable: boolean;
  ShowInMailListOrder: number;
  addAvailable: boolean;
  removeAvailable: boolean;
}
