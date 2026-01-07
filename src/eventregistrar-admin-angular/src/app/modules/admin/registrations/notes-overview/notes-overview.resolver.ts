import { Injectable } from '@angular/core';
import { RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { NotesOverviewService } from './notes-overview.service';

@Injectable({
  providedIn: 'root'
})
export class NotesOverviewResolver 
{
  constructor(private notesService: NotesOverviewService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean>
  {
    return this.notesService.fetchNotes();
  }
}
