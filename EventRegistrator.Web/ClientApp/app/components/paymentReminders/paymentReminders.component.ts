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
    this.http.get(`${this.baseUrl}api/events/${eventId}/duepayments`).subscribe(result => {
      this.dueRegistrations = result.json() as Registration[];
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
  Reminder1Due: boolean;
  Reminder2Due: boolean;
}

interface Mail {
  Id: string;
  Created: Date;
}