import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSidenavModule } from '@angular/material/sidenav';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { PotentialPartnerMatch } from 'app/api/api';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { MatchPartnersService } from './match-partners.service';

@Component({
  selector: 'app-match-partners',
  templateUrl: './match-partners.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    MatSidenavModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule
  ]
})
export class MatchPartnersComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  unmatchedRegistrations: PotentialPartnerMatch[];
  filteredUnmatchedRegistrations: PotentialPartnerMatch[];
  selectedUnmatchedRegistration: PotentialPartnerMatch;

  // candidates: AssignmentCandidate[];

  query$: BehaviorSubject<string> = new BehaviorSubject('');

  constructor(private service: MatchPartnersService,
    private router: Router,
    private route: ActivatedRoute,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    combineLatest([this.service.unmatchedPartners$, this.query$])
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe(([registrations, query]) =>
      {
        this.unmatchedRegistrations = registrations;
        this.filteredUnmatchedRegistrations = registrations;
        if (!this.selectedUnmatchedRegistration && this.filteredUnmatchedRegistrations.length > 0)        
        {
          this.selectUnmatchedRegistration(this.filteredUnmatchedRegistrations[0]);
        }
        if (!!query)
        {
          this.filteredUnmatchedRegistrations = this.unmatchedRegistrations.filter(reg =>
            reg.firstName.toLowerCase().includes(query.toLowerCase())
            || reg.lastName?.toLowerCase().includes(query.toLowerCase())
            || reg.declaredPartner?.toLowerCase().includes(query.toLowerCase()));
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  selectUnmatchedRegistration(unmatchedRegistration: PotentialPartnerMatch)
  {
    this.selectedUnmatchedRegistration = unmatchedRegistration;
    this.router.navigate([this.selectedUnmatchedRegistration.registrationId], { relativeTo: this.route });

    // Mark for check
    this.changeDetectorRef.markForCheck();
  }

  filter(search: string)
  {
    this.query$.next(search);
  }
}
