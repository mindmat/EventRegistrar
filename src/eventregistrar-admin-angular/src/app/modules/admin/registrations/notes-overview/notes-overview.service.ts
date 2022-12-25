import { Injectable } from '@angular/core';
import { NotesDisplayItem, Api } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class NotesOverviewService extends FetchService<NotesDisplayItem[] | null>
{
  constructor(private api: Api, private eventService: EventService, notificationService: NotificationService)
  {
    super('InternalNotesQuery', notificationService);
  }

  get notes$(): Observable<NotesDisplayItem[]>
  {
    return this.result$;
  }

  fetchNotes(): Observable<any>
  {
    return this.fetchItems(this.api.internalNotes_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}
