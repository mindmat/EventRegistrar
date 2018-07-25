import { Component, Inject, } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { HostListener } from '@angular/core';

@Component({
  selector: 'unrecognizedPayments',
  templateUrl: './unrecognizedPayments.component.html'
})
export class UnrecognizedPaymentsComponent {
  public payments: Payment[];
  registrations: Registration[];
  isSearching: boolean;
  eventId: string = '762A93A4-56E0-402C-B700-1CFB3362B39D';

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.http.get<Payment[]>(`api/events/${this.getEventAcronym()}/payments?unrecognized=true`)
      .subscribe(result => { this.payments = result; },
        error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  saveMail(payment: Payment) {
    payment.Locked = true;
    this.http.post(`api/events/${this.getEventAcronym()}/payments/${payment.Id}/RecognizedEmail`, payment.RecognizedEmail)
      .subscribe(result => { },
        error => {
          console.error(error);
          payment.Locked = false;
        });
  }

  textSelected() {
    var selection = document.getSelection().toString();
    console.log(`selection: ${selection}`);

    if (selection.length > 3) {
      this.searchRegistration(selection);
    }
  }

  searchRegistration(searchString: string) {
    this.isSearching = true;
    this.http.get<Registration[]>(`api/event/${this.getEventAcronym()}/registrations?searchstring=${searchString}`)
      .subscribe(result => {
        this.registrations = result;
        this.registrations.map(reg => reg.ResponsesJoined =
          reg.Responses.map(rsp => `${rsp.Question} = ${rsp.Response}`).reduce((agg, line) => `${agg} / ${line}`));
        this.isSearching = false;
      },
        error => console.error(error));
  }
}

interface Payment {
  Id: string;
  Amount: number;
  Assigned: number;
  BookingDate: Date,
  Currency: string;
  Info: string;
  Reference: string;
  Repaid: number;
  Settled: boolean;
  RecognizedEmail: string;
  Locked: boolean;
}

interface Registration {
  Id: string;
  Email: string;
  FirstName: string;
  LastName: string;
  Language: string;
  Responses: Response[];
  ResponsesJoined: string;
  Price: number;
}

interface Response {
  Response: string;
  Question: string;
}
