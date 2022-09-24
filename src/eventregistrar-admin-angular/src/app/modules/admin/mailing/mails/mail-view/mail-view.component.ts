import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MailView } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { MailViewService } from './mail-view.service';

@Component({
  selector: 'app-mail-view',
  templateUrl: './mail-view.component.html'
})
export class MailViewComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  mail: MailView;

  constructor(private service: MailViewService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.mail$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((mail: MailView) =>
      {
        this.mail = mail;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  release(mailId: string)
  {
    this.service.releaseMail(mailId);
  }
}