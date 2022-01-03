import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { ParticipantsService, RegistrableWithParticipants } from '../participants.service';

@Component({
  selector: 'app-participants-single',
  templateUrl: './participants-single.component.html'
})
export class ParticipantsSingleComponent implements OnInit
{
  registrable: RegistrableWithParticipants;
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private service: ParticipantsService, private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    // Get the participants
    this.service.registrable$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registrable: RegistrableWithParticipants) =>
      {
        this.registrable = registrable;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }
}
