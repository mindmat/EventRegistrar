import { Injectable } from '@angular/core';
import { Api, AvailableQuestionMapping } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class QuestionMappingService extends FetchService<AvailableQuestionMapping[]>
{
  constructor(private api: Api,
    notificationService: NotificationService,
    private eventService: EventService)
  {
    super('AvailableQuestionMappingsQuery', notificationService);
  }

  get questionMappings(): Observable<AvailableQuestionMapping[]>
  {
    return this.result$;
  }

  fetchMappings(): Observable<any>
  {
    return this.fetchItems(this.api.availableQuestionMappings_Query({ eventId: this.eventService.selectedId }));
  }
}