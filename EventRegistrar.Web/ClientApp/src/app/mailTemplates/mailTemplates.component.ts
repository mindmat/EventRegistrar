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
    this.refresh()
  }

  refresh() {
    this.http.get<MailTemplate[]>(`api/events/${this.getEventAcronym()}/mailTemplates`)
      .subscribe(result => this.mailTemplates = result,
        error => {
          console.error(error);
        });
  }

  showTemplate(mailTemplate: MailTemplate) {
    this.mailTemplate = mailTemplate;
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

  deleteTemplate(mailTemplateId: string) {
    console.log(mailTemplateId);
    this.http.delete(`api/events/${this.getEventAcronym()}/mailTemplates/${mailTemplateId}`).subscribe(result => {
      this.refresh();
    }, error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class MailTemplate {
  id: string;
  type: string;
  language: string;
  template: string;
  senderMail: string;
  senderName: string;
  subject: string;
  audience: number;
  releaseImmediately: boolean;
}

class MailType {
  type: string;
  userText: string;
}

class Language {
  acronym: string;
  userText: string;
}
