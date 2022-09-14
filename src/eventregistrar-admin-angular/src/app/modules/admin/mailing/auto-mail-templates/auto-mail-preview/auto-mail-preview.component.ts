import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { AutoMailPreview, RegistrableTagDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { AutoMailPreviewService } from './auto-mail-preview.service';

@Component({
  selector: 'app-auto-mail-preview',
  templateUrl: './auto-mail-preview.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutoMailPreviewComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  preview: AutoMailPreview;

  constructor(private _changeDetectorRef: ChangeDetectorRef, private mailPreviewService: AutoMailPreviewService) { }

  ngOnInit(): void
  {
    this.mailPreviewService.preview$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((preview: AutoMailPreview) =>
      {
        this.preview = preview;

        // Mark for check
        this._changeDetectorRef.markForCheck();
      });
  }

}
