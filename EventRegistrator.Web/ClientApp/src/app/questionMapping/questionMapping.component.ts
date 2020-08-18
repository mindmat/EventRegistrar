import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { DoubleRegistrable, SingleRegistrable, Registrable } from '../registrables/registrables.component';

@Component({
  selector: 'questionMapping',
  templateUrl: './questionMapping.component.html'
})
export class QuestionMappingComponent implements OnInit {
  dropdownSettingsQuestionOptions = {};
  dropdownSettingsQuestions = {};

  forms: RegistrationFormGroup[];
  availableQuestionOptionMappings: AvailableQuestionOptionMapping[];
  availableQuestionMappings: AvailableQuestionMapping[];

  ngOnInit() {
    this.dropdownSettingsQuestionOptions = {
      placeholder: 'Zuordnung',
      singleSelection: false,
      idField: 'combinedId',
      textField: 'name',
      selectAllText: 'Select All',
      unSelectAllText: 'Unselect All',
      itemsShowLimit: 5,
      allowSearchFilter: true
    };
    this.dropdownSettingsQuestions = {
      placeholder: 'Zuordnung',
      singleSelection: true,
      idField: 'type',
      textField: 'text',
      selectAllText: 'Select All',
      unSelectAllText: 'Unselect All',
      itemsShowLimit: 5,
      allowSearchFilter: true
    };

    this.http.get<AvailableQuestionOptionMapping[]>(`api/events/${this.getEventAcronym()}/availableQuestionOptionMappings`).subscribe(result => {
      this.availableQuestionOptionMappings = result;
    }, error => console.error(error));
    this.http.get<AvailableQuestionMapping[]>(`api/events/${this.getEventAcronym()}/availableQuestionMappings`).subscribe(result => {
      this.availableQuestionMappings = result;
    }, error => console.error(error));

    this.refreshLists();
  }

  private refreshLists() {
    this.http.get<RegistrationFormGroup[]>(`api/events/${this.getEventAcronym()}/formPaths`).subscribe(result => {
      this.forms = result;
      console.log(this.forms);
    }, error => console.error(error));
  }

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  save(form: RegistrationFormGroup) {
    this.http.post(`api/events/${this.getEventAcronym()}/registrationForms/${form.id}/mappings`, form).subscribe(result => {
    }, error => console.error(error));
  }
}

class QuestionOption {
  section: string;
  question: string;
  questionOptionId: string;
  answer: string;
}

class Question {
  id: string;
  section: string;
  question: string;
}

class RegistrationFormGroup {
  id: string;
  title: string;
  sections: FormSection[];
}

class RegistrationFormPath {
  id: string;
  description: string;
}

class FormSection {
  name: string;
  sortKey: number;
  questions: QuestionMapping[];
}

class QuestionMapping {
  id: string;
  question: string;
  type: QuestionType;
  mappable: boolean;
  mapping: QuestionMappingType;
  options: QuestionOptionMapping[];
}

enum QuestionType {
  Checkbox = 1,

  CheckboxGrid = 2,
  Date = 3,

  Datetime = 4,
  Duration = 5,

  Grid = 6,
  Image = 7,
  List = 8,
  MultipleChoice = 9,

  PageBreak = 10,
  ParagraphText = 11,
  Scale = 12,
  SectionHeader = 13,
  Text = 14,

  Time = 15
}

class QuestionOptionMapping {
  combinedId: string;
  id: string;
  answer: string;
  mappedRegistrables: AvailableQuestionOptionMapping[];
}

class AvailableQuestionOptionMapping {
  combinedId: string;
  id: string;
  type: QuestionOptionMappingType;
  name: string;
}

enum QuestionOptionMappingType {
  SingleRegistrable = 1,
  DoubleRegistrable = 2,
  DoubleRegistrableLeader = 3,
  DoubleRegistrableFollower = 4,
  Language = 5,
  Reduction = 6
}

class AvailableQuestionMapping {
  type: QuestionMappingType;
  text: string;
}

enum QuestionMappingType {
  FirstName = 1,
  LastName = 2,
  Phone = 3
}
