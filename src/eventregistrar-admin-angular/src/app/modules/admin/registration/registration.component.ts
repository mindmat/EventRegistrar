import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RegistrationDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { RegistrationService } from './registration.service';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html'
})
export class RegistrationComponent implements OnInit
{
  public registration: RegistrationDisplayItem;
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private service: RegistrationService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    // Get the participants
    this.service.registration$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registration: RegistrationDisplayItem) =>
      {
        this.registration = registration;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }
}
