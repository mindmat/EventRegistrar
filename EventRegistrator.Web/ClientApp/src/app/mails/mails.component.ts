import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'mails',
  templateUrl: './mails.component.html'
})
export class MailsComponent {
  mails: Mail[];
  mail: Mail;
  saving: boolean;

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.refresh();
  }

  refresh() {
    this.http.get<Mail[]>(`api/events/${this.getEventAcronym()}/mails/pending`)
      .subscribe(result => {
        this.mails = result;
      },
        error => {
          console.error(error);
        });
  }

  showMail(mail: Mail) {
    this.mail = mail;
    this.renderMail(this.mail.contentHtml);
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  renderMail(template: string) {
    var mailContainer = document.getElementById("mailRenderSpace") as HTMLDivElement;
    mailContainer.innerHTML = template;
  }

  releaseMail(mailId: string) {
    this.http.post(`api/events/${this.getEventAcronym()}/mails/${mailId}/release`, null).subscribe(result => {
      this.refresh();
    });
  }
}

class Mail {
  id: string;
  senderMail: string;
  senderName: string;
  subject: string;
  recipients: string;
  created: Date;
  withhold: boolean;
  contentHtml: string;
}
