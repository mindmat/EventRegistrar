import { Component, OnInit } from '@angular/core';
import { AuthService } from '../authentication/authService.service';
import { ActivatedRoute } from '@angular/router';
//import { EventsService } from ('../events/events.service');

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  constructor(public authService: AuthService, private route: ActivatedRoute) {
  }
  ngOnInit() {
    this.eventAcronym = this.getEventAcronym();
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  isExpanded = false;
  public eventAcronym: string;

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
