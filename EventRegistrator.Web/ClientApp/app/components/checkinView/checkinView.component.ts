import { Component, Inject, } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'checkinView',
    templateUrl: './checkinView.component.html'
})
export class CheckinViewComponent {
    registrations: Registration[];

    constructor(private http: Http, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
    }

    ngOnInit() {
        var eventId = '762A93A4-56E0-402C-B700-1CFB3362B39D';
        this.http.get(`${this.baseUrl}api/events/${eventId}/checkinView`)
            .subscribe(result => { this.registrations = result.json() as Registration[]; },
            error => console.error(error));
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
    Status: string;
}
