import { Component } from '@angular/core';
import { AuthService } from '../authentication/authService.service';
//import { EventsService } from ('../events/events.service');

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  constructor(public authService: AuthService) {
  }

  isExpanded = false;
  public eventAcronym: string = "tev";

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
