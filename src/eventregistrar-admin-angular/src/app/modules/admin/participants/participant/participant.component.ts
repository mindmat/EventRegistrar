import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { RegistrationDisplayInfo, Role } from 'app/api/api';
import { NavigatorService } from '../../navigator.service';
import { ParticipantsService } from '../participants.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-participant',
  templateUrl: './participant.component.html',
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule, MatMenuModule, TranslateModule]
})
export class ParticipantComponent implements OnInit
{
  @Input() registration?: RegistrationDisplayInfo;
  @Input() placeholderPartner?: string;
  @Input() role?: Role;
  @Input() isPartnerSpot?: boolean;
  @Output() switchRoleEvent = new EventEmitter<SwitchRoleRequest>();

  Role = Role;

  constructor(public navigator: NavigatorService) { }

  ngOnInit(): void
  {
  }

  emitSwitchRole(registrationId: string, toRole: Role): void
  {
    this.switchRoleEvent.emit({ registrationId, toRole } as SwitchRoleRequest);
  }
}

export class SwitchRoleRequest
{
  registrationId: string;
  toRole: Role;
}
