import { ChangeDetectionStrategy, ChangeDetectorRef, OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { RemarksDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { RemarksOverviewService } from './remarks-overview.service';

@Component({
  selector: 'app-remarks-overview',
  templateUrl: './remarks-overview.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RemarksOverviewComponent implements OnInit
{
  remarksUnprocessed: RemarksDisplayItem[];
  remarksProcessed: RemarksDisplayItem[];
  totalRemarkCount: number;
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private changeDetectorRef: ChangeDetectorRef,
    private remarksService: RemarksOverviewService,
    public navigator: NavigatorService) { }

  ngOnInit(): void
  {
    // Get the tags
    this.remarksService.remarks$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((remarks: RemarksDisplayItem[]) =>
      {
        this.remarksUnprocessed = remarks.filter(rmk => !rmk.processed);
        this.remarksProcessed = remarks.filter(rmk => !!rmk.processed);
        this.totalRemarkCount = remarks.length;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  processedChanged(registrationId: string, processed: boolean)
  {
    this.remarksService.setProcessedState(registrationId, processed);
  }
}
