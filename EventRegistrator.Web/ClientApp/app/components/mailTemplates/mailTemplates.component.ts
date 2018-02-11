import { Component, Inject, } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'mailTemplates',
    templateUrl: './mailTemplates.component.html'
})
export class MailTemplatesComponent {
    mailTemplate: MailTemplate;
    searching: boolean;
    saving: boolean;

    constructor(private http: Http, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
        this.searching = false;
    }

    getTemplate(mailingKey: string, language: string) {
        const eventId = '762A93A4-56E0-402C-B700-1CFB3362B39D';
        this.searching = true;
        this.http.get(`${this.baseUrl}api/events/${eventId}/mailTemplates/${mailingKey}?lang=${language}`)
            .subscribe(result => {
                this.mailTemplate = result.json() as MailTemplate;
                console.log("ok2");
                console.log(this.mailTemplate);
                this.searching = false;
            },
            error => {
                console.error(error);
                this.searching = false;
            });
    }

    saveTemplate(mailingKey: string, language: string) {
        const eventId = '762A93A4-56E0-402C-B700-1CFB3362B39D';
        this.saving = true;
        this.mailTemplate.Language = language;

        this.http.post(`${this.baseUrl}api/events/${eventId}/mailTemplates/${mailingKey}`, this.mailTemplate)
            .subscribe(result => {
                this.mailTemplate = result.json() as MailTemplate;
                this.saving = false;
            },
            error => {
                console.error(error);
                this.saving = false;
            });
    }
}

interface MailTemplate {
    Language: string;
    Template: string;
    SenderMail: string;
    SenderName: string;
    Subject: string;
}
