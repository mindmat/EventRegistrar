import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { Guid } from "../infrastructure/guid";

@Component({
  selector: 'bulkMailTemplates',
  templateUrl: './bulkMailTemplates.component.html'
})
export class BulkMailTemplatesComponent {
  mailingTemplates: BulkMailTemplate[];
  mailingTemplate: BulkMailTemplate;
  languages: Language[];
  saving: boolean;

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.http.get<BulkMailTemplate[]>(`api/events/${this.getEventAcronym()}/bulkMailTemplates`)
      .subscribe(result => this.mailingTemplates = result,
        error => {
          console.error(error);
        });
    this.http.get<Language[]>(`api/events/${this.getEventAcronym()}/mails/languages`)
      .subscribe(result => this.languages = result,
        error => {
          console.error(error);
        });
  }

  showTemplate(mailTemplate: BulkMailTemplate) {
    this.mailingTemplate = mailTemplate;
  }

  saveTemplate() {
    this.saving = true;

    this.http.post<BulkMailTemplate>(`api/events/${this.getEventAcronym()}/bulkMailTemplates/${this.mailingTemplate.id}`, this.mailingTemplate)
      .subscribe(result => {
        this.mailingTemplate = result;
        this.saving = false;
      },
        error => {
          console.error(error);
          this.saving = false;
        });
  }

  createMails() {
    this.saving = true;
    this.http.post<BulkMailTemplate>(`api/events/${this.getEventAcronym()}/bulkMailTemplates/${this.mailingTemplate.key}/createMails`, this.mailingTemplate)
      .subscribe(result => {
        this.saving = false;
      },
        error => {
          console.error(error);
          this.saving = false;
        });  }

  createNew() {
    this.mailingTemplate = new BulkMailTemplate();
    this.mailingTemplate.id = Guid.newGuid();
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class BulkMailTemplate {
  id: string;
  key: string;
  language: string;
  template: string;
  senderMail: string;
  senderName: string;
  subject: string;
  audience: number;
}

class Language {
  acronym: string;
  userText: string;
}
