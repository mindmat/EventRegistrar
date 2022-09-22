import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { PendingMailListItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { ReleaseMailsService } from './release-mails.service';

@Component({
  selector: 'app-release-mails',
  templateUrl: './release-mails.component.html'
})
export class ReleaseMailsComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  pendingMails: PendingMailListItem[];
  selectedTemplate: PendingMailListItem;

  constructor(private service: ReleaseMailsService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.pendingMails$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((pendingMails: PendingMailListItem[]) =>
      {
        this.pendingMails = pendingMails;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

}
