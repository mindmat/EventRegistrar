import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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

  constructor(private http: HttpClient, private router: Router, private route: ActivatedRoute) {
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

  showTemplate(mailingKey: string, language: string) {
    this.searching = true;
    this.http.get<MailTemplate>(`api/events/${this.getEventAcronym()}/mailTemplates/${mailingKey}?lang=${language}`)
      .subscribe(result => {
        this.mailTemplate = result;
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

  renderMail() {
    var mailContainer = document.getElementById("templateRenderSpace") as HTMLDivElement;
    mailContainer.innerHTML = this.mailTemplate.template;
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
