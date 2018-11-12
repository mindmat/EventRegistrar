import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'partnerMatching',
  templateUrl: './partnerMatching.component.html'
})
export class PartnerMatchingComponent implements OnInit {
  unassignedPartnerRegistrations: PotentialPartner[];
  unassignedPartnerPointer: number;
  unassignedPartnerRegistration: PotentialPartner;

  potentialMatches: PotentialPartner[];
  isSearching: boolean;

  constructor(private readonly http: HttpClient, private readonly route: ActivatedRoute) {
    this.unassignedPartnerPointer = 0;
  }

  ngOnInit() {
    this.http.get<PotentialPartner[]>(`api/events/${this.getEventAcronym()}/registrations/unmatchedPartners`)
      .subscribe(result => {
        this.unassignedPartnerRegistrations = result;
        this.setRegistration(this.unassignedPartnerRegistrations[0]);
      },
        error => console.error(error));
  }

  gotoPrevious() {
    if (this.unassignedPartnerPointer > 0) {
      console.log(this.unassignedPartnerPointer);
      this.setRegistration(this.unassignedPartnerRegistrations[--this.unassignedPartnerPointer]);
    }
  }

  gotoNext() {
    if (this.unassignedPartnerPointer < this.unassignedPartnerRegistrations.length - 1) {
      console.log(this.unassignedPartnerPointer);
      this.setRegistration(this.unassignedPartnerRegistrations[++this.unassignedPartnerPointer]);
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

  match(assignment: PotentialPartner) {
    assignment.locked = true;
    var url = `api/events/${this.getEventAcronym()}/registrations/matchPartners?registrationId1=${this.unassignedPartnerRegistration.registrationId}&registrationId2=${assignment.registrationId}`;
    this.http.post(url, null)
      .subscribe(result => { },
        error => {
          console.error(error);
          assignment.locked = false;
        });
  }

  //textSelected() {
  //  var selection = document.getSelection().toString();
  //  console.log(`selection: ${selection}`);

  //  if (selection.length > 3) {
  //    this.searchRegistrationManually(selection);
  //  } else {
  //    this.searchRegistration(this.payment);
  //  }
  //}

  searchRegistration(unassignedPartnerRegistration: PotentialPartner) {
    this.isSearching = true;
    var url = `api/events/${this.getEventAcronym()}/registrations/${unassignedPartnerRegistration.registrationId}/potentialMatches`;
    this.http.get<PotentialPartner[]>(url) //?searchstring=${searchString}`)
      .subscribe(result => {
        this.potentialMatches = result;
        this.isSearching = false;
      },
        error => console.error(error));
  }

  //searchRegistrationManually(searchString: string) {
  //  this.isSearching = true;
  //  this.http.get<PossibleAssignment[]>(`api/events/${this.getEventAcronym()}/registrations?searchstring=${searchString}&states=received`)
  //    .subscribe(result => {
  //      this.possibleAssignments = result;
  //      this.isSearching = false;
  //      this.calculateAmountToAssign(this.possibleAssignments, this.payment);
  //    },
  //      error => console.error(error));
  //}

  setRegistration(unassignedPartnerRegistration: PotentialPartner) {
    this.unassignedPartnerRegistration = unassignedPartnerRegistration;
    this.potentialMatches = null;
    this.searchRegistration(this.unassignedPartnerRegistration);
  }
}

class PotentialPartner {
  email: string;
  firstName: string;
  lastName: string;
  registrationId: string;
  state: string;
  isWaitingList: boolean;
  registrables: string[];
  locked: boolean;
  registrationId_Partner: string;
  matchedPartner: string;
}
