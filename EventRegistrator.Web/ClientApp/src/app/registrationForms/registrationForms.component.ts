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
    // ToDo: hardcoded
    var formId = '1OGXhOqcT2OT6map1tr5omg4bt8_xU3vMraDbxmaH4uI';
    this.http.post(`api/events/${this.getEventAcronym()}/registrationforms/${formId}`, null).subscribe(result => { },
      error => { console.error(error); }
    );
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}
