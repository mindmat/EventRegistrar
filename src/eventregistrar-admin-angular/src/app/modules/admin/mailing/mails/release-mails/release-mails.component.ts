import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { PendingMailListItem } from 'app/api/api';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { ReleaseMailsService } from './release-mails.service';

@Component({
  selector: 'app-release-mails',
  templateUrl: './release-mails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReleaseMailsComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  pendingMails: PendingMailListItem[];
  filteredPendingMails: PendingMailListItem[];
  selectedMail: PendingMailListItem;
  query$: BehaviorSubject<string | null> = new BehaviorSubject(null);

  constructor(private service: ReleaseMailsService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    combineLatest([this.query$, this.service.pendingMails$])
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe(([query, mails]) =>
      {
        this.pendingMails = mails;
        this.filteredPendingMails = mails;

        // Filter by search query
        if (!!query)
        {
          this.filteredPendingMails = this.pendingMails.filter(
            mail => mail.recipientsEmails?.toLowerCase().includes(query.toLowerCase())
              || mail.recipientsNames?.toLowerCase().includes(query.toLowerCase())
              || mail.subject?.toLowerCase().includes(query.toLowerCase()));
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  filterChats(query: string)
  {
    this.query$.next(query);
  }

  onMailSelected(mail: PendingMailListItem)
  {
    this.selectedMail = mail;
  }

  releaseAll()
  {
    this.service.releaseMails(this.filteredPendingMails.map(mail => mail.id));
  }

  deleteAll()
  {
    this.service.deleteMails(this.filteredPendingMails.map(mail => mail.id));
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }
}
