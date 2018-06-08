import { Component } from '@angular/core';
import { AuthService } from "../authentication/authService.service";

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  constructor(public authService: AuthService) {

  }

  isExpanded = false;
  public eventAcronym: string = "ll18";

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
