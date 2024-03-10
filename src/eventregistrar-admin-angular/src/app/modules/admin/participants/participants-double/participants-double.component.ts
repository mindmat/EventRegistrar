import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { ParticipantsService } from '../participants.service';
import { CdkDrag, CdkDragDrop } from '@angular/cdk/drag-drop';
import { RegistrableDisplayInfo, RegistrationDisplayInfo, Role, SpotDisplayInfo } from 'app/api/api';

@Component({
  selector: 'app-participants-double',
  templateUrl: './participants-double.component.html'
})
export class ParticipantsDoubleComponent implements OnInit
{
  registrable: RegistrableDisplayInfo;
  dragOverParticipants: boolean;
  canDefrag: boolean;
  private unsubscribeAll: Subject<any> = new Subject<any>();
  Role = Role;

  constructor(private service: ParticipantsService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    // Get the participants
    this.service.registrable$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registrable: RegistrableDisplayInfo) =>
      {
        this.registrable = registrable;

        this.canDefrag = this.registrable.participants.some(prt => !prt.isPartnerRegistration && prt.leader !== null && prt.follower === null)
          && this.registrable.participants.some(prt => !prt.isPartnerRegistration && prt.leader === null && prt.follower !== null);

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  defrag(): void
  {
    this.service.defrag(this.registrable.id);
  }

  triggerMoveUp(): void
  {
    if (this.registrable?.id != null)
    {
      this.service.triggerMoveUp(this.registrable.id);
    }
  }

  switchRole(registrationId: string, toRole: Role)
  {
    this.service.switchRole(this.registrable.id, registrationId, toRole);
  }

  drop(dragData: SpotDisplayInfo | RegistrationDisplayInfo): void
  {
    const registrationId = (<RegistrationDisplayInfo>dragData).id
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
