import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { DoubleRegistrable, SingleRegistrable, Registrable } from '../registrables/registrables.component';

@Component({
  selector: 'questionMapping',
  templateUrl: './questionMapping.component.html'
})
export class QuestionMappingComponent implements OnInit {
  doubleRegistrables: Registrable[];
  singleRegistrables: Registrable[];
  registrables: Registrable[];
  mappings: Mapping[];
  questions: Question[];
  unassignedQuestionOptions: Mapping[];
  dropdownSettings = {};

  ngOnInit() {
    this.dropdownSettings = {
      placeholder: 'Zuordnung',
      singleSelection: false,
      idField: 'id',
      textField: 'name',
      selectAllText: 'Select All',
      unSelectAllText: 'UnSelect All',
      itemsShowLimit: 5,
      allowSearchFilter: true
    };

    this.refreshLists();

    this.http.get<DoubleRegistrable[]>(`api/events/${this.getEventAcronym()}/DoubleRegistrableOverview`).subscribe(result => {
      this.doubleRegistrables = result;
      this.fillRegistrables();
    }, error => console.error(error));
    this.http.get<SingleRegistrable[]>(`api/events/${this.getEventAcronym()}/SingleRegistrableOverview`).subscribe(result => {
      this.singleRegistrables = result;
      this.fillRegistrables();
    }, error => console.error(error));

  }
  private fillRegistrables() {
    if (this.singleRegistrables != null && this.doubleRegistrables != null) {
      this.registrables = this.singleRegistrables.concat(this.doubleRegistrables);
    }
  }

  private onItemSelect(registrable: Registrable, mapping: Mapping) {
    this.http.put(`api/events/${this.getEventAcronym()}/questionoptions/${mapping.questionOptionId}/registrables/${registrable.id}`, null).subscribe(result => {
    }, error => console.error(error));
  }

  private onItemDeselect(registrable: Registrable, mapping: Mapping) {
    this.removeMapping(mapping.questionOptionId, registrable.id);
  }

  private removeMapping(questionOptionId: string, registrableId: string) {
    this.http.delete(`api/events/${this.getEventAcronym()}/questionoptions/${questionOptionId}/registrables/${registrableId}`).subscribe(result => {
      this.refreshLists();
    }, error => console.error(error));
}

  private refreshLists() {
    this.http.get<Mapping[]>(`api/events/${this.getEventAcronym()}/questions/mapping`).subscribe(result => {
      this.mappings = result;
    }, error => console.error(error));

    this.http.get<Mapping[]>(`api/events/${this.getEventAcronym()}/questions/unassignedOptions`).subscribe(result => {
      this.unassignedQuestionOptions = result;
    }, error => console.error(error));

    this.http.get<Question[]>(`api/events/${this.getEventAcronym()}/questions`).subscribe(result => {
      this.questions = result;
    }, error => console.error(error)); }

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
  
  changeMappingAttribute(mapping: Mapping) {
    mapping.saveAttributesPending = true;
  }

  saveMapping(mapping: Mapping) {
    var attributes = {
      questionId_Partner: mapping.questionId_Partner,
      questionOptionId_Leader: mapping.questionOptionId_Leader,
      questionOptionId_Follower: mapping.questionOptionId_Follower
    };
    this.http.put(`api/events/${this.getEventAcronym()}/questionoptionsmapping/${mapping.id}`, attributes).subscribe(result => {
      mapping.saveAttributesPending = false;
    }, error => console.error(error));
  }
}

class Mapping {
  id: string;
  section: string;
  question: string;
  answer: string;
  questionOptionId: string;
  registrableId: string;
  registrableName: string;
  isPartnerRegistrable: boolean;
  assignedRegistrableIds: string[];
  questionId_Partner: string;
  questionOptionId_Leader: string;
  questionOptionId_Follower: string;
  saveAttributesPending: boolean;
}

class Question{
  id: string;
  section: string;
  question: string;
}

//class Registrable {
//  id: string;
//  name: string;
//}
