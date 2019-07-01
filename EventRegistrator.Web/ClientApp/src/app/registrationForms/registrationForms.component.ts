import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'registrationForms',
  templateUrl: './registrationForms.component.html'
})
export class RegistrationFormsComponent implements OnInit {
  forms: RegistrationForm[];
  ngOnInit() {
    this.http.get<RegistrationForm[]>(`api/events/${this.getEventAcronym()}/registrationforms/pending`).subscribe(result => {
      this.forms = result;
    },
      error => { console.error(error); }
    );
  }

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  import(form: RegistrationForm) {
    // ToDo: hardcoded
    var formId = '1rIgAL0iDPvGAbDAkKsRfj4huqDwz93qK60-2eqdtM7k';
    console.log(form.registrationFormId);
    console.log(form.pendingRawFormCreated);
    //this.http.post(`api/events/${this.getEventAcronym()}/registrationforms/${formId}`, null).subscribe(result => { },
    //  error => { console.error(error); }
    //);
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}


class RegistrationForm {
  registrationFormId: string;
  externalIdentifier: string;
  title: string;
  language: string;
  lastImport: Date;
  pendingRawFormCreated: Date;
  pendingRawFormId: string;
}
