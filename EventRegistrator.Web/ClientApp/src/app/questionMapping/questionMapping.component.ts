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
  formPaths: RegistrationFormGroup[];
  availableQuestionOptionMappings: AvailableQuestionOptionMapping[];

  ngOnInit() {
    this.dropdownSettings = {
      placeholder: 'Zuordnung',
      singleSelection: false,
      idField: 'combinedId',
      textField: 'name',
      selectAllText: 'Select All',
      unSelectAllText: 'Unselect All',
      itemsShowLimit: 5,
      allowSearchFilter: true
    };

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

    this.http.get<AvailableQuestionOptionMapping[]>(`api/events/${this.getEventAcronym()}/availableQuestionOptionMappings`).subscribe(result => {
      this.availableQuestionOptionMappings = result;
    }, error => console.error(error));

    this.refreshLists();
  }

  private fillRegistrables() {
    if (this.singleRegistrables && this.doubleRegistrables) {
      this.registrables = this.singleRegistrables.concat(this.doubleRegistrables);
    }
  }

  private onItemSelect(registrable: Registrable, unassignedOption: QuestionOption, formPath: RegistrationFormPath) {
    //this.http.put(`api/events/${this.getEventAcronym()}/questionoptions/${mapping.questionOptionId}/registrables/${registrable.id}`, null).subscribe(result => {
    //}, error => console.error(error));
    let config = formPath.singleConfig;
    let newMapping = new Mapping();
    newMapping.questionOptionId = unassignedOption.questionOptionId;
    newMapping.registrableId = registrable.id;
    if (config.mappingsToRegistrables === null) {
      config.mappingsToRegistrables = [newMapping];
    } else {
      config.mappingsToRegistrables.push(newMapping);
    }
  }

  private onItemDeselect(registrable: Registrable, unassignedOption: QuestionOption, formPath: RegistrationFormPath) {
    let config = formPath.singleConfig;
    //let newMapping = new Mapping();
    //newMapping.questionOptionId = unassignedOption.questionOptionId;
    //newMapping.registrableId = registrable.id;
    //config.mappingsToRegistrables.reduce() slice(newMapping);


    //this.removeMapping(mapping.questionOptionId, registrable.id);
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

    this.http.get<RegistrationFormGroup[]>(`api/events/${this.getEventAcronym()}/formPaths`).subscribe(result => {
      this.formPaths = result;
      console.log(this.formPaths);
    }, error => console.error(error));
  }

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  changeMappingAttribute(mapping: Mapping) {
    //mapping.saveAttributesPending = true;
  }

  changeFormType(form: RegistrationFormMappings) {
    if (!form.singleConfiguration) {
      form.singleConfiguration = new SingleRegistrationFormConfiguration();
    }
  }

  saveMapping(mapping: Mapping) {
    //var attributes = {
    //  questionId_Partner: mapping.questionId_Partner,
    //  questionOptionId_Leader: mapping.questionOptionId_Leader,
    //  questionOptionId_Follower: mapping.questionOptionId_Follower
    //};
    //this.http.put(`api/events/${this.getEventAcronym()}/questionoptionsmapping/${mapping.id}`, attributes).subscribe(result => {
    //  //mapping.saveAttributesPending = false;
    //}, error => console.error(error));
  }

  save(form: RegistrationFormGroup) {
    this.http.post(`api/events/${this.getEventAcronym()}/registrationForms/${form.id}/mappings`, form).subscribe(result => {
    }, error => console.error(error));
  }
}

class RegistrationFormMappings {
  registrationFormId: string;
  type: FormType;
  title: string;
  singleConfiguration: SingleRegistrationFormConfiguration;
  unassignedOptions: Mapping[];
  questions: Question[];
}

//class Mapping {
//  id: string;
//  registrationFormId: string;
//  section: string;
//  question: string;
//  answer: string;
//  questionOptionId: string;
//  registrableId: string;
//  registrableName: string;
//  isPartnerRegistrable: boolean;
//  assignedRegistrableIds: string[];
//  questionId_Partner: string;
//  questionOptionId_Leader: string;
//  questionOptionId_Follower: string;
//  saveAttributesPending: boolean;
//}
class Mapping {
  questionOptionId: string;
  registrableId: string;
  assignedRegistrableIds: string[];
  questionId_Partner: string;
  questionOptionId_Leader: string;
  questionOptionId_Follower: string;
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

class RegistrationFormGroup {
  id: string;
  title: string;
  paths: RegistrationFormPath[];
  sections: FormSection[];
  questions: Question[];
  unassignedOptions: QuestionOption[];
}

class RegistrationFormPath {
  id: string;
  description: string;
  singleConfig: SingleRegistrationFormConfiguration;
  //partnerConfig: Partner
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
  mappingsToRegistrables: Mapping[];
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
  type: MappingType;
  name: string;
}

enum MappingType {
  SingleRegistrable = 1,
  //DoubleRegistrable = 2,
  DoubleRegistrableLeader = 3,
  DoubleRegistrableFollower = 4,
  Language = 5,
  Reduction = 6
}
