import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { RemarksDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { RemarksOverviewService } from './remarks-overview.service';

@Component({
  selector: 'app-remarks-overview',
  templateUrl: './remarks-overview.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    MatButtonModule,
    MatSlideToggleModule
  ]
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

  processedChanged(remarkId: string, processed: boolean): void
  {
    this.remarksService.setProcessedState(remarkId, processed);
  }
}
