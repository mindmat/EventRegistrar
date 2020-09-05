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
    this.refreshList();
  }

  private refreshList() {
    this.http.get<RegistrationForm[]>(`api/events/${this.getEventAcronym()}/registrationforms/pending`).subscribe(result => {
      this.forms = result;
    },
      error => { console.error(error); }
    );
  }

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  import(form: RegistrationForm) {
    this.http.post(`api/events/${this.getEventAcronym()}/registrationforms/${form.externalIdentifier}`, null)
      .subscribe(result => { this.refreshList() },
        error => { console.error(error); }
      );
  }

  delete(form: RegistrationForm) {
    this.http.delete(`api/events/${this.getEventAcronym()}/registrationforms/${form.registrationFormId}`)
      .subscribe(result => { this.refreshList() },
        error => { console.error(error); }
      );
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
  state: string;
  deletable: boolean;
}
