import { Component, Input, OnInit } from '@angular/core';
import { Registration } from '../participants-double/participants-double.service';

@Component({
  selector: 'app-participant',
  templateUrl: './participant.component.html',
  styleUrls: ['./participant.component.scss']
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
