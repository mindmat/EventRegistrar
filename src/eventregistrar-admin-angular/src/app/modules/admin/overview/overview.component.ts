import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { OverviewService } from './overview.service';
import { RegistrableTagDisplayItem } from '../registrables/tags/registrableTagDisplayItem';
import { DoubleRegistrableDisplayItem, EventState, PaymentOverview, PricePackageOverview, RegistrablesOverview, RegistrationsPerDay, SingleRegistrableDisplayItem } from 'app/api/api';
import { MatSelectChange } from '@angular/material/select';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { MatDialog } from '@angular/material/dialog';
import { RegistrableDetailComponent } from './registrable-detail/registrable-detail.component';
import { RegistrablesService } from '../pricing/registrables.service';
import { PaymentOverviewService } from './payment-overview.service';
import { ApexOptions } from 'ng-apexcharts';
import { DateTime } from 'luxon';
import { TranslateService } from '@ngx-translate/core';
import { NavigatorService } from '../navigator.service';
import { PricePackagesOverviewService } from './price-packages-overview.service';
import { EventService } from '../events/event.service';
import { RegistrationsPerDayService } from './registrations-per-day.service';
import { RegistrableIcsComponent } from '../registrables/registrable-ics/registrable-ics.component';

@Component({
    selector: 'app-overview',
    templateUrl: './overview.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class OverviewComponent implements OnInit, OnDestroy
{
    EventState = EventState;
    tags: RegistrableTagDisplayItem[];
    singleRegistrables: SingleRegistrableDisplayItem[];
    doubleRegistrables: DoubleRegistrableDisplayItem[];
    filteredSingleRegistrables: SingleRegistrableDisplayItem[];
    filteredDoubleRegistrables: DoubleRegistrableDisplayItem[];
    paymentOverview: PaymentOverview;
    registrationsPerDay: RegistrationsPerDay[];
    accountBalanceOptions: ApexOptions;
    pricePackageOverview: PricePackageOverview;

    filters: {
        categoryTags$: BehaviorSubject<string[]>;
        query$: BehaviorSubject<string>;
        hideCompleted$: BehaviorSubject<boolean>;
    } = {
            categoryTags$: new BehaviorSubject([]),
            query$: new BehaviorSubject(''),
            hideCompleted$: new BehaviorSubject(false)
        };

    private unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(private changeDetectorRef: ChangeDetectorRef,
        private overviewService: OverviewService,
        private paymentOverviewService: PaymentOverviewService,
        private registrationsPerDayService: RegistrationsPerDayService,
        private pricePackagesOverviewService: PricePackagesOverviewService,
        private registrableService: RegistrablesService,
        private matDialog: MatDialog,
        private translateService: TranslateService,
        public navigator: NavigatorService,
        public eventService: EventService) { }

    ngOnInit(): void
    {
        this.prepareChartData();

        // Get the tags
        this.overviewService.registrableTags$
            .pipe(takeUntil(this.unsubscribeAll))
            .subscribe((tags: RegistrableTagDisplayItem[]) =>
            {
                this.tags = tags;

                // Mark for check
                this.changeDetectorRef.markForCheck();
            });

        // Get the registrables
        this.overviewService.registrables$
            .pipe(takeUntil(this.unsubscribeAll))
            .subscribe((registrables: RegistrablesOverview) =>
            {
                this.singleRegistrables = registrables.singleRegistrables;
                this.doubleRegistrables = registrables.doubleRegistrables;

                // Trigger filtering with current filter values
                this.applyFilters();

                // Mark for check
                this.changeDetectorRef.markForCheck();
            });

        combineLatest([this.paymentOverviewService.paymentOverview$, this.registrationsPerDayService.registrationsPerDay$])
            .pipe(takeUntil(this.unsubscribeAll))
            .subscribe(([paymentOverview, days]) =>
            {
                this.paymentOverview = paymentOverview;
                this.registrationsPerDay = days;
                this.accountBalanceOptions.series = [{
                    name: this.translateService.instant('Balance'),
                    type: 'area',
                    data: paymentOverview.balanceHistory.map(blc =>
                    ({
                        x: blc.date,
                        y: blc.balance
                    }))
                },
                {
                    name: this.translateService.instant('Registrations'),
                    type: 'column',
                    data: days.map(blc =>
                    ({
                        x: blc.date,
                        y: blc.countActive + blc.countCancelled
                    }))
                }];

                // Mark for check
                this.changeDetectorRef.markForCheck();
            });

        this.pricePackagesOverviewService.pricePackageOverview$
            .pipe(takeUntil(this.unsubscribeAll))
            .subscribe((pricePackageOverview: PricePackageOverview) =>
            {
                this.pricePackageOverview = pricePackageOverview;
                this.changeDetectorRef.markForCheck();
            });

        // Filter the courses
        combineLatest([this.filters.categoryTags$, this.filters.query$, this.filters.hideCompleted$])
            .subscribe(([categoryTags, query, hideCompleted]) =>
            {
                this.applyFilters();

                // Mark for check
                this.changeDetectorRef.markForCheck();
            });
    }

    addRegistrable(): void
    {
        this.matDialog.open(RegistrableDetailComponent, {
            autoFocus: true,
            data: { singleRegistrable: null, doubleRegistrable: null }
        });
    }

    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this.unsubscribeAll.next(null);
        this.unsubscribeAll.complete();
    }

    filterByQuery(query: string): void
    {
        this.filters.query$.next(query);
    }

    filterByCategory(change: MatSelectChange): void
    {
        const selectedTags = change.value as string[];
        this.filters.categoryTags$.next(selectedTags || []);
    }

    toggleCompleted(change: MatSlideToggleChange): void
    {
        this.filters.hideCompleted$.next(change.checked);
    }

    changeDoubleRegistrable(doubleRegistrable: DoubleRegistrableDisplayItem): void
    {
        this.matDialog.open(RegistrableDetailComponent, {
            autoFocus: true,
            data: { singleRegistrable: null, doubleRegistrable }
        });
    }

    changeSingleRegistrable(singleRegistrable: SingleRegistrableDisplayItem): void
    {
        this.matDialog.open(RegistrableDetailComponent, {
            autoFocus: true,
            data: { singleRegistrable, doubleRegistrable: null }
        });
    }

    changeRegistrableIcs(icsId: string, registrableId: string, name: string): void
    {
        this.matDialog.open(RegistrableIcsComponent, {
            autoFocus: true,
            data: { icsId, registrableId, name }
        });
    }

    addRegistrableIcs(registrableId: string, name: string): void
    {
        this.matDialog.open(RegistrableIcsComponent, {
            autoFocus: true,
            data: { registrableId, name }
        });
    }

    openRegistration(): void
    {
        this.overviewService.openRegistration(true);
    }

    deleteTestData(): void
    {
        this.overviewService.deleteTestData();
    }

    deleteRegistrable(registrableId: string): void
    {
        this.registrableService.deleteRegistrable(registrableId);
    }

    updateView(): void
    {
        this.overviewService.triggerUpdate();
    }

    trackByFn(index: number, item: any): any
    {
        return item.id || index;
    }

    private applyFilters(): void
    {
        // Don't filter if source data is not loaded yet
        if (!this.singleRegistrables || !this.doubleRegistrables)
        {
            return;
        }

        const categoryTags = this.filters.categoryTags$.value;
        const query = this.filters.query$.value;
        const hideCompleted = this.filters.hideCompleted$.value;

        // Reset the filtered courses
        this.filteredSingleRegistrables = [...this.singleRegistrables];
        this.filteredDoubleRegistrables = [...this.doubleRegistrables];

        // Filter by category - only show items that match selected tags
        if (categoryTags.length > 0)
        {
            this.filteredSingleRegistrables = this.filteredSingleRegistrables.filter(rbl => categoryTags.includes(rbl.tag));
            this.filteredDoubleRegistrables = this.filteredDoubleRegistrables.filter(rbl => categoryTags.includes(rbl.tag));
        }
        // If no tags selected, show all

        // Filter by search query
        if (query !== '')
        {
            this.filteredSingleRegistrables = this.filteredSingleRegistrables.filter(
                rbl => rbl.name.toLowerCase().includes(query.toLowerCase())
                    || rbl.nameSecondary?.toLowerCase().includes(query.toLowerCase()));
            this.filteredDoubleRegistrables = this.filteredDoubleRegistrables.filter(
                rbl => rbl.name.toLowerCase().includes(query.toLowerCase())
                    || rbl.nameSecondary?.toLowerCase().includes(query.toLowerCase()));
        }

        // Filter by completed
        if (hideCompleted)
        {
            // this.filteredCourses = this.filteredCourses.filter(course => course.progress.completed === 0);
        }
    }

    private prepareChartData(): void
    {
        const now = DateTime.now();

        // Account balance
        this.accountBalanceOptions = {
            chart: {
                animations: {
                    speed: 400,
                    animateGradually: {
                        enabled: false
                    }
                },
                fontFamily: 'inherit',
                foreColor: 'inherit',
                width: '100%',
                height: '100%',
                type: 'area',
                sparkline: {
                    enabled: true
                }
            },
            colors: ['#A3BFFA', '#00E396'],
            fill: {
                colors: ['#CED9FB', '#00E396'],
                opacity: 0.5,
                type: 'solid'
            },
            stroke: {
                curve: 'straight',
                width: 2
            },
            tooltip: {
                followCursor: true,
                theme: 'dark',
                x: {
                    format: 'dd.MM.yyyy'
                },
                y: {
                    formatter: (value): string => value.toLocaleString()
                }
            },

            xaxis: {
                type: 'datetime',
                tooltip: { enabled: false }
            },
            yaxis: [{
                // title: {
                //     text: this.translateService.instant('Balance')
                // },
                seriesName: this.translateService.instant('Balance')
            },
            {
                // title: {
                //     text: this.translateService.instant('Registrations')
                // },
                opposite: true,
                seriesName: this.translateService.instant('Registrations')
            }],
            // dataLabels: {
            //     enabled: true,
            //     enabledOnSeries: [1],
            //     background: {
            //         borderWidth: 0,
            //     },
            // },

        };
    }
}
