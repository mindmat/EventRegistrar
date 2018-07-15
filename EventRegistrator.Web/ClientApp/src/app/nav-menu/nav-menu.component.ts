import { Component, OnInit } from '@angular/core';
import { AuthService } from '../authentication/authService.service';
import { ActivatedRoute } from '@angular/router';
//import { EventsService } from ('../events/events.service');
import { EventService } from '../events/eventService.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  constructor(public authService: AuthService, private route: ActivatedRoute, private eventService: EventService) {
  }

  ngOnInit() {
  }

  isExpanded = false;

  collapse() {
    console.info(this.eventService.eventAcronym);
    this.isExpanded = false;
  }

  toggle() {
    console.info(this.eventService.eventAcronym);
    this.isExpanded = !this.isExpanded;
  }
}
