import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'mailTemplates',
  templateUrl: './mailTemplates.component.html'
})
export class MailTemplatesComponent {
  mailTemplates: MailTemplate[];
  mailTemplate: MailTemplate;
  mailTypes: MailType[];
  languages: Language[];
  saving: boolean;

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.http.get<MailTemplate[]>(`api/events/${this.getEventAcronym()}/mailTemplates`)
      .subscribe(result => this.mailTemplates = result,
        error => {
          console.error(error);
        });
    this.http.get<MailType[]>(`api/events/${this.getEventAcronym()}/mails/types`)
      .subscribe(result => this.mailTypes = result,
        error => {
          console.error(error);
        });
    this.http.get<Language[]>(`api/events/${this.getEventAcronym()}/mails/languages`)
      .subscribe(result => this.languages = result,
        error => {
          console.error(error);
        });
  }

  showTemplate(mailTemplate: MailTemplate) {
    this.mailTemplate = mailTemplate;
    this.renderMail(this.mailTemplate.template);
  }

  saveTemplate() {
    this.saving = true;

    this.http.post<MailTemplate>(`api/events/${this.getEventAcronym()}/mailTemplates`, this.mailTemplate)
      .subscribe(result => {
        this.mailTemplate = result;
        this.saving = false;
      },
        error => {
          console.error(error);
          this.saving = false;
        });
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  renderMail(template: string) {
    var mailContainer = document.getElementById("templateRenderSpace") as HTMLDivElement;
    mailContainer.innerHTML = template;
  }
}

class MailTemplate {
  key: string;
  type: string;
  language: string;
  template: string;
  senderMail: string;
  senderName: string;
  subject: string;
  audience: number;
}

class MailType {
  type: string;
  userText: string;
}

class Language {
  acronym: string;
  userText: string;
}
