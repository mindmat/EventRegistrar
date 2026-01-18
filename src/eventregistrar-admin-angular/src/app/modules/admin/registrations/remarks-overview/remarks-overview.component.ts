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
  filteredRemarksUnprocessed: RemarksDisplayItem[];
  filteredRemarksProcessed: RemarksDisplayItem[];
  totalRemarkCount: number;
  sectionTags: { id: string; name: string; count: number; }[] = [];
  selectedSectionIds: string[] = [];
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private changeDetectorRef: ChangeDetectorRef,
    private remarksService: RemarksOverviewService,
    public navigator: NavigatorService) { }

  ngOnInit(): void
  {
    // Get the remarks
    this.remarksService.remarks$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((remarks: RemarksDisplayItem[]) =>
      {
        this.remarksUnprocessed = remarks.filter(rmk => !rmk.processed);
        this.remarksProcessed = remarks.filter(rmk => !!rmk.processed);
        this.totalRemarkCount = remarks.length;
        this.generateSectionTags(remarks);
        this.applyTagFilter();

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  processedChanged(remarkId: string, processed: boolean): void
  {
    this.remarksService.setProcessedState(remarkId, processed);
  }

  onTagsSelectionChange(selectedIds: string[]): void
  {
    // Filter out empty string (All chip) if present
    this.selectedSectionIds = selectedIds.filter(id => id !== '');
    this.applyTagFilter();
    this.changeDetectorRef.markForCheck();
  }

  toggleSectionFilter(sectionId: string): void
  {
    const index = this.selectedSectionIds.indexOf(sectionId);
    if (index > -1)
    {
      this.selectedSectionIds.splice(index, 1);
    }
    else
    {
      this.selectedSectionIds.push(sectionId);
    }
    this.applyTagFilter();
    this.changeDetectorRef.markForCheck();
  }

  clearSectionFilter(): void
  {
    this.selectedSectionIds = [];
    this.applyTagFilter();
    this.changeDetectorRef.markForCheck();
  }

  private generateSectionTags(remarks: RemarksDisplayItem[]): void
  {
    const sectionCounts = new Map<string, number>();

    // Count unprocessed remarks per section
    this.remarksUnprocessed.forEach(remark =>
    {
      if (remark.section && remark.section.trim())
      {
        const section = remark.section.trim();
        sectionCounts.set(section, (sectionCounts.get(section) || 0) + 1);
      }
    });

    this.sectionTags = Array.from(sectionCounts.entries())
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([section, count]) => ({
        id: section,
        name: section,
        count: count
      }));
  }

  private applyTagFilter(): void
  {
    if (!this.selectedSectionIds || this.selectedSectionIds.length === 0)
    {
      // Show all unprocessed remarks when no tags are selected
      this.filteredRemarksUnprocessed = [...(this.remarksUnprocessed || [])];
    }
    else
    {
      // Filter unprocessed remarks by selected section tags
      this.filteredRemarksUnprocessed = (this.remarksUnprocessed || [])
        .filter(remark => this.selectedSectionIds.includes(remark.section || ''));
    }

    // Processed remarks are always shown in full (no filtering applied)
    this.filteredRemarksProcessed = [...(this.remarksProcessed || [])];
  }
}
