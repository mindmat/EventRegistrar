import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'questionMapping',
  templateUrl: './questionMapping.component.html'
})
export class QuestionMappingComponent implements OnInit {
  ngOnInit() {
    this.http.get<Mapping[]>(`api/events/${this.getEventAcronym()}/questions/mapping`).subscribe(result => {
      this.mappings = result;
    }, error => console.error(error));
  }

  mappings: Mapping[];

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class Mapping {
  answer: string;
  questionOptionId: string;
  registrableId: string;
  registrableName: string;
  question: string;
}
