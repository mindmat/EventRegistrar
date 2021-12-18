import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { EventService } from '../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class OverviewService
{
  private singleRegistrables: BehaviorSubject<SingleRegistrable[] | null> = new BehaviorSubject(null);
  private doubleRegistrables: BehaviorSubject<DoubleRegistrable[] | null> = new BehaviorSubject(null);

  constructor(private http: HttpClient,
    private eventService: EventService) { }

  get singleRegistrables$(): Observable<SingleRegistrable[]>
  {
    return this.singleRegistrables.asObservable();
  }

  get doubleRegistrables$(): Observable<DoubleRegistrable[]>
  {
    return this.doubleRegistrables.asObservable();
  }

  fetchSingleRegistrables(): Observable<SingleRegistrable[]>
  {
    return this.http.get<SingleRegistrable[]>(`api/events/${this.eventService.selected}/SingleRegistrableOverview`).pipe(
      tap((response: any) =>
      {
        this.singleRegistrables.next(response);
      })
    );
  }

  fetchDoubleRegistrables(): Observable<DoubleRegistrable[]>
  {
    return this.http.get<DoubleRegistrable[]>(`api/events/${this.eventService.selected}/DoubleRegistrableOverview`).pipe(
      tap((response: any) =>
      {
        this.doubleRegistrables.next(response);
      })
    );
  }
}


export class Registrable
{
  id: string;
  name: string;
  isDeletable: boolean;
  automaticPromotionFromWaitingList: boolean;
}
export class DoubleRegistrable extends Registrable
{
  spotsAvailable: number;
  leadersAccepted: number;
  followersAccepted: number;
  leadersOnWaitingList: number;
  followersOnWaitingList: number;
  maximumAllowedImbalance: number;
}

export class SingleRegistrable extends Registrable
{
  spotsAvailable: number;
  accepted: number;
  onWaitingList: number;
}