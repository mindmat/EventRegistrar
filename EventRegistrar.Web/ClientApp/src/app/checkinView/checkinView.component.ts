import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'checkinView',
  templateUrl: './checkinView.component.html'
})
export class CheckinViewComponent {
  checkinView: CheckinView;

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.http.get<CheckinView>(`api/events/${this.getEventAcronym()}/checkinView`)
      .subscribe(result => { this.checkinView = result; },
        error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  downloadXlsx() {
    var link = document.createElement('a');
    link.href = `api/events/${this.getEventAcronym()}/checkinView.xlsx`;
    link.download = "Checkin.xlsx";
    link.click();
  }
}

class CheckinView {
  dynamicHeaders: string[];
  items: Registration[];
}

class Registration {
  registrationId: string;
  email: string;
  firstName: string;
  lastName: string;
  status: string;
  admittedAt: Date;
  unsettledAmount: number;
}
