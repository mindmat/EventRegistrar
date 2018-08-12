import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'registrationForms',
  templateUrl: './registrationForms.component.html'
})
export class RegistrationFormsComponent implements OnInit {
  ngOnInit() {
  }

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  import() {
    var formId = '1mwsgFgVgNM4Da6C-81gOPxlp9tRZfrW8X62eTtMIRj8';
    this.http.post(`api/events/${this.getEventAcronym()}/registrationforms/${formId}`, null).subscribe(result => { },
      error => { console.error(error); }
    );
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}
