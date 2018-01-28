import { Component, Inject, } from "@angular/core";
import { Http } from "@angular/http";
import { Router, ActivatedRoute } from "@angular/router";

@Component({
    selector: "paymentReminders",
    templateUrl: "./paymentReminders.component.html"
})
export class PaymentRemindersComponent {
    public dueRegsitrations: Registration[];

    constructor(private http: Http, @Inject("BASE_URL") private baseUrl: string, private router: Router, private route: ActivatedRoute) {
    }

    ngOnInit() {
        const eventId = "762A93A4-56E0-402C-B700-1CFB3362B39D";
        var registrableId = this.route.snapshot.params["id"];
        this.http.get(`${this.baseUrl}api/events/${eventId}/duepayments`).subscribe(result => {
            this.dueRegsitrations = result.json() as Registration[];
        }, error => console.error(error));
    }
}

interface Registration {
    Id: string;
    FirstName: string;
    LastName: string;
    Price: number;
    ReceivedAt: Date;
    ReminderLevel: number;
    Paid: number;
}