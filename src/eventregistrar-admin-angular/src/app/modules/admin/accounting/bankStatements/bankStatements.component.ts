import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { BookingsOfDay, CreditDebit } from 'app/api/api';
import { BehaviorSubject, combineLatest, debounceTime, Subject, takeUntil } from 'rxjs';
import { EventService } from '../../events/event.service';
import { BankStatementsService } from './bankStatements.service';

@Component({
  selector: 'app-bankStatements',
  templateUrl: './bankStatements.component.html'
})
export class BankStatementsComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  bookingDays: BookingsOfDay[];
  filteredBookingDays: BookingsOfDay[];
  CreditDebit = CreditDebit;
  filters: {
    // categoryTag$: BehaviorSubject<string>;
    query$: BehaviorSubject<string>;
    // hideCompleted$: BehaviorSubject<boolean>;
  } = {
      // categoryTag$: new BehaviorSubject('all'),
      query$: new BehaviorSubject(''),
      // hideCompleted$: new BehaviorSubject(false)
    };
  uploadUrl: string;

  constructor(private service: BankStatementsService,
    private eventService: EventService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.eventService.selected$.subscribe(event => this.uploadUrl = `api/events/${event.acronym}/paymentfiles/upload`);

    this.service.payments$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((bookings: BookingsOfDay[]) =>
      {
        this.bookingDays = bookings;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    // Filter
    combineLatest([this.filters.query$]).pipe(debounceTime(200))
      .subscribe(([query]) =>
      {
        query = query.toLowerCase();

        // Reset
        this.filteredBookingDays = this.bookingDays;

        // // Filter by category
        // if (categoryTag !== 'all')
        // {
        //   this.filteredSingleRegistrables = this.filteredSingleRegistrables.filter(course => course.tag === categoryTag);
        //   this.filteredDoubleRegistrables = this.filteredDoubleRegistrables.filter(course => course.tag === categoryTag);
        // }

        // Filter by search query
        if (query !== '')
        {
          this.filteredBookingDays = this.filteredBookingDays.map(day =>
          ({
            ...day,
            bookings: day.bookings.filter(bok => bok.debitorName?.toLowerCase().includes(query)
              || bok.creditorName?.toLowerCase().includes(query)
              || bok.creditorIban?.toLowerCase().includes(query)
              || bok.message?.toLowerCase().includes(query))
          } as BookingsOfDay))
            .filter(day => day.bookings.length > 0);
        };


        // // Filter by completed
        // if (hideCompleted)
        // {
        //   // this.filteredCourses = this.filteredCourses.filter(course => course.progress.completed === 0);
        // }
      });
  }

  filterByQuery(query: string): void
  {
    this.filters.query$.next(query);
  }
}
