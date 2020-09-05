import { Component } from "@angular/core";
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: "paymentReminders",
  templateUrl: "./paymentReminders.component.html"
})
export class PaymentRemindersComponent {
  public dueRegistrations: Registration[];
  public withholdMails: boolean = true;

  constructor(private readonly http: HttpClient, private route: ActivatedRoute) {
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  ngOnInit() {
    this.withholdMails = true;
    this.http.get<Registration[]>(`api/events/${this.getEventAcronym()}/duepayments`)
      .subscribe(result => { this.dueRegistrations = result; },
        error => console.error(error));
  }

  sendReminder(registrationId: string, level: number) {
    var url = `api/events/${this.getEventAcronym()}/registrations/${registrationId}/sendReminder`;
    if (this.withholdMails) {
      url += "?withholdMail=true";
    }

    this.http.post(url, null)
      .subscribe(result => {
        var registration = this.dueRegistrations.find(reg => reg.id === registrationId);

        if (registration != null && level === 1) {
          registration.reminder1Due = false;
        }
        if (registration != null && level === 2) {
          registration.reminder2Due = false;
        }
      },
        error => console.error(error));
  }

  sendSmsReminder(registrationId: string) {
    var url = `api/events/${this.getEventAcronym()}/registrations/${registrationId}/sendReminderSms`;

    this.http.post(url, null)
      .subscribe(result => {
        var registration = this.dueRegistrations.find(reg => reg.id === registrationId);

        if (registration != null) {
          registration.reminderSmsSent = new Date(Date.now());
        }
      },
        error => console.error(error));
  }
}

class Registration {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  price: number;
  receivedAt: Date;
  reminderLevel: number;
  paid: number;
  acceptedMail: Mail;
  reminder1Mail: Mail;
  reminder1Due: boolean;
  reminder2Due: boolean;
  reminderSmsSent: Date;
  phoneNormalized: string;
  reminderSmsPossible: boolean;
}

class Mail {
  id: string;
  sent: Date;
}
