import { Component, Input, OnInit } from '@angular/core';
import { Registration } from '../participants.service';

@Component({
  selector: 'app-participant',
  templateUrl: './participant.component.html'
})
export class ParticipantComponent implements OnInit
{

  @Input() registration?: Registration;
  @Input() placeholderPartner?: string;

  constructor() { }

  ngOnInit(): void
  {
  }

}