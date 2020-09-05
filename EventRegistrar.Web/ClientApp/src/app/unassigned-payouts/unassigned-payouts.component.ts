import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'unassigned-payouts',
  templateUrl: './unassigned-payouts.component.html'
})
export class UnassignedPayoutsComponent implements OnInit {
  payments: Payment[];
  paymentPointer: number;
  payment: Payment;

  possibleAssignments: PossibleAssignment[];
  registrationMatches: PossibleAssignment[];
  isSearching: boolean;

  constructor(private readonly http: HttpClient, private readonly route: ActivatedRoute) {
    this.paymentPointer = 0;
  }

  ngOnInit() {
    this.http.get<Payment[]>(`api/events/${this.getEventAcronym()}/payouts/unassigned`)
      .subscribe(result => {
        this.payments = result;
        this.setPayment(this.payments[0]);
        //for (let payment of this.payouts) {
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

  ignoreBooking(payment: Payment) {
    this.http.post(`api/events/${this.getEventAcronym()}/payments/${payment.id}/ignore`, null)
      .subscribe(result => { this.removePayment(payment); },
        error => {
          console.error(error);
        });
  }

  removePayment(payment: Payment) {
    var index = this.payments.indexOf(payment);
    if (index >= 0) {
      var x = this.payments.splice(index, 1);
      if (this.payment === payment) {
        this.paymentPointer = Math.min(this.payments.length - 1, Math.max(0, this.paymentPointer));
        if (this.paymentPointer < 0) {
          this.setPayment(null);
        }
        else {
          this.setPayment(this.payments[this.paymentPointer]);
        }
      }
    }
  }


  assignPayment(assignment: PossibleAssignment) {
    assignment.locked = true;
    var url = `api/events/${this.getEventAcronym()}/payouts/${assignment.paymentId_OpenPosition != null ? assignment.paymentId_OpenPosition : this.payment.id}/assign/${assignment.payoutRequestId}?amount=${assignment.amountToAssign}`;
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

    if (selection.length > 3) {
      this.searchRegistrationManually(selection);
    } else {
      this.searchRegistration(this.payment);
    }
  }

  searchRegistration(payment: Payment) {
    if (payment == null) {
      this.setAssignments(null);
      return;
    }
    this.isSearching = true;
    this.http.get<PossibleAssignment[]>(`api/events/${this.getEventAcronym()}/payouts/${payment.id}/possibleAssignments`) //?searchstring=${searchString}`)
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
      possibleAssignment.amountToAssign = Math.min(possibleAssignment.amount - possibleAssignment.amountAssigned, payment.amount - payment.amountAssigned);
    }
  }

  setPayment(payment: Payment) {
    this.payment = payment;
    this.possibleAssignments = null;
    this.searchRegistration(this.payment);
  }

  setAssignments(assignments: PossibleAssignment[]) {
    this.possibleAssignments = assignments == null ? null : assignments.sort((a, b) => {
      return b.matchScore - a.matchScore;
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
  creditorName: string;
  creditorIban: string;
}

class PossibleAssignment {
  locked: boolean;
  amountToAssign: number;
  acceptDifference: boolean;
  acceptDifferenceReason: string;

  registrationId: string;
  amount: number;
  amountAssigned: number;
  created: Date;
  currency: string;
  participant: string;
  info: string;
  matchScore: number;
  amountMatch: boolean;
  isOpen: boolean;
  ibans: string[];
  payoutRequestId: string;
  paymentId_OpenPosition: string;
}
