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
  possibleRepaymentAssignment: PossibleRepaymentAssignment[];
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
      if (this.payment == payment) {
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

  assignToRepayment(assignment: PossibleRepaymentAssignment) {
    assignment.locked = true;
    var url = `api/events/${this.getEventAcronym()}/payments/${assignment.paymentId_OpenPosition != null ? assignment.paymentId_OpenPosition : this.payment.id}/assignToRepayment/${assignment.paymentId_Counter}?amount=${assignment.amountToAssign}`;
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
      this.setPossibleRepaymentAssignments(null);
      return;
    }
    this.isSearching = true;
    this.http.get<PossibleAssignment[]>(`api/events/${this.getEventAcronym()}/payments/${payment.id}/possibleAssignments`) //?searchstring=${searchString}`)
      .subscribe(result => {
        this.setAssignments(result);
        this.isSearching = false;
        this.calculateAmountToAssign(this.possibleAssignments, this.payment);
      },
        error => console.error(error));

    this.http.get<PossibleRepaymentAssignment[]>(`api/events/${this.getEventAcronym()}/payments/${payment.id}/possibleOutgoingAssignments`) //?searchstring=${searchString}`)
      .subscribe(result => {
        this.setPossibleRepaymentAssignments(result);
        this.calculateAmountToAssignToRepayment(this.possibleRepaymentAssignment, this.payment);
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

  calculateAmountToAssignToRepayment(possibleAssignments: PossibleRepaymentAssignment[], payment: Payment) {
    for (let possibleAssignment of possibleAssignments) {
      possibleAssignment.amountToAssign = Math.min(possibleAssignment.amountUnsettled, payment.amount - payment.amountAssigned);
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
    this.possibleAssignments = assignments == null ? null : assignments.sort((a, b) => {
      if (a.isWaitingList === b.isWaitingList) {
        return b.matchScore - a.matchScore;
      }
      if (a.isWaitingList) {
        return 1;
      }
      return -1;
    });
  }

  setPossibleRepaymentAssignments(assignments: PossibleRepaymentAssignment[]) {
    this.possibleRepaymentAssignment = assignments == null ? null :assignments.sort((a, b) => {
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

class PossibleRepaymentAssignment {
  amount: number;
  amountUnsettled: number;
  amountToAssign: number;
  bookingDate: Date;
  currency: string;
  debitorName: string;
  info: string;
  message: string;
  matchScore: number;
  paymentId_Counter: string;
  paymentId_OpenPosition: string;
  settled: boolean;
  locked: boolean;
}
