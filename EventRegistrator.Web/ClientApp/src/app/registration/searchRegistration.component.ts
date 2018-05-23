import { Component, Inject, } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'searchRegistration',
    templateUrl: './searchRegistration.component.html'
})
export class SearchRegistrationComponent {
    registrations: Registration[];
    isSearching: boolean;

    constructor(private readonly http: Http, @Inject('BASE_URL') private baseUrl: string) {
        this.isSearching = false;
    }

    ngOnInit() {
    }

    search(searchString: string) {
        const eventId = "762A93A4-56E0-402C-B700-1CFB3362B39D";
        this.isSearching = true;
        this.http.get(`${this.baseUrl}api/event/${eventId}/registrations?searchstring=${searchString}`)
            .subscribe(result => {
                this.registrations = result.json() as Registration[];
                this.registrations.map(reg => reg.ResponsesJoined =
                    reg.Responses.map(rsp => `${rsp.Question} = ${rsp.Response}`).reduce((agg, line) => `${agg} / ${line}`));
                this.isSearching = false;
            },
            error => console.error(error));

    }
}

interface Registration {
    Id: string;
    Email: string;
    FirstName: string;
    LastName: string;
    Language: string;
    Responses: Response[];
    ResponsesJoined: string;
}

interface Response {
    Response: string;
    Question: string;
}