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
  //mappings: Mapping[];
  //questions: Question[];
  //unassignedQuestionOptions: Mapping[];
  dropdownSettings = {};

  formTypeItems: FormTypeItem[];

  forms: RegistrationFormMappings[];
  formPaths: IRegistrationProcessConfiguration[];

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

    this.http.get<FormTypeItem[]>(`api/registrationFormTypes`).subscribe(result => {
      this.formTypeItems = result;
    }, error => console.error(error));

  }
  private fillRegistrables() {
    if (this.singleRegistrables != null && this.doubleRegistrables !== null) {
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
    this.http.get<RegistrationFormMappings[]>(`api/events/${this.getEventAcronym()}/registrationForms`).subscribe(result => {
      this.forms = result;
    }, error => console.error(error));

    this.http.get<IRegistrationProcessConfiguration[]>(`api/events/${this.getEventAcronym()}/formPaths`).subscribe(result => {
      this.formPaths = result;
    }, error => console.error(error));
  }

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  changeMappingAttribute(mapping: Mapping) {
    mapping.saveAttributesPending = true;
  }

  changeFormType(form: RegistrationFormMappings) {
    if (!form.singleConfiguration) {
      form.singleConfiguration = new SingleRegistrationFormConfiguration();
    }
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

  save(form: RegistrationFormMappings) {
    this.http.post(`api/events/${this.getEventAcronym()}/registrationForms/${form.registrationFormId}/mappings`, form).subscribe(result => {
    }, error => console.error(error));
  }
}

class RegistrationFormMappings {
  registrationFormId: string;
  type: FormType;
  title: string;
  singleConfiguration: SingleRegistrationFormConfiguration;
  mappings: Mapping[];
  unassignedOptions: Mapping[];
  questions: Question[];
}

class Mapping {
  id: string;
  registrationFormId: string;
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

class Question {
  id: string;
  section: string;
  question: string;
}

//class Registrable {
//  id: string;
//  name: string;
//}

enum FormType {
  Single = 1,
  Partner = 2
}

class FormTypeItem {
  type: FormType;
  name: string;
}

interface IRegistrationProcessConfiguration {
  id: string;
  registrationFormId: string;
  description: string;
  type: FormType;
}

class SingleRegistrationFormConfiguration implements IRegistrationProcessConfiguration {
  id: string;
  registrationFormId: string;
  type: FormType;
  description: string;
  //IEnumerable< (Guid QuestionOptionId, string Language)> LanguageMappings: string;
  questionId_Email: string;
  questionId_FirstName: string;
  questionId_LastName: string;
  questionId_Phone: string;
  questionId_Remarks: string;
  questionOptionId_Follower: string;
  questionOptionId_Leader: string;
  questionOptionId_Reduction: string;
  questionOptionId_Trigger: string;
}

