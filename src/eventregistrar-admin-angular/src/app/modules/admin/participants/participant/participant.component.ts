import { Component, Input, OnInit } from '@angular/core';
import { Participant } from '../participants.service';

@Component({
  selector: 'app-participant',
  templateUrl: './participant.component.html'
})
export class ParticipantComponent implements OnInit
{

  @Input() registration?: Participant;
  @Input() placeholderPartner?: string;

  constructor() { }

  ngOnInit(): void
  {
  }

}