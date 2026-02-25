import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { RequestLogService } from './request-log.service';
import { EventService } from '../../events/event.service';

@Injectable({
    providedIn: 'root'
})
export class RequestLogResolver implements Resolve<any>
{
    constructor(private readonly service: RequestLogService,
        private readonly eventService: EventService) { }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        return this.service.fetchRequestLog({ eventId: this.eventService.selectedId ?? undefined });
    }
}
