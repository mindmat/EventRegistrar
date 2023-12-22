import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { SetupEventService } from './setup-event.service';
import { Subject, takeUntil } from 'rxjs';
import { EventSetupState } from 'app/api/api';
import { NavigatorService } from '../navigator.service';

@Component({
  selector: 'app-setup-event',
  templateUrl: './setup-event.component.html'
})
export class SetupEventComponent implements OnInit
{
  state: EventSetupState;
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(public navigator: NavigatorService,
    private setupEventService: SetupEventService,
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
}
