import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'unassignedPayments',
  templateUrl: './unassignedPayments.component.html'
})
export class UnassignedPaymentsComponent implements OnInit {
  payments: Payment[];
  paymentPointer: number;
  payment: Payment;

  possibleAssignments: PossibleAssignment[];
  isSearching: boolean;

  constructor(private readonly http: HttpClient, private readonly route: ActivatedRoute) {
    this.paymentPointer = 0;
  }

  ngOnInit() {
    this.http.get<Payment[]>(`api/events/${this.getEventAcronym()}/payments/unassigned`)
      .subscribe(result => {
        this.payments = result;
        this.setPayment(this.payments[0]);
        //for (let payment of this.payments) {
        //  payment.amountUnassigned = payment.amount - payment.amountAssigned;
        //}
      },
        error => console.error(error));
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

  //saveMail(payment: Payment) {
  //  payment.locked = true;
  //  this.http.post(`api/events/${this.getEventAcronym()}/payments/${payment.id}/RecognizedEmail`, payment.recognizedEmail)
  //    .subscribe(result => { },
  //      error => {
  //        console.error(error);
  //        payment.locked = false;
  //      });
  //}

  savePayment(assignment: PossibleAssignment) {
    assignment.locked = true;
    var url = `api/events/${this.getEventAcronym()}/payments/${assignment.paymentId}/assign/${assignment.registrationId}?amount=${assignment.amountToAssign}`;
    if (assignment.acceptDifference) {
      url += `&acceptDifference=${assignment.acceptDifference}`;
      if (assignment.acceptDifferenceReason != null) {
        url += `&acceptDifferenceReason=${assignment.acceptDifferenceReason}`;
      }
    }
    this.http.post(url, null)
      .subscribe(result => { },
        error => {
          console.error(error);
          assignment.locked = false;
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
        for (let possibleAssignment of this.possibleAssignments) {
          possibleAssignment.amountToAssign = Math.min(possibleAssignment.amount - possibleAssignment.amountPaid, payment.amount - payment.amountAssigned);
        }
      },
        error => console.error(error));
  }

  setPayment(payment: Payment) {
    this.payment = payment;
    this.possibleAssignments = null;
    this.searchRegistration("", this.payment);
  }
}

class Payment {
  id: string;
  amount: number;
  amountAssigned: number;
  //amountUnassigned: number;
  bookingDate: Date;
  currency: string;
  info: string;
  reference: string;
  repaid: number;
  settled: boolean;
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
  firstName: string;
  lastName: string;
  paymentId: string;
  registrationId: string;
  amount: number;
  amountPaid: number;
  amountToAssign: number;
  acceptDifference: boolean;
  acceptDifferenceReason: string;
  locked: boolean;
}
