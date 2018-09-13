import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'unrecognizedPayments',
  templateUrl: './unrecognizedPayments.component.html'
})
export class UnrecognizedPaymentsComponent implements OnInit {
  public payments: Payment[];
  paymentPointer: number;
  payment: Payment;

  possibleAssignments: PossibleAssignment[];
  isSearching: boolean;

  constructor(private http: HttpClient, private route: ActivatedRoute) {
    this.paymentPointer = 0;
  }

  ngOnInit() {
    this.http.get<Payment[]>(`api/events/${this.getEventAcronym()}/payments/unrecognized`)
      .subscribe(result => { this.payments = result; this.setPayment(this.payments[0]) },
        error => console.error(error));
    //console.log($('#paymentsCarousel'));
    //$('#paymentsCarousel').bind('slid.bs.carousel',
    //  function(e) {
    //    console.log('before');
    //  });
  }

  gotoPreviousPayment() {
    if (this.paymentPointer > 0) {
      console.log(this.paymentPointer);
      this.setPayment(this.payments[--this.paymentPointer]);

    }
  }

  gotoNextPayment() {
    if (this.paymentPointer < this.payments.length - 1) {
      console.log(this.paymentPointer);
      this.setPayment(this.payments[++this.paymentPointer]);
    }
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  saveMail(payment: Payment) {
    payment.locked = true;
    this.http.post(`api/events/${this.getEventAcronym()}/payments/${payment.id}/RecognizedEmail`, payment.recognizedEmail)
      .subscribe(result => { },
        error => {
          console.error(error);
          payment.locked = false;
        });
  }

  textSelected(payment: Payment) {
    var selection = document.getSelection().toString();
    console.log(`selection: ${selection}`);

    if (selection.length > 3) {
      this.searchRegistration(selection, payment);
    }
  }


  searchRegistration(searchString: string, payment: Payment) {
    this.isSearching = true;
    this.http.get<PossibleAssignment[]>(`api/events/${this.getEventAcronym()}/payments/${payment.id}/possibleAssignments`) //?searchstring=${searchString}`)
      .subscribe(result => {
        this.possibleAssignments = result;
        this.isSearching = false;
      },
        error => console.error(error));
  }

  setPayment(payment: Payment) {
    this.payment = payment;
    this.searchRegistration("", this.payment);
  }
}

class Payment {
  id: string;
  amount: number;
  assigned: number;
  bookingDate: Date;
  currency: string;
  info: string;
  reference: string;
  repaid: number;
  settled: boolean;
  recognizedEmail: string;
  locked: boolean;
}

class Registration {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  language: string;
  responses: Response[];
  responsesJoined: string;
  price: number;
}

class Response {
  response: string;
  question: string;
}

class PossibleAssignment {
  amount: number;
  firstName: string;
  lastName: string;
  paymentId: string;
  registrationId: string;
}
