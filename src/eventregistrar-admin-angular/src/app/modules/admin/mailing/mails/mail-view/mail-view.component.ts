import { Overlay, OverlayRef } from '@angular/cdk/overlay';
import { TemplatePortal } from '@angular/cdk/portal';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild, ViewContainerRef } from '@angular/core';
import { MatButton, MatButtonModule } from '@angular/material/button';
import { MailAttachmentMetadata, MailView } from 'app/api/api';
import { NavigatorService } from 'app/modules/admin/navigator.service';
import { Subject, takeUntil } from 'rxjs';
import { MailService } from './mail.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { TranslateModule } from '@ngx-translate/core';
import { FuseScrollResetModule } from '@fuse/directives/scroll-reset';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-mail-view',
  templateUrl: './mail-view.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule, MatMenuModule, TranslateModule, FuseScrollResetModule, MatTooltipModule]
})
export class MailViewComponent implements OnInit
{
  @ViewChild('infoDetailsPanelOrigin') private infoDetailsPanelOrigin: MatButton;
  @ViewChild('infoDetailsPanel') private infoDetailsPanel: TemplateRef<any>;
  private unsubscribeAll: Subject<any> = new Subject<any>();
  private overlayRef: OverlayRef;

  mail: MailView;

  constructor(private service: MailService,
    private overlay: Overlay,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef,
    private viewContainerRef: ViewContainerRef) { }

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

  openInfoDetailsPanel(): void
  {
    // Create the overlay
    this.overlayRef = this.overlay.create({
      backdropClass: '',
      hasBackdrop: true,
      scrollStrategy: this.overlay.scrollStrategies.block(),
      positionStrategy: this.overlay.position()
        .flexibleConnectedTo(this.infoDetailsPanelOrigin._elementRef.nativeElement)
        .withFlexibleDimensions(true)
        .withViewportMargin(16)
        .withLockedPosition(true)
        .withPositions([
          {
            originX: 'start',
            originY: 'bottom',
            overlayX: 'start',
            overlayY: 'top'
          },
          {
            originX: 'start',
            originY: 'top',
            overlayX: 'start',
            overlayY: 'bottom'
          },
          {
            originX: 'end',
            originY: 'bottom',
            overlayX: 'end',
            overlayY: 'top'
          },
          {
            originX: 'end',
            originY: 'top',
            overlayX: 'end',
            overlayY: 'bottom'
          }
        ])
    });

    // Create a portal from the template
    const templatePortal = new TemplatePortal(this.infoDetailsPanel, this.viewContainerRef);

    // Attach the portal to the overlay
    this.overlayRef.attach(templatePortal);

    // Subscribe to the backdrop click
    this.overlayRef.backdropClick().subscribe(() =>
    {

      // If overlay exists and attached...
      if (this.overlayRef && this.overlayRef.hasAttached())
      {
        // Detach it
        this.overlayRef.detach();
      }

      // If template portal exists and attached...
      if (templatePortal && templatePortal.isAttached)
      {
        // Detach it
        templatePortal.detach();
      }
    });
  }

  releaseMail(mailId: string)
  {
    this.service.releaseMail(mailId);
  }

  deleteMail(mailId: string)
  {
    this.service.deleteMail(mailId);
  }

  addIcs(mailId: string)
  {
    this.service.addIcs(mailId);
  }

  downloadAttachment(attachment: MailAttachmentMetadata)
  {
    this.service.downloadAttachment(attachment);
  }
}