import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { RegistrationDisplayInfo, Role } from 'app/api/api';
import { NavigatorService } from '../../navigator.service';
import { ParticipantsService } from '../participants.service';

@Component({
  selector: 'app-participant',
  templateUrl: './participant.component.html'
})
export class ParticipantComponent implements OnInit
{
  @Input() registration?: RegistrationDisplayInfo;
  @Input() placeholderPartner?: string;
  @Input() role?: Role;
  @Output() switchRoleEvent = new EventEmitter<SwitchRoleRequest>();

  Role = Role;

  constructor(public navigator: NavigatorService) { }

  ngOnInit(): void
  {
  }

  emitSwitchRole(registrationId: string, toRole: Role)
  {
    this.switchRoleEvent.emit({ registrationId, toRole } as SwitchRoleRequest);
  }
}

export class SwitchRoleRequest
{
  registrationId: string;
  toRole: Role;
}