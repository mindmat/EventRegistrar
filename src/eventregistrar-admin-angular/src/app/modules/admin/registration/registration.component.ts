import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { AssignedPaymentDisplayItem, RegistrationDisplayItem, SpotDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { PaymentsService } from './payments/payments.service';
import { RegistrationService } from './registration.service';
import { SpotsService } from './spots/spots.service';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html'
})
export class RegistrationComponent implements OnInit
{
  public registration: RegistrationDisplayItem;
  public spots: SpotDisplayItem[];
  public payments: AssignedPaymentDisplayItem[];
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private service: RegistrationService,
    private spotService: SpotsService,
    private paymentService: PaymentsService,
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

    this.spotService.spots$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((spots: SpotDisplayItem[]) =>
      {
        this.spots = spots;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.paymentService.payments$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((payments: AssignedPaymentDisplayItem[]) =>
      {
        this.payments = payments;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

}
