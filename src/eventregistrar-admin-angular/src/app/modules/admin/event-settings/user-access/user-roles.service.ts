import { Injectable } from '@angular/core';
import { Api, RoleDescription, UserInEventDisplayItem, UserInEventRole } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';

@Injectable({
  providedIn: 'root'
})
export class UserRolesService extends FetchService<RoleDescription[]>
{
  constructor(private api: Api,
    private eventService: EventService)
  {
    super();
  }

  get roles$(): Observable<RoleDescription[]>
  {
    return this.result$;
  }

  fetchRoles(): Observable<RoleDescription[]>
  {
    return this.fetchItems(this.api.userInEventRoles_Query({}));
  }

  setRoleOfUserInEvent(userId: string, role: UserInEventRole): void
  {
    this.api.setRoleOfUserInEvent_Command({ userId, role, eventId: this.eventService.selectedId })
      .subscribe();
  }

  removeUserFromEvent(userId: string): void
  {
    this.api.removeUserFromEvent_Command({ userId, eventId: this.eventService.selectedId })
      .subscribe();
  }
}
