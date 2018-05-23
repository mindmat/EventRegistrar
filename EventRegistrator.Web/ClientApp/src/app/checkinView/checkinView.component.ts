import { Component, Inject, } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'checkinView',
  templateUrl: './checkinView.component.html'
})
export class CheckinViewComponent {
  registrations: Registration[];

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
  }

  ngOnInit() {
    var eventId = '762A93A4-56E0-402C-B700-1CFB3362B39D';
    this.http.get<Registration[]>(`${this.baseUrl}api/events/${eventId}/checkinView`)
      .subscribe(result => { this.registrations = result; },
        error => console.error(error));
  }

  downloadXlsx() {
    var link = document.createElement('a');
    link.href = "https://eventregistrator.azurewebsites.net/api/events/762A93A4-56E0-402C-B700-1CFB3362B39D/checkin.xlsx";
    link.download = "Report.xlsx";
    link.click();
  }
}

interface Registration {
  Id: string;
  Email: string;
  FirstName: string;
  LastName: string;
  Kurs: string;
  MittagessenSamstag: string;
  MittagessenSonntag: string;
  PartyPass: boolean;
  SoloFriday: boolean;
  Status: string;
  AdmittedAt: Date;
  UnsettledAmount: number;
}
