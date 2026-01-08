import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RegistrableDisplayInfo, RegistrationDisplayInfo } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { EventService } from '../../events/event.service';
import { ParticipantsService } from '../participants.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { ParticipantComponent } from '../participant/participant.component';

@Component({
  selector: 'app-participants-single',
  templateUrl: './participants-single.component.html',
  standalone: true,
  imports: [CommonModule, RouterModule, DragDropModule, MatButtonModule, MatIconModule, MatMenuModule, MatTooltipModule, TranslateModule, ParticipantComponent]
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

  drop(registration: RegistrationDisplayInfo): void
  {
    if (!!registration.id)
    {
      this.service.promoteFromWaitingList(this.registrable.id, registration.id);
    }
  }

  triggerMoveUp(): void
  {
    if (this.registrable?.id != null)
    {
      this.service.triggerMoveUp(this.registrable.id);
    }
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }
}
