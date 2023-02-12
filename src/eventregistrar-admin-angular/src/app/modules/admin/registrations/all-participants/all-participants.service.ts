import { Inject, Injectable, Optional } from '@angular/core';
import { Api, RegistrationState, Participant, API_BASE_URL } from 'app/api/api';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { EventService } from '../../events/event.service';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AllParticipantsService
{
  private list: BehaviorSubject<Participant[] | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService,
    @Inject(HttpClient) private http: HttpClient,
    @Optional() @Inject(API_BASE_URL) private baseUrl?: string) { }

  get list$(): Observable<Participant[]>
  {
    return this.list.asObservable();
  }

  fetchItemsOf(searchString: string): Observable<Participant[]>
  {
    return this.api.participantsOfEvent_Query({ eventId: this.eventService.selectedId, searchString, states: [RegistrationState.Received, RegistrationState.Paid] })
      .pipe(
        tap(newItems => this.list.next(newItems))
      );
  }

  downloadXlsx()
  {
    const url = this.baseUrl + "/api/ParticipantsOfEventQuery";
    const formatXlsx = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
    this.http.post(url, { eventId: this.eventService.selectedId }, { responseType: "blob", headers: { 'Accept': formatXlsx } }).subscribe((file: Blob) =>
    {
      const blob = new Blob([file], { type: formatXlsx });
      const anchor = window.document.createElement('a');
      anchor.href = window.URL.createObjectURL(blob);
      anchor.download = 'participants.xlsx';
      document.body.appendChild(anchor);
      anchor.click();
      document.body.removeChild(anchor);
      window.URL.revokeObjectURL(anchor.href);
    });
  }
}
