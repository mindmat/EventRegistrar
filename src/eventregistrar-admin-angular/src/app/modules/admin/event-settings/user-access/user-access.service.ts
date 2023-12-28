import { Injectable } from '@angular/core';
import { Api, UserInEventDisplayItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class UserAccessService extends FetchService<UserInEventDisplayItem[]>
{
  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('UsersOfEventQuery', notificationService);
  }

  get usersWithAccess$(): Observable<UserInEventDisplayItem[]>
  {
    return this.result$;
  }

  fetchUsersOfEvent(): Observable<any>
  {
    return this.fetchItems(this.api.usersOfEvent_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}
