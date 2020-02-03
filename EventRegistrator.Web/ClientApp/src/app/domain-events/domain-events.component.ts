import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'domain-events',
  templateUrl: './domain-events.component.html'
})
export class DomainEventsComponent {
  domainevents: DomainEvent[];

  constructor(private readonly http: HttpClient, private readonly route: ActivatedRoute) {
  }

  ngOnInit() {
    this.refresh();
  }

  refresh() {
    this.http.get<DomainEvent[]>(`api/events/${this.getEventAcronym()}/domainevents`)
      .subscribe(result => {
        this.domainevents = result;
      },
        error => {
          console.error(error);
        });
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class DomainEvent {
  id: string;
  timestamp: Date;
  type: string;
  content: string;
}
