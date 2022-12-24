import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RegistrableDisplayInfo, RegistrationDisplayInfo } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { EventService } from '../../events/event.service';
import { ParticipantsService } from '../participants.service';

@Component({
  selector: 'app-participants-single',
  templateUrl: './participants-single.component.html'
})
export class ParticipantsSingleComponent implements OnInit
{
  registrable: RegistrableDisplayInfo;
  dragOverParticipants: boolean;
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private service: ParticipantsService, private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    // Get the participants
    this.service.registrable$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registrable: RegistrableDisplayInfo) =>
      {
        this.registrable = registrable;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  drop(registration: RegistrationDisplayInfo)
  {
    if (!!registration.id)
    {
      this.service.promoteFromWaitingList(this.registrable.id, registration.id);
    }
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }
}
