import { Component } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ActivatedRoute } from "@angular/router";

@Component({
  selector: "smsConversation",
  templateUrl: "./smsConversation.component.html",
  styleUrls: ["./smsConversation.component.css"]
})
export class SmsConversationComponent {
  registrationId: string;
  conversation: Sms[];

  constructor(private readonly http: HttpClient,
    private readonly route: ActivatedRoute) {
  }

  ngOnInit() {
    this.registrationId = this.route.snapshot.params['id'];

    this.refresh();
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  refresh() {
    this.http.get<Sms[]>(`api/events/${this.getEventAcronym()}/registrations/${this.registrationId}/sms`)
      .subscribe(result => { this.conversation = result; },
        error => console.error(error));
  }

  send(text: string) {
    if (text == null || text === "") {
      return;
    }
    var content = new SmsContent();
    content.body = text;
    this.http.post(`api/events/${this.getEventAcronym()}/registrations/${this.registrationId}/sms/send`, content)
      .subscribe(result => { this.refresh() },
        error => console.error(error));
  }
}

class Sms {
  date: Date;
  body: string;
  status: string;
  sent: boolean;
}

class SmsContent {
  body: string
}
