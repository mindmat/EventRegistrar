import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { NotesDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { NotesOverviewService } from './notes-overview.service';

@Component({
  selector: 'app-notes-overview',
  templateUrl: './notes-overview.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule
  ]
})
export class NotesOverviewComponent implements OnInit, OnDestroy
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

  ngOnDestroy(): void
  {
    // Unsubscribe from all subscriptions
    this.unsubscribeAll.next(null);
    this.unsubscribeAll.complete();
  }
}
