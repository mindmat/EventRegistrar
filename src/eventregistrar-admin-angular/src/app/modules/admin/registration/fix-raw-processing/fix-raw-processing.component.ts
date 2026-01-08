import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FixRawProcessingService } from './fix-raw-processing.service';
import { Subject, takeUntil } from 'rxjs';
import { ProcessingError, Role } from 'app/api/api';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule } from '@ngx-translate/core';
import { TranslateEnumPipe } from '../../infrastructure/translate-enum.pipe';

@Component({
  selector: 'app-fix-raw-processing',
  templateUrl: './fix-raw-processing.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatSelectModule, TranslateModule, TranslateEnumPipe]
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
