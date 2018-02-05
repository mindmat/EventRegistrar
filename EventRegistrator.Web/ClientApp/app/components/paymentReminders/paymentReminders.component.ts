import { Component, Inject, } from "@angular/core";
import { Http, URLSearchParams } from "@angular/http";

@Component({
    selector: "paymentReminders",
    templateUrl: "./paymentReminders.component.html"
})
export class PaymentRemindersComponent {
    public dueRegistrations: Registration[];
    public withholdMails: boolean = true;

    constructor(private http: Http,
        @Inject("BASE_URL") private baseUrl: string) {
    }

    ngOnInit() {
        const eventId = "762A93A4-56E0-402C-B700-1CFB3362B39D";
        this.withholdMails = true;
        this.http.get(`${this.baseUrl}api/events/${eventId}/duepayments`)
            .subscribe(result => { this.dueRegistrations = result.json() as Registration[]; },
            error => console.error(error));
    }

    sendReminder(registrationId: string, level: number) {
        var url = `${this.baseUrl}api/registration/${registrationId}/sendReminder`;
        if (this.withholdMails) {
            url += "?withhold=true";
        }

        this.http.post(url, null)
            .subscribe(result => {
                var registration = this.dueRegistrations.find(reg => reg.Id === registrationId);

                if (registration != null && level === 1) {
                    registration.Reminder1Due = false;
                }
                if (registration != null && level === 2) {
                    registration.Reminder2Due = false;
                }
            },
            error => console.error(error));
    }

    sendSmsReminder(registrationId: string) {
        var url = `${this.baseUrl}api/registrations/${registrationId}/sendReminderSms`;

        this.http.post(url, null)
            .subscribe(result => {
                var registration = this.dueRegistrations.find(reg => reg.Id === registrationId);

                if (registration != null) {
                    registration.ReminderSmsSent = new Date(Date.now());
                }
            },
            error => console.error(error));

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
    ReminderSmsSent: Date;
    PhoneNormalized: string;
    ReminderSmsPossible: boolean;
}

interface Mail {
    Id: string;
    Created: Date;
}