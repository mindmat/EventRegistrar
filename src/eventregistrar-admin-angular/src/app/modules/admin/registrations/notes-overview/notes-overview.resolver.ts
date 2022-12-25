import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { NotesOverviewService } from './notes-overview.service';

@Injectable({
  providedIn: 'root'
})
export class NotesOverviewResolver implements Resolve<boolean>
{
  constructor(private notesService: NotesOverviewService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean>
  {
    return this.notesService.fetchNotes();
  }
}
