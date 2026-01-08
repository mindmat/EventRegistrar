import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { EventSetupState } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../navigator.service';
import { UnprocessedRawRegistrationsService } from '../registrations/unprocessed-raw-registrations.service';
import { SetupEventService } from './setup-event.service';

@Component({
  selector: 'app-setup-event',
  templateUrl: './setup-event.component.html',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    MatButtonModule,
    MatIconModule,
    RouterModule
  ]
})
export class SetupEventComponent implements OnInit
{
  state: EventSetupState;
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(public navigator: NavigatorService,
    private setupEventService: SetupEventService,
    private unprocessedRawRegistrationsService: UnprocessedRawRegistrationsService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.setupEventService.state$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((state: EventSetupState) =>
      {
        this.state = state;
        this.changeDetectorRef.markForCheck();
      });
  }

  downloadGoogleFormsScript(): void
  {
    this.setupEventService.copyScriptToClipboard();
  }

  startProcessAllPendingRawRegistrationsCommand(): void
  {
    this.unprocessedRawRegistrationsService.startProcessAllPendingRawRegistrationsCommand();
  }
}
