import { Component, Inject, } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'unrecognizedPayments',
    templateUrl: './unrecognizedPayments.component.html'
})
export class UnrecognizedPaymentsComponent {
    public payments: Payment[];
    eventId: string = '762A93A4-56E0-402C-B700-1CFB3362B39D';
    constructor(private http: Http, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
    }

    ngOnInit() {
        //var eventId = this.route.snapshot.params['eventId'];
        this.http.get(this.baseUrl + 'api/event/' + this.eventId + '/payments?unrecognized')
            .subscribe(result => { this.payments = result.json() as Payment[]; },
                                 error => console.error(error));
    }

    saveMail(payment: Payment) {
        console.log("saveMail called " + payment.Id);
        this.http.post(this.baseUrl + 'api/payments/' + payment.Id + '/RecognizedEmail', payment.RecognizedEmail)
                 .subscribe(result => {},
                                      error => console.error(error));
        //payment.RecognizedEmail = "test";
        //alert("test");
    }
}



interface Payment {
    Id : string;
    Amount: number;
    Assigned: number;
    BookingDate: Date,
    Currency: string;
    Info: string;
    Reference: string;
    Repaid: number;
    Settled: boolean;
    RecognizedEmail: string;
}