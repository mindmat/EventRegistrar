import { Component, Inject, } from '@angular/core';
import { Http } from '@angular/http';
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

    constructor(private http: Http, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
    }

    ngOnInit() {
        this.registrationId = this.route.snapshot.params['id'];

        this.reloadRegistration();
        this.reloadMails();
        this.reloadSpots();
    }

    reloadRegistration() {
        this.http.get(`${this.baseUrl}api/registrations/${this.registrationId}`).subscribe(result => {
            this.registration = result.json() as Registration;
        }, error => console.error(error));
    }

    reloadMails() {
        this.http.get(`${this.baseUrl}api/registrations/${this.registrationId}/mails`).subscribe(result => {
            this.mails = result.json() as Mail[];
        }, error => console.error(error));
    }

    reloadSpots() {
        this.http.get(`${this.baseUrl}api/registrations/${this.registrationId}/spots`).subscribe(result => {
            this.spots = result.json() as Spot[];
        }, error => console.error(error));        
    }

    cancelRegistration(reason: string, ignorePayments: boolean, refundPercentage: number) {
        console.log(`cancel registration ${this.registration.Id}, reason ${reason}, ignorePayments ${ignorePayments}, refundPercentage ${refundPercentage}`);
        this.registration.Status = 4; // cancelled
        var url = `${this.baseUrl}api/registration/${this.registration.Id}/Cancel?reason=${reason}`;
        if (ignorePayments) {
            url += "&ignorePayments=true";
        }
        if (refundPercentage > 0) {
            url += `&refundPercentage=${refundPercentage % 100}`;
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

    fallbackToPartyPass() {
        var url = `${this.baseUrl}api/registrations/${this.registration.Id}/SetWaitingListFallback`;
        this.http.post(url, null)
            .subscribe(result => { this.reloadRegistration(); }, error => console.error(error));
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