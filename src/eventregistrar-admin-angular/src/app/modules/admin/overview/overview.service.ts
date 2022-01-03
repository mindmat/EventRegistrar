import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { EventService } from '../events/event.service';
import { RegistrableTagDisplayItem } from '../registrables/tags/registrableTagDisplayItem';

@Injectable({
  providedIn: 'root'
})
export class OverviewService
{
  private registrableTags: BehaviorSubject<RegistrableTagDisplayItem[] | null> = new BehaviorSubject(null);
  private singleRegistrables: BehaviorSubject<SingleRegistrable[] | null> = new BehaviorSubject(null);
  private doubleRegistrables: BehaviorSubject<DoubleRegistrable[] | null> = new BehaviorSubject(null);

  constructor(private http: HttpClient, private eventService: EventService) { }

  get registrableTags$(): Observable<RegistrableTagDisplayItem[]>
  {
    return this.registrableTags.asObservable();
  }

  get singleRegistrables$(): Observable<SingleRegistrable[]>
  {
    return this.singleRegistrables.asObservable();
  }

  get doubleRegistrables$(): Observable<DoubleRegistrable[]>
  {
    return this.doubleRegistrables.asObservable();
  }

  fetchRegistrableTags(): Observable<RegistrableTagDisplayItem[]>
  {
    return this.http.get<RegistrableTagDisplayItem[]>(`api/events/${this.eventService.selected}/RegistrableTags`)
      .pipe(
        tap((response: any) =>
        {
          this.registrableTags.next(response);
        })
      );
  }

  fetchSingleRegistrables(): Observable<SingleRegistrable[]>
  {
    return this.http.get<SingleRegistrable[]>(`api/events/${this.eventService.selected}/SingleRegistrableOverview`)
      .pipe(
        tap((response: any) =>
        {
          this.singleRegistrables.next(response);
        })
      );
  }

  fetchDoubleRegistrables(): Observable<DoubleRegistrable[]>
  {
    return this.http.get<DoubleRegistrable[]>(`api/events/${this.eventService.selected}/DoubleRegistrableOverview`)
      .pipe(
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
  nameSecondary?: string;
  isDeletable: boolean;
  hasWaitingList: boolean;
  automaticPromotionFromWaitingList: boolean;
  tag: string;
}

export class DoubleRegistrable extends Registrable
{
  spotsAvailable: number;
  leadersAccepted: number;
  followersAccepted: number;
  leadersOnWaitingList: number;
  followersOnWaitingList: number;
  maximumAllowedImbalance: number;
  class: DoubleSpotState[];
  waitingList: DoubleSpotState[];
}

export class SingleRegistrable extends Registrable
{
  spotsAvailable: number;
  accepted: number;
  onWaitingList: number;
  class: SpotState[];
  waitingList: SpotState[];
}

export class SpotState
{
  available = 1;
  reserved = 2;
  registered = 3;
  paid = 4;
}

export class DoubleSpotState
{
  leader: SpotState;
  follower: SpotState;
  linked: boolean;
}