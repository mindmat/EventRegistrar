import { Component, Input, OnInit } from '@angular/core';
import { RegistrationDisplayInfo } from 'app/api/api';

@Component({
  selector: 'app-participant',
  templateUrl: './participant.component.html'
})
export class ParticipantComponent implements OnInit
{

  @Input() registration?: RegistrationDisplayInfo;
  @Input() placeholderPartner?: string;

  constructor() { }

  ngOnInit(): void
  {
  }

}