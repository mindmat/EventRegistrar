import { Overlay, OverlayRef } from '@angular/cdk/overlay';
import { TemplatePortal } from '@angular/cdk/portal';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild, ViewContainerRef } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MailView } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { MailViewService } from './mail-view.service';

@Component({
  selector: 'app-mail-view',
  templateUrl: './mail-view.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MailViewComponent implements OnInit
{
  @ViewChild('infoDetailsPanelOrigin') private infoDetailsPanelOrigin: MatButton;
  @ViewChild('infoDetailsPanel') private infoDetailsPanel: TemplateRef<any>;
  private unsubscribeAll: Subject<any> = new Subject<any>();
  private overlayRef: OverlayRef;

  mail: MailView;

  constructor(private service: MailViewService,
    private overlay: Overlay,
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
}