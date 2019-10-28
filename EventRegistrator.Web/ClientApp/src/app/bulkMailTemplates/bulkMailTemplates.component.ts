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
  selectedAudiences = [];
  possibleAudiences = [];
  dropdownSettings: {};

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
    this.possibleAudiences = [
      { id: Audience.paid, name: "bezahlt" },
      { id: Audience.unpaid, name: "Zahlung ausstehend" },
      { id: Audience.waitingList, name: "auf Warteliste" }
    ];

    this.dropdownSettings = {
      placeholder: '-',
      singleSelection: false,
      idField: 'id',
      textField: 'name',
      enableCheckAll: false,
      itemsShowLimit: 5,
      allowSearchFilter: false
    };

  }

  showTemplate(mailTemplate: BulkMailTemplate) {
    this.mailingTemplate = mailTemplate;
    this.selectedAudiences = new Array();
    if (this.mailingTemplate.audience & Audience.paid) {
      this.selectedAudiences.push(this.possibleAudiences.find(itm => itm.id == Audience.paid));
    }
    if (this.mailingTemplate.audience & Audience.unpaid) {
      this.selectedAudiences.push(this.possibleAudiences.find(itm => itm.id == Audience.unpaid));
    }
    if (this.mailingTemplate.audience & Audience.waitingList) {
      this.selectedAudiences.push(this.possibleAudiences.find(itm => itm.id == Audience.waitingList));
    }
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
        });
  }

  createNew() {
    this.mailingTemplate = new BulkMailTemplate();
    this.mailingTemplate.id = Guid.newGuid();
  }

  releaseMails() {
    this.saving = true;
    this.http.post<BulkMailTemplate>(`api/events/${this.getEventAcronym()}/bulkMailTemplates/${this.mailingTemplate.key}/releaseMails`, this.mailingTemplate)
      .subscribe(result => {
        this.saving = false;
      },
        error => {
          console.error(error);
          this.saving = false;
        });
  }

  setAudience() {
    this.mailingTemplate.audience = this.selectedAudiences.length > 0
      ? this.selectedAudiences.map(itm => itm.id).reduce((sum, current) => sum += current)
      : 0;
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
  audience: Audience;
  mailsReadyCount: number;
  mailsSentCount: number;
}

class Language {
  acronym: string;
  userText: string;
}

enum Audience {
  paid = 1,
  unpaid = 2,
  waitingList = 4
}
