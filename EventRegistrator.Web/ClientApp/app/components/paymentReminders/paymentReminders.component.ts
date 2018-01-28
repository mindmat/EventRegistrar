import { Component, Inject, } from "@angular/core";
import { Http } from "@angular/http";
import { Router, ActivatedRoute } from "@angular/router";

@Component({
  selector: "paymentReminders",
  templateUrl: "./paymentReminders.component.html"
})
export class PaymentRemindersComponent {
  public dueRegistrations: Registration[];

  constructor(private http: Http, @Inject("BASE_URL") private baseUrl: string, private router: Router, private route: ActivatedRoute) {
  }

  ngOnInit() {
    const eventId = "762A93A4-56E0-402C-B700-1CFB3362B39D";
    const millisecondsPerDay = 1000 * 3600 * 24;
    this.http.get(`${this.baseUrl}api/events/${eventId}/duepayments`).subscribe(result => {
      this.dueRegistrations = result.json() as Registration[];
      for (let registration of this.dueRegistrations) {
        registration.Reminder1Possible = registration.Reminder1Mail == null &&
                                         ((registration.AcceptedMail.Created.valueOf() - registration.ReceivedAt.valueOf()) / millisecondsPerDay) > 5;
      }
    }, error => console.error(error));
  }
}

interface Registration {
  Id: string;
  FirstName: string;
  LastName: string;
  Email: string;
  Price: number;
  ReceivedAt: Date;
  ReminderLevel: number;
  Paid: number;
  AcceptedMail: Mail;
  Reminder1Mail: Mail;
  Reminder1Possible: boolean;
}

interface Mail {
  Id: string;
  Created: Date;
}