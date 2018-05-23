import { Component, Inject, } from "@angular/core";
import { Http } from "@angular/http";
import { Router, ActivatedRoute } from "@angular/router";

@Component({
    selector: "smsConversation",
    templateUrl: "./smsConversation.component.html",
    styleUrls: ["./smsConversation.component.css"]
})
export class SmsConversationComponent {
    registrationId: string;
    conversation: Sms[];

    constructor(private readonly http: Http,
        @Inject("BASE_URL") private readonly baseUrl: string,
        private readonly router: Router,
        private readonly route: ActivatedRoute) {
    }

    ngOnInit() {
        this.registrationId = this.route.snapshot.params['id'];

        this.refresh();
    }

    refresh() {
        this.http.get(`${this.baseUrl}api/registrations/${this.registrationId}/smsConversation`)
            .subscribe(result => { this.conversation = result.json() as Sms[]; },
            error => console.error(error));
    }

    send(text: string) {
        if (text == null || text === "") {
            return;
        }
        this.http.post(`${this.baseUrl}api/registrations/${this.registrationId}/sendSms`, text)
            .subscribe(result => { this.refresh() },
            error => console.error(error));
    }
}

interface Sms {
    Date: Date;
    Body: string;
    Status: string;
    Sent: boolean;
}
