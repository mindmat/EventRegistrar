import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FixRawProcessingService } from './fix-raw-processing.service';
import { Subject, takeUntil } from 'rxjs';
import { ProcessingError, Role } from 'app/api/api';

@Component({
    standalone: false,
  selector: 'app-fix-raw-processing',
  templateUrl: './fix-raw-processing.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FixRawProcessingComponent implements OnInit, OnDestroy
{
  Role = Role;
  errors: ProcessingError[];
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private changeDetectorRef: ChangeDetectorRef,
    private fixService: FixRawProcessingService)
  { }

  ngOnInit(): void
  {
    // Get the errors
    this.fixService.errors$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((errors: ProcessingError[]) =>
      {
        this.errors = errors;
        this.changeDetectorRef.markForCheck();
      });
  }

  fixRole(rawRegistrationId: string, role: Role)
  {
    this.fixService.fixRole(rawRegistrationId, role);
  }

  retry(rawRegistrationId: string)
  {
    this.fixService.retry(rawRegistrationId);
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }

  ngOnDestroy(): void
  {
    // Unsubscribe from all subscriptions
    this.unsubscribeAll.next(null);
    this.unsubscribeAll.complete();
  }
}
