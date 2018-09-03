import { Component, OnInit } from '@angular/core';
import { AuthService } from '../authentication/authService.service';
import { ActivatedRoute } from '@angular/router';
import { EventService } from '../events/eventService.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  constructor(public authService: AuthService,
    private readonly route: ActivatedRoute,
    public eventService: EventService) {
  }

  isExpanded = true;

  collapse() {
    console.info(this.eventService.acronym);
    this.isExpanded = false;
  }

  toggle() {
    console.info(this.eventService.acronym);
    this.isExpanded = !this.isExpanded;
  }
}
