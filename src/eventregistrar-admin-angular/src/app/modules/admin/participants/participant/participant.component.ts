import { Component, Input, OnInit } from '@angular/core';
import { RegistrationDisplayInfo } from 'app/api/api';
import { NavigatorService } from '../../navigator.service';

@Component({
  selector: 'app-participant',
  templateUrl: './participant.component.html'
})
export class ParticipantComponent implements OnInit
{

  @Input() registration?: RegistrationDisplayInfo;
  @Input() placeholderPartner?: string;

  constructor(public navigator: NavigatorService) { }

  ngOnInit(): void
  {
  }

}