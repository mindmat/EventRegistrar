import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { PotentialPartnerMatch } from 'app/api/api';
import { BehaviorSubject, Subject, takeUntil } from 'rxjs';
import { MatchPartnersService } from './match-partners.service';

@Component({
  selector: 'app-match-partners',
  templateUrl: './match-partners.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MatchPartnersComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  unmatchedRegistrations: PotentialPartnerMatch[];
  selectedUnmatchedRegistration: PotentialPartnerMatch;

  // candidates: AssignmentCandidate[];

  query$: BehaviorSubject<string> = new BehaviorSubject('');

  constructor(private service: MatchPartnersService, private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.unmatchedPartners$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registrations: PotentialPartnerMatch[]) =>
      {
        this.unmatchedRegistrations = registrations;
        if (!this.selectedUnmatchedRegistration && this.unmatchedRegistrations.length > 0)        
        {
          this.selectUnmatchedRegistration(this.unmatchedRegistrations[0]);
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    // this.service.candidates$
    //   .pipe(takeUntil(this.unsubscribeAll))
    //   .subscribe((candidates: AssignmentCandidate[]) =>
    //   {
    //     this.candidates = candidates;

    //     // Mark for check
    //     this.changeDetectorRef.markForCheck();
    //   });
  }

  selectUnmatchedRegistration(unmatchedRegistration: PotentialPartnerMatch)
  {
    this.selectedUnmatchedRegistration = unmatchedRegistration;

    // Mark for check
    this.changeDetectorRef.markForCheck();
  }
}
