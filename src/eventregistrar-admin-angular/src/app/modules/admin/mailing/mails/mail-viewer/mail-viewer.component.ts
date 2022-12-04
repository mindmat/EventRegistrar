import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MailView } from 'app/api/api';
import { MailViewerService } from './mail-viewer.service';

@Component({
  selector: 'app-mail-viewer',
  templateUrl: './mail-viewer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MailViewerComponent implements OnInit
{
  public mail: MailView;

  constructor(private mailViewerService: MailViewerService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.mailViewerService.mail$
      .subscribe(mail =>
      {
        this.mail = mail;
        this.changeDetectorRef.markForCheck();
      });
  }

}
