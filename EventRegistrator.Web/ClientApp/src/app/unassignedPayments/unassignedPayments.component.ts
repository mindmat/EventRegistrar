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
  paymentSlipUrl: string;

  possibleAssignments: PossibleAssignment[];
  registrationMatches: PossibleAssignment[];
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
      this.setPayment(this.payments[--this.paymentPointer]);
    }
  }

  gotoNextPayment() {
    if (this.paymentPointer < this.payments.length - 1) {
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
    var url = `api/events/${this.getEventAcronym()}/payments/${assignment.paymentId != null ? assignment.paymentId : this.payment.id}/assign/${assignment.registrationId}?amount=${assignment.amountToAssign}`;
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

  textSelected() {
    var selection = document.getSelection().toString();
    console.log(`selection: ${selection}`);

    if (selection.length > 3) {
      this.searchRegistrationManually(selection);
    } else {
      this.searchRegistration(this.payment);
    }
  }

  searchRegistration(payment: Payment) {
    this.isSearching = true;
    this.http.get<PossibleAssignment[]>(`api/events/${this.getEventAcronym()}/payments/${payment.id}/possibleAssignments`) //?searchstring=${searchString}`)
      .subscribe(result => {
        this.setAssignments(result);
        this.isSearching = false;
        this.calculateAmountToAssign(this.possibleAssignments, this.payment);
      },
        error => console.error(error));
  }

  searchRegistrationManually(searchString: string) {
    this.isSearching = true;
    this.http.get<PossibleAssignment[]>(`api/events/${this.getEventAcronym()}/registrations?searchstring=${searchString}&states=received`)
      .subscribe(result => {
        this.setAssignments(result);
        this.isSearching = false;
        this.calculateAmountToAssign(this.possibleAssignments, this.payment);
      },
        error => console.error(error));
  }

  calculateAmountToAssign(possibleAssignments: PossibleAssignment[], payment: Payment) {
    for (let possibleAssignment of possibleAssignments) {
      possibleAssignment.amountToAssign = Math.min(possibleAssignment.amount - possibleAssignment.amountPaid, payment.amount - payment.amountAssigned);
    }
  }
  setPayment(payment: Payment) {
    this.payment = payment;
    this.possibleAssignments = null;
    this.searchRegistration(this.payment);
    if (payment != null) {
      this.paymentSlipUrl = `api/events/${this.getEventAcronym()}/paymentslips/${payment.paymentSlipId}`;
    } else {
      this.paymentSlipUrl = null;
    }
  }

  setAssignments(assignments: PossibleAssignment[]) {
    this.possibleAssignments = assignments.sort((a, b) => {
      if (a.isWaitingList === b.isWaitingList) {
        return b.matchScore - a.matchScore;
      }
      if (a.isWaitingList) {
        return 1;
      }
      return -1;
    });
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
  paymentSlipId: string;
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
  isWaitingList: boolean;
  matchScore: number;
  amountMatch: boolean;
}
