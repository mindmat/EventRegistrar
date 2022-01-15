import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { RegistrationService, Registration } from './registration.service';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html'
})
export class RegistrationComponent implements OnInit
{
  public registration: Registration;
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private service: RegistrationService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    // Get the participants
    this.service.registration$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registration: Registration) =>
      {
        this.registration = registration;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

}
