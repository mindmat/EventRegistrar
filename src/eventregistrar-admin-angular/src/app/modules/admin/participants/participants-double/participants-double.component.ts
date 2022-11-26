import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { ParticipantsService } from '../participants.service';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
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

  participantsDropped(event: CdkDragDrop<SpotDisplayInfo | RegistrationDisplayInfo>): void
  {
    // Move or transfer the item
    if (event.previousContainer === event.container)
    {
      // Move the item
      // moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    }
    else
    {
      // Transfer the item
      // transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);

      // Update the card's list it
      // event.container.data[event.currentIndex].listId = event.container.id;
    }

    // // Calculate the positions
    // const updated = this._calculatePositions(event);

    // // Update the cards
    // this._scrumboardService.updateCards(updated).subscribe();
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }
}
