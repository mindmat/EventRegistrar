import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { OverviewService } from './overview.service';
import { RegistrableTagDisplayItem } from '../registrables/tags/registrableTagDisplayItem';
import { DoubleRegistrableDisplayItem, EventState, PaymentOverview, PricePackageOverview, RegistrablesOverview, SingleRegistrableDisplayItem } from 'app/api/api';
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

@Component({
    selector: 'app-overview',
    templateUrl: './overview.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
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
    accountBalanceOptions: ApexOptions;
    pricePackageOverview: PricePackageOverview;

    filters: {
        categoryTag$: BehaviorSubject<string>;
        query$: BehaviorSubject<string>;
        hideCompleted$: BehaviorSubject<boolean>;
    } = {
            categoryTag$: new BehaviorSubject('all'),
            query$: new BehaviorSubject(''),
            hideCompleted$: new BehaviorSubject(false)
        };

    private unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(private changeDetectorRef: ChangeDetectorRef,
        private overviewService: OverviewService,
        private paymentOverviewService: PaymentOverviewService,
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
                this.singleRegistrables = this.filteredSingleRegistrables = registrables.singleRegistrables;
                this.doubleRegistrables = this.filteredDoubleRegistrables = registrables.doubleRegistrables;

                // Mark for check
                this.changeDetectorRef.markForCheck();
            });

        this.paymentOverviewService.paymentOverview$
            .pipe(takeUntil(this.unsubscribeAll))
            .subscribe((paymentOverview: PaymentOverview) =>
            {
                this.paymentOverview = paymentOverview;
                this.accountBalanceOptions.series = [{
                    name: this.translateService.instant('Balance'),
                    data: paymentOverview.balanceHistory.map(blc =>
                    ({
                        x: blc.date,
                        y: blc.balance
                    }))
                }],

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
        combineLatest([this.filters.categoryTag$, this.filters.query$, this.filters.hideCompleted$])
            .subscribe(([categoryTag, query, hideCompleted]) =>
            {
                // Reset the filtered courses
                this.filteredSingleRegistrables = this.singleRegistrables;
                this.filteredDoubleRegistrables = this.doubleRegistrables;

                // Filter by category
                if (categoryTag !== 'all')
                {
                    this.filteredSingleRegistrables = this.filteredSingleRegistrables.filter(course => course.tag === categoryTag);
                    this.filteredDoubleRegistrables = this.filteredDoubleRegistrables.filter(course => course.tag === categoryTag);
                }

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
        this.filters.categoryTag$.next(change.value);
    }

    toggleCompleted(change: MatSlideToggleChange): void
    {
        this.filters.hideCompleted$.next(change.checked);
    }

    changeDoubleRegistrable(doubleRegistrable: DoubleRegistrableDisplayItem)
    {
        this.matDialog.open(RegistrableDetailComponent, {
            autoFocus: true,
            data: { singleRegistrable: null, doubleRegistrable }
        });
    }

    changeSingleRegistrable(singleRegistrable: SingleRegistrableDisplayItem)
    {
        this.matDialog.open(RegistrableDetailComponent, {
            autoFocus: true,
            data: { singleRegistrable, doubleRegistrable: null }
        });
    }

    openRegistration()
    {
        this.overviewService.openRegistration(true);
    }

    deleteTestData()
    {
        this.overviewService.deleteTestData();
    }

    deleteRegistrable(registrableId: string)
    {
        this.registrableService.deleteRegistrable(registrableId);
    }

    updateView()
    {
        this.overviewService.triggerUpdate();
    }

    trackByFn(index: number, item: any): any
    {
        return item.id || index;
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
            colors: ['#A3BFFA', '#667EEA'],
            fill: {
                colors: ['#CED9FB', '#AECDFD'],
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
                type: 'datetime'
            }
        };
    }
}
