import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'domain-events',
  templateUrl: './domain-events.component.html'
})
export class DomainEventsComponent {
  domainevents: DomainEvent[];
  domaineventTypes: DomainEventType[];
  selectedDomaineventTypes: DomainEventType[];
  dropdownSettings: {};

  constructor(private readonly http: HttpClient, private readonly router: Router, private readonly route: ActivatedRoute) {
  }

  ngOnInit() {
    this.dropdownSettings = {
      placeholder: '-',
      singleSelection: false,
      idField: 'typeNameShort',
      textField: 'userText',
      enableCheckAll: true,
      itemsShowLimit: 10,
      allowSearchFilter: true
    };

    this.http.get<DomainEventType[]>(`api/events/${this.getEventAcronym()}/domaineventtypes`)
      .subscribe(result => {
        result.forEach(det => det.typeNameShort = det.typeName.substr(det.typeName.lastIndexOf(".") + 1));
        this.domaineventTypes = result;
        this.route.queryParamMap.subscribe(params => {
          this.selectedDomaineventTypes = this.domaineventTypes.filter(det => params.getAll("types").includes(det.typeNameShort));
          this.refresh();
        });
      },
        error => {
          console.error(error);
        });
  }

  refresh() {
    let url = `api/events/${this.getEventAcronym()}/domainevents`;
    if (this.selectedDomaineventTypes) {
      url += "?" + this.selectedDomaineventTypes.map(typ => `types=${typ.typeName}`).join("&");
    }

    this.http.get<DomainEvent[]>(url)
      .subscribe(result => {
        this.domainevents = result;
      },
        error => {
          console.error(error);
        });
  }

  filterChanged() {
    this.router.navigate(
      [],
      {
        relativeTo: this.route,
        queryParams: { types: this.selectedDomaineventTypes.map(typ => typ.typeNameShort) },
        queryParamsHandling: 'merge'
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

class DomainEventType {
  typeNameShort: string;
  typeName: string;
  userText: string;
}
