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
  searching: boolean;
  saving: boolean;

  constructor(private http: HttpClient, private route: ActivatedRoute) {
    this.searching = false;
  }

  ngOnInit() {
    this.http.get<MailTemplate[]>(`api/events/${this.getEventAcronym()}/mailTemplates`)
      .subscribe(result => {
        this.mailTemplates = result;
      },
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

    this.http.post<MailTemplate>(`api/events/${this.getEventAcronym()}/mailTemplates/${this.mailTemplate.key}`, this.mailTemplate)
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
  language: string;
  template: string;
  senderMail: string;
  senderName: string;
  subject: string;
  audience: number;
}
