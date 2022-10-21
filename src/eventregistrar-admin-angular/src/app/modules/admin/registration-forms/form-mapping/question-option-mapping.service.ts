import { Injectable } from '@angular/core';
import { Api, AvailableQuestionOptionMapping } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class QuestionOptionMappingService extends FetchService<AvailableQuestionOptionMapping[]>
{
  constructor(private api: Api,
    notificationService: NotificationService,
    private eventService: EventService)
  {
    super('AvailableQuestionOptionMappingsQuery', notificationService);
  }

  get questionOptionMappings(): Observable<AvailableQuestionOptionMapping[]>
  {
    return this.result$;
  }

  fetchMappings(): Observable<any>
  {
    return this.fetchItems(this.api.availableQuestionOptionMappings_Query({ eventId: this.eventService.selectedId }));
  }
}