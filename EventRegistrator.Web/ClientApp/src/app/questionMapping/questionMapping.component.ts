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
      itemsShowLimit: 3,
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

  private onItemSelect(item: Registrable, mapping: Mapping) {
    this.http.put(`api/events/${this.getEventAcronym()}/questionoptions/${mapping.questionOptionId}/registrables/${item.id}`, null).subscribe(result => {
    }, error => console.error(error));
  }

  private onItemDeselect(item: Registrable, mapping: Mapping) {
    this.http.delete(`api/events/${this.getEventAcronym()}/questionoptions/${mapping.questionOptionId}/registrables/${item.id}`).subscribe(result => {
    }, error => console.error(error));
  }

  private refreshLists() {
    this.http.get<Mapping[]>(`api/events/${this.getEventAcronym()}/questions/mapping`).subscribe(result => {
      this.mappings = result;
    }, error => console.error(error));

    this.http.get<Mapping[]>(`api/events/${this.getEventAcronym()}/questions/unassignedOptions`).subscribe(result => {
      this.unassignedQuestionOptions = result;
    }, error => console.error(error));
  }

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class Mapping {
  answer: string;
  questionOptionId: string;
  registrableId: string;
  registrableName: string;
  question: string;
  assignedRegistrableIds: string[];
}

//class Registrable {
//  id: string;
//  name: string;
//}
