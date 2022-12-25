import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { NotesDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { NotesOverviewService } from './notes-overview.service';

@Component({
  selector: 'app-notes-overview',
  templateUrl: './notes-overview.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotesOverviewComponent implements OnInit
{
  notesList: NotesDisplayItem[];
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private changeDetectorRef: ChangeDetectorRef,
    private notesService: NotesOverviewService,
    public navigator: NavigatorService) { }

  ngOnInit(): void
  {
    this.notesService.notes$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((notes: NotesDisplayItem[]) =>
      {
        this.notesList = notes;

        this.changeDetectorRef.markForCheck();
      });
  }
}
