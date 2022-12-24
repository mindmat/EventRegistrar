import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { ParticipantsService } from '../participants.service';
import { CdkDrag, CdkDragDrop } from '@angular/cdk/drag-drop';
import { RegistrableDisplayInfo, RegistrationDisplayInfo, SpotDisplayInfo } from 'app/api/api';

@Component({
  selector: 'app-participants-double',
  templateUrl: './participants-double.component.html'
})
export class ParticipantsDoubleComponent implements OnInit
{
  constructor(private service: ParticipantsService,
    private changeDetectorRef: ChangeDetectorRef) { }

  private unsubscribeAll: Subject<any> = new Subject<any>();
  registrable: RegistrableDisplayInfo;
  dragOverParticipants: boolean;

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

  triggerMoveUp(): void
  {
    if (this.registrable?.id != null)
    {
      this.service.triggerMoveUp(this.registrable.id);
    }
  }

  drop(dragData: SpotDisplayInfo | RegistrationDisplayInfo)
  {
    var registrationId = (<RegistrationDisplayInfo>dragData).id
      ?? (<SpotDisplayInfo>dragData).leader?.id
      ?? (<SpotDisplayInfo>dragData).follower?.id;

    if (!!registrationId)
    {
      this.service.promoteFromWaitingList(this.registrable.id, registrationId);
    }
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }
}
