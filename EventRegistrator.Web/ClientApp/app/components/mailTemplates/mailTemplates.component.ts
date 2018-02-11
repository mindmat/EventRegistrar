import { Component, Inject, } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'mailTemplates',
    templateUrl: './mailTemplates.component.html'
})
export class MailTemplatesComponent {
    mailTemplates: MailTemplate[];
    mailTemplate: MailTemplate;
    searching: boolean;
    saving: boolean;
    eventId = "762A93A4-56E0-402C-B700-1CFB3362B39D";

    constructor(private http: Http, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
        this.searching = false;
    }

    ngOnInit() {
        this.http.get(`${this.baseUrl}api/events/${this.eventId}/mailTemplates`)
            .subscribe(result => {
                this.mailTemplates = result.json() as MailTemplate[];
            },
            error => {
                console.error(error);
            });
    }

    showTemplate(mailingKey: string, language: string) {
        this.searching = true;
        this.http.get(`${this.baseUrl}api/events/${this.eventId}/mailTemplates/${mailingKey}?lang=${language}`)
            .subscribe(result => {
                this.mailTemplate = result.json() as MailTemplate;
                this.renderMail();
                this.searching = false;
            },
            error => {
                console.error(error);
                this.searching = false;
            });
    }

    saveTemplate() {
        this.saving = true;

        this.http.post(`${this.baseUrl}api/events/${this.eventId}/mailTemplates/${this.mailTemplate.Key}`, this.mailTemplate)
            .subscribe(result => {
                this.mailTemplate = result.json() as MailTemplate;
                this.saving = false;
            },
            error => {
                console.error(error);
                this.saving = false;
            });
    }

    renderMail() {
        var mailContainer = document.getElementById("templateRenderSpace") as HTMLDivElement;
        mailContainer.innerHTML = this.mailTemplate.Template;
    }

}

interface MailTemplate {
    Key: string;
    Language: string;
    Template: string;
    SenderMail: string;
    SenderName: string;
    Subject: string;
}
